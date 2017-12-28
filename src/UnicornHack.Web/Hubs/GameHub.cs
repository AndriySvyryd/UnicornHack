using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UnicornHack.Data;
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
            var s = new Stopwatch();
            s.Start();

            var player = FindPlayer(name) ?? CreateGame(name);

            _dbContext.SaveChanges();

            var serializedPlayer = CompactPlayer.Serialize(player, EntityState.Added,
                new SerializationContext(_dbContext, player, _gameServices));

            s.Stop();
            serializedPlayer.Add(s.Elapsed.TotalMilliseconds);

            return serializedPlayer;
        }

        public async Task PerformAction(string name, PlayerAction? action, int? target, int? target2)
        {
            var s = new Stopwatch();
            s.Start();

            var currentPlayer = FindPlayer(name);
            if (currentPlayer == null)
            {
                currentPlayer = CreateGame(name);
                action = null;
                target = null;
                target2 = null;
            }

            if (currentPlayer.Game.NextPlayerTick != currentPlayer.NextActionTick)
            {
                // TODO: Log action sent out of turn
                return;
            }

            currentPlayer.NextAction = action;
            currentPlayer.NextActionTarget = target;
            currentPlayer.NextActionTarget2 = target2;

            var playerLevels = new HashSet<Level>();
            foreach (var player in currentPlayer.Game.Players)
            {
                if (playerLevels.Add(player.Level))
                {
                    player.Level.Snapshot();
                }

                var connection = player.Level.Connections.SingleOrDefault(c => c.LevelCell == player.LevelCell);
                if (connection != null
                    && playerLevels.Add(connection.TargetLevel))
                {
                    connection.TargetLevel.Snapshot();
                }
            }

            currentPlayer.Game.Turn();

            if (!currentPlayer.IsAlive)
            {
                // Show the last events before death
                currentPlayer.Act();
            }

            _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            foreach (var playerLevel in playerLevels)
            {
                playerLevel.DetectVisibilityChanges();

                var levelEntry = _dbContext.Entry(playerLevel);
                if (playerLevel.TerrainChanges == null
                    || playerLevel.TerrainChanges.Count > 0)
                {
                    levelEntry.Property(l => l.Terrain).IsModified = true;
                }

                if (playerLevel.KnownTerrainChanges == null
                    || playerLevel.KnownTerrainChanges.Count > 0)
                {
                    levelEntry.Property(l => l.KnownTerrain).IsModified = true;
                }

                if (playerLevel.WallNeighboursChanges == null
                    || playerLevel.WallNeighboursChanges.Count > 0)
                {
                    levelEntry.Property(l => l.WallNeighbours).IsModified = true;
                }

                if (playerLevel.VisibleTerrainChanges == null
                    || playerLevel.VisibleTerrainChanges.Count > 0)
                {
                    levelEntry.Property(l => l.VisibleTerrain).IsModified = true;
                }

                if (playerLevel.VisibleNeighboursChanged)
                {
                    levelEntry.Property(l => l.VisibleNeighbours).IsModified = true;
                }
            }

            _dbContext.ChangeTracker.DetectChanges();
            _dbContext.SaveChanges(acceptAllChangesOnSuccess: false);

            foreach (var player in currentPlayer.Game.Players)
            {
                var newPlayer = action == null;
                var serializedPlayer = CompactPlayer.Serialize(
                    player, newPlayer ? EntityState.Added : EntityState.Modified,
                    new SerializationContext(_dbContext, player, _gameServices));

                s.Stop();
                if (newPlayer)
                {
                    serializedPlayer.Add(s.Elapsed.TotalMilliseconds);
                }
                else
                {
                    serializedPlayer.Add(10);
                    serializedPlayer.Add(s.Elapsed.TotalMilliseconds);
                }

                // TODO: only send to clients watching this player
                await Clients.All.InvokeAsync("ReceiveState", serializedPlayer);
            }

            _dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            _dbContext.ChangeTracker.AcceptAllChanges();
        }

        private Player CreateGame(string name)
        {
            int seed;
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                var rngData = new byte[4];
                rng.GetBytes(rngData);

                seed = rngData[0] | rngData[1] << 8 | rngData[2] << 16 | rngData[3] << 24;
            }

            seed = 2;
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
            var player = new Player(initialLevel, upStairs.LevelX, upStairs.LevelY) {Name = name, PlayerName = name};

            player.WriteLog(game.Services.Language.Welcome(player), player.Level.CurrentTick);
            _dbContext.Players.Add(player);

            // Avoid cyclical dependency
            var attack = player.DefaultAttack;
            player.DefaultAttack = null;
            _dbContext.SaveChanges();

            player.DefaultAttack = attack;
            player.DefaultAttackId = attack?.Id;

            player.Game.Turn();

            _dbContext.SaveChanges();

            _gameServices.SharedCache.Set(name, player,
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1)));

            return player;
        }

        private Player FindPlayer(string name)
        {
            var player = _dbContext.Players
                .Include(c => c.Game)
                .Include(c => c.Level)
                .AsNoTracking()
                .FirstOrDefault(c => c.PlayerName == name);

            if (player?.Level == null)
            {
                return null;
            }

            // TODO: Use an object as the key
            if (_gameServices.SharedCache.TryGetValue(name, out var cachedPlayerObject)
                && cachedPlayerObject is Player cachedPlayer
                && cachedPlayer.Game.NextPlayerTick == player.Game.NextPlayerTick)
            {
                _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
                EntityAttacher.Attach(cachedPlayer.Level, _dbContext);
                _dbContext.ChangeTracker.AutoDetectChangesEnabled = true;

                LoadAdjacentLevel(cachedPlayer);
                player = cachedPlayer;
            }
            else
            {
                player = _dbContext.LoadPlayer(name);

                Initialize(player.Game);

                _dbContext.LoadLevel(player.Level.GameId, player.Level.BranchName, player.Level.Depth);

                LoadAdjacentLevel(player);

                _gameServices.SharedCache.Set(name, player,
                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1)));
            }

            if (!player.IsAlive
                && !player.Game.Players.Any(pc => pc.IsAlive))
            {
                Clean(player.Game);
                return null;
            }

            return player;
        }

        private void LoadAdjacentLevel(Player character)
        {
            var connection = character.Level.Connections.SingleOrDefault(s =>
                s.LevelCell == character.LevelCell);
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
                // TODO: Don't remove players and game
                _dbContext.Entities.Remove(entity);
            }

            foreach (var connection in game.Connections.ToList())
            {
                _dbContext.Connections.Remove(connection);
            }

            foreach (var level in game.Levels.ToList())
            {
                _dbContext.Levels.Remove(level);
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