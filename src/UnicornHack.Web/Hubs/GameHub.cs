using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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
                var serializedLevel = CompactLevel.Serialize(
                    level,
                    level == initialLevel && character.Level.TerrainChanges != null ? EntityState.Modified : EntityState.Added,
                    previousTick,
                    new SerializationContext(_dbContext, player, _gameServices));

                // TODO: only send to clients watching the current player
                await Clients.All.InvokeAsync("ReceiveState", serializedLevel);
            }

            _dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            _dbContext.ChangeTracker.AcceptAllChanges();
        }

        #region Game managment

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
                character = new Player(initialLevel, upStairs.LevelX, upStairs.LevelY) {Name = name};

                character.WriteLog(game.Services.Language.Welcome(character), character.Level.CurrentTick);
                _dbContext.Characters.Add(character);

                // Avoid cyclical dependency
                var attack = character.DefaultAttack;
                character.DefaultAttack = null;
                _dbContext.SaveChanges();

                character.DefaultAttack = attack;
                _dbContext.Entry(character).Property("DefaultAttackId").CurrentValue = attack.Id;
                Turn(character);

                _dbContext.SaveChanges();
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
                .Include(c => c.Game.Random)
                .Include(c => c.Skills)
                //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ActorMoveEvent, Actor>(e => e.Mover)
                //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ActorMoveEvent, Actor>(e => e.Movee)
                .Include(c => c.SensedEvents)
                .ThenInclude<Player, SensoryEvent, AttackEvent, IEnumerable<AppliedEffect>>(e => e.AppliedEffects)
                //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, AttackEvent, Actor>(e => e.Attacker)
                .Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, AttackEvent, Actor>(e => e.Victim)
                .Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, DeathEvent, Actor>(e => e.Deceased)
                //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, DeathEvent, Item>(e => e.Corpse)
                //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ItemConsumptionEvent, Actor>(e => e.Consumer)
                .Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ItemConsumptionEvent, Item>(e => e.Item)
                //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ItemDropEvent, Actor>(e => e.Dropper)
                //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ItemDropEvent, Item>(e => e.Item)
                //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ItemPickUpEvent, Actor>(e => e.Picker)
                //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ItemPickUpEvent, Item>(e => e.Item)
                //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ItemEquipmentEvent, Actor>(e => e.Equipper)
                //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ItemEquipmentEvent, Item>(e => e.Item)
                //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ItemUnequipmentEvent, Actor>(e => e.Unequipper)
                //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ItemUnequipmentEvent, Item>(e => e.Item)
                .Include(c => c.Level)
                .FirstOrDefault(c => c.Name == name);

            if (character?.Level == null)
            {
                return null;
            }

            _dbContext.LogEntries
                .Where(e => e.GameId == character.GameId && e.PlayerId == character.Id)
                .OrderByDescending(e => e.Tick)
                .ThenByDescending(e => e.Id)
                .Take(10)
                .Load();

            Initialize(character.Game);

            _dbContext.LoadLevel(character.Level.GameId, character.Level.BranchName, character.Level.Depth);

            // Preload adjacent level
            var connection = character.Level.Connections.SingleOrDefault(s =>
                s.LevelX == character.LevelX
                && s.LevelY == character.LevelY);
            if (connection != null)
            {
                _dbContext.LoadLevel(connection.GameId, connection.TargetBranchName, connection.TargetLevelDepth);

                // TODO: Pregenerate all connected levels to ensure the order is deterministic
                connection.TargetLevel.EnsureGenerated();
            }

            var gameId = character.GameId;
            var addedAbilities = _dbContext.Set<AddedAbility>().Local.Select(c => c.Id).ToList();
            if (addedAbilities.Count > 0)
            {
                _dbContext.Set<AddedAbility>()
                    .Include(a => a.Ability).ThenInclude(a => a.Triggers)
                    .Include(a => a.Ability).ThenInclude(a => a.Effects)
                    .Where(a => a.GameId == gameId && addedAbilities.Contains(a.Id))
                    .Load();
            }

            var addAbilities = _dbContext.Set<AddAbility>().Local.Select(c => c.Id).ToList();
            if (addAbilities.Count > 0)
            {
                _dbContext.Set<AddAbility>()
                    .Include(a => a.Ability).ThenInclude(a => a.Triggers)
                    .Include(a => a.Ability).ThenInclude(a => a.Effects)
                    .Where(a => a.GameId == gameId && addAbilities.Contains(a.Id))
                    .Load();
            }

            return character;
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
                .Include(g => g.Random)
                .Include(g => g.Branches)
                .Include(g => g.Levels).ThenInclude(l => l.GenerationRandom)
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

        #endregion
    }
}