using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UnicornHack.Data;
using UnicornHack.Effects;
using UnicornHack.Events;
using UnicornHack.Generation;
using UnicornHack.Models.GameHubModels;
using UnicornHack.Services;
using UnicornHack.Utils;

namespace UnicornHack.Hubs
{
    public class GameHub : Hub
    {
        private readonly GameDbContext _dbContext;
        private readonly GameServices _gameServices;

        public GameHub(GameDbContext dbContext, GameServices gameServices)
        {
            _dbContext = dbContext;
            _gameServices = gameServices;
        }

        public Task SendMessage(string message)
            => Clients.All.InvokeAsync("ReceiveMessage", Context.User.Identity.Name, message);

        public List<object> GetState(string name)
        {
            var player = FindOrCreateCharacter(name);
            return CompactLevel.Serialize(
                player.Level, EntityState.Added, 0, new SerializationContext(_dbContext, player, _gameServices));
        }

        public async Task PerformAction(string name, string action, int? target, int? target2)
        {
            var character = FindOrCreateCharacter(name);
            CompactLevel.Snapshot(character.Level);

            character.NextAction = action;
            character.NextActionTarget = target;
            character.NextActionTarget2 = target2;

            var initialLevel = character.Level;
            var previousTick = initialLevel.CurrentTick;

            Turn(character);

            _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            var levelEntry = _dbContext.Entry(character.Level);
            if (character.Level.TerrainChanges == null
                ||character.Level.TerrainChanges.Count > 0)
            {
                levelEntry.Property(l => l.Terrain).IsModified = true;
            }
            if (character.Level.WallNeighboursChanges == null
                || character.Level.WallNeighboursChanges.Count > 0)
            {
                levelEntry.Property(l => l.WallNeighbours).IsModified = true;
            }
            if (character.Level.VisibleTerrainChanges == null
                || character.Level.VisibleTerrainChanges.Count > 0)
            {
                levelEntry.Property(l => l.VisibleTerrain).IsModified = true;
            }
            if (character.Level.VisibleNeighboursChanged)
            {
                levelEntry.Property(l => l.VisibleNeighbours).IsModified = true;
            }

            _dbContext.ChangeTracker.DetectChanges();
            _dbContext.SaveChanges(acceptAllChangesOnSuccess: false);

            var level = character.Level;
            foreach (var player in level.Players)
            {
                var levelChanged = level != initialLevel
                                   || character.Level.TerrainChanges == null
                                   || previousTick == 0;
                var serializedLevel = CompactLevel.Serialize(
                    level,
                    levelChanged ? EntityState.Added : EntityState.Modified,
                    previousTick,
                    new SerializationContext(_dbContext, player, _gameServices));

                // TODO: only send to clients watching the current player
                await Clients.All.InvokeAsync("ReceiveState", serializedLevel);
            }

            _dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            _dbContext.ChangeTracker.AcceptAllChanges();
        }

        private void Turn(Player character)
        {
            if (character.Game.NextPlayerTick == character.NextActionTick)
            {
                character.Game.Turn();
            }

            if (!character.IsAlive)
            {
                // Show the last events before death
                character.Act();
            }
        }

        private Player FindOrCreateCharacter(string name)
        {
            var character = FindCharacter(name);
            if (character == null)
            {
                int seed;
                using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
                {
                    var rngData = new byte[4];
                    rng.GetBytes(rngData);

                    seed = rngData[0] | rngData[1] << 8 | rngData[2] << 16 | rngData[3] << 24;
                }

                var game = new Game
                {
                    InitialSeed = seed,
                    Random = new SimpleRandom {Seed = seed}
                };
                Initialize(game);
                _dbContext.Games.Add(game);
                _dbContext.SaveChanges();

                var surfaceBranch = BranchDefinition.Loader.Get("surface").Instantiate(game);
                var surfaceLevel = new Level(surfaceBranch, depth: 1, seed: seed);
                surfaceLevel.EnsureGenerated();
                var initialLevel = surfaceLevel.Connections.Single().TargetLevel;
                initialLevel.EnsureGenerated();
                var upStairs = initialLevel.Connections.First(c => c.TargetBranchName == surfaceBranch.Name);
                character = new Player(initialLevel, upStairs.LevelX, upStairs.LevelY) {Name = name, PlayerName = name};

                character.WriteLog(game.Services.Language.Welcome(character), character.Level.CurrentTick);
                _dbContext.Characters.Add(character);

                // Avoid cyclical dependency
                var attack = character.DefaultAttack;
                character.DefaultAttack = null;
                _dbContext.SaveChanges();

                character.DefaultAttack = attack;
                character.DefaultAttackId = attack?.Id;
                Turn(character);

                _dbContext.SaveChanges();

                _gameServices.SharedCache.Set(name, character,
                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1)));
            }

            if (!character.IsAlive
                && !character.Game.Players.Any(pc => pc.IsAlive))
            {
                Clean(character.Game);
                character = FindOrCreateCharacter(name);
            }

            return character;
        }

        private Player FindCharacter(string name)
        {
            var character = _dbContext.Characters
                .Include(c => c.Game)
                .Include(c => c.Level)
                .AsNoTracking()
                .FirstOrDefault(c => c.PlayerName == name);

            if (character?.Level == null)
            {
                return null;
            }

            // TODO: Use an object as the key
            if (_gameServices.SharedCache.TryGetValue(name, out var cachedCharacterObject)
                && cachedCharacterObject is Player cachedCharacter
                && cachedCharacter.Game.NextPlayerTick == character.Game.NextPlayerTick)
            {
                _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
                EntityAttacher.Attach(cachedCharacter.Level, _dbContext);
                _dbContext.ChangeTracker.AutoDetectChangesEnabled = true;

                LoadAdjacentLevel(cachedCharacter);
                return cachedCharacter;
            }

            character = _dbContext.LoadPlayer(name);

            Initialize(character.Game);

            _dbContext.LoadLevel(character.Level.GameId, character.Level.BranchName, character.Level.Depth);

            LoadAdjacentLevel(character);

            _gameServices.SharedCache.Set(name, character,
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1)));
            return character;
        }

        private void LoadAdjacentLevel(Player character)
        {
            var connection = character.Level.Connections.SingleOrDefault(s =>
                s.LevelX == character.LevelX
                && s.LevelY == character.LevelY);
            if (connection != null)
            {
                if (connection.TargetLevel == null)
                {
                    _dbContext.LoadLevel(connection.GameId, connection.TargetBranchName, connection.TargetLevelDepth);
                }

                // TODO: Unload non-adjacent levels

                // TODO: Pregenerate all connected levels to ensure the order is deterministic
                connection.TargetLevel.EnsureGenerated();
            }
        }

        private void Initialize(Game game)
        {
            game.Services = _gameServices;
            game.Repository = new Repository(_dbContext);
        }

        private void Clean(Game game)
        {
            // Avoid cyclical dependency
            foreach (var playerCharacter in game.Players)
            {
                playerCharacter.DefaultAttack = null;
            }
            _dbContext.SaveChanges();

            game = _dbContext.Games
                .Include(g => g.Branches)
                .Include(g => g.Levels)
                .Include(g => g.Connections)
                .Include(g => g.Entities)
                .Include(g => g.AbilityDefinitions)
                .Include(g => g.Effects)
                .Include(g => g.Abilities)
                .Include(g => g.Triggers)
                .Include(g => g.AppliedEffects)
                .Include(g => g.SensoryEvents)
                .Single(g => g.Id == game.Id);

            foreach (var playerCharacter in game.Players)
            {
                foreach (var log in playerCharacter.Log.ToList())
                {
                    _dbContext.LogEntries.Remove(log);
                }
            }
            foreach (var effect in game.Effects.ToList())
            {
                _dbContext.Effects.Remove(effect);
            }
            foreach (var appliedEffect in game.AppliedEffects.ToList())
            {
                _dbContext.AppliedEffects.Remove(appliedEffect);
            }
            foreach (var ability in game.Abilities.ToList())
            {
                _dbContext.Abilities.Remove(ability);
            }
            foreach (var trigger in game.Triggers.ToList())
            {
                _dbContext.Triggers.Remove(trigger);
            }
            foreach (var abilityDefinition in game.AbilityDefinitions.ToList())
            {
                _dbContext.AbilityDefinitions.Remove(abilityDefinition);
            }
            foreach (var sensoryEvent in game.SensoryEvents.ToList())
            {
                _dbContext.SensoryEvents.Remove(sensoryEvent);
            }
            foreach (var entity in game.Entities.ToList())
            {
                _dbContext.Entities.Remove(entity);
            }
            foreach (var level in game.Levels.ToList())
            {
                _dbContext.Levels.Remove(level);
            }
            foreach (var stairs in game.Connections.ToList())
            {
                _dbContext.Connections.Remove(stairs);
            }
            foreach (var branch in game.Branches.ToList())
            {
                _dbContext.Branches.Remove(branch);
            }

            _dbContext.Games.Remove(game);
            _dbContext.SaveChanges();
        }
    }
}