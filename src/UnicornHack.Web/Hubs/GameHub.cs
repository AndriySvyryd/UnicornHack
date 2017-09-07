using System;
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

        public Task SendMessage(string message) => Clients.All.InvokeAsync("ReceiveMessage", Context.User.Identity.Name, message);

        public async Task<CompactLevel> GetState(string name)
            => CompactLevel.Serialize((await FindOrCreateCharacterAsync(name)).Level, _dbContext, _gameServices);

        public async Task PerformAction(string name, string action, int? target, int? target2)
        {
            var character = await FindOrCreateCharacterAsync(name);

            character.NextAction = action;
            character.NextActionTarget = target;
            character.NextActionTarget2 = target2;

            await TurnAsync(character);
            await Clients.All.InvokeAsync("ReceiveState", CompactLevel.Serialize(character.Level, _dbContext, _gameServices));
        }

        #region Game managment

        private async Task TurnAsync(Player character)
        {
            var level = character.Level;
            if (character.Game.NextPlayerTick == character.NextActionTick)
            {
                character.Game.Turn();
            }

            if (!character.IsAlive)
            {
                // Show the last events before death
                character.Act();
            }

            // TODO: Move this
            _dbContext.Entry(level).Property(l => l.VisibleTerrain).IsModified = true;

            await _dbContext.SaveChangesAsync();

            // Level is null if the character is dead
            if (!character.IsAlive)
            {
                character.Level = level;
            }
        }

        private async Task<Player> FindOrCreateCharacterAsync(string name)
        {
            var character = FindCharacter(name);
            if (character == null)
            {
                int seed;
                using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
                {
                    byte[] rngData = new byte[4];
                    rng.GetBytes(rngData);

                    seed = rngData[0] | rngData[1] << 8 | rngData[2] << 16 | rngData[3] << 24;
                }

                var game = new Game
                {
                    InitialSeed = seed,
                    Random = new SimpleRandom { Seed = seed }
                };
                Initialize(game);
                _dbContext.Games.Add(game);
                await _dbContext.SaveChangesAsync();

                var surfaceBranch = BranchDefinition.Loader.Get("surface").Instantiate(game);
                var surfaceLevel = new Level(surfaceBranch, depth: 1, seed: seed);
                surfaceLevel.EnsureGenerated();
                var initialLevel = surfaceLevel.Connections.Single().TargetLevel;
                initialLevel.EnsureGenerated();
                var upStairs = initialLevel.Connections.First(c => c.TargetBranchName == surfaceBranch.Name);
                character = new Player(initialLevel, upStairs.LevelX, upStairs.LevelY) { Name = name };

                character.WriteLog(game.Services.Language.Welcome(character), character.Level.CurrentTick);
                _dbContext.Characters.Add(character);

                // Avoid cyclical dependency
                var attack = character.DefaultAttack;
                character.DefaultAttack = null;
                await _dbContext.SaveChangesAsync();

                character.DefaultAttack = attack;
                await TurnAsync(character);
                await _dbContext.SaveChangesAsync();
            }

            if (!character.IsAlive
                && !character.Game.Players.Any(pc => pc.IsAlive))
            {
                Clean(character.Game);
                _dbContext.SaveChanges();
                character = await FindOrCreateCharacterAsync(name);
            }

            return character;
        }

        // TODO: make async
        private Player FindCharacter(string name)
        {
            var character = _dbContext.Characters
                .FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (character == null)
            {
                return null;
            }

            _dbContext.Characters
                .Include(c => c.Game.Random)
                .Include(c => c.Log)
                .Include(c => c.Abilities).ThenInclude(a => a.Effects)
                .Include(c => c.ActiveEffects).ThenInclude(e => e.SourceAbility)
                .Include(c => c.Properties)
                .Include(c => c.Skills)
                .Include(c => c.SensedEvents)
                .Include(c => c.Level.Connections)
                .Include(c => c.Level.IncomingConnections)
                .Include(c => c.Level.Game)
                .Include(c => c.Level.GenerationRandom)
                .Where(c => c.GameId == character.GameId)
                .Load();

            if (character.Level == null)
            {
                return character;
            }

            Initialize(character.Game);

            LoadLevel(character.Level.GameId, character.Level.BranchName, character.Level.Depth);

            // Preload adjacent level
            var connection = character.Level.Connections.SingleOrDefault(s =>
                s.LevelX == character.LevelX
                && s.LevelY == character.LevelY);
            if (connection != null)
            {
                LoadLevel(connection.GameId, connection.TargetBranchName, connection.TargetLevelDepth);
            }

            // TODO: Pregenerate all connected levels to ensure the order
            connection?.TargetLevel.EnsureGenerated();

            LoadEvents(character);

            var gameId = character.GameId;
            var loadedContainerIds = _dbContext.Set<Container>().Local.Select(c => c.Id).ToList();
            _dbContext.Set<Container>()
                .Include(c => c.Items).ThenInclude(i => i.Abilities).ThenInclude(a => a.Effects)
                .Where(i => i.GameId == gameId && loadedContainerIds.Contains(i.Id))
                .Load();

            var meleeAttacks = _dbContext.Set<MeleeAttack>().Local.Select(c => c.Id).ToList();
            if (meleeAttacks.Any())
            {
                _dbContext.Set<MeleeAttack>()
                    .Include(e => e.Weapon.Abilities).ThenInclude(a => a.Effects)
                    .Where(e => e.GameId == gameId && meleeAttacks.Contains(e.Id))
                    .Load();
            }

            var rangeAttacks = _dbContext.Set<RangeAttack>().Local.Select(c => c.Id).ToList();
            if (rangeAttacks.Any())
            {
                _dbContext.Set<RangeAttack>()
                    .Include(e => e.Weapon.Abilities).ThenInclude(a => a.Effects)
                    .Where(e => e.GameId == gameId && rangeAttacks.Contains(e.Id))
                    .Load();
            }

            var addedAbilities = _dbContext.Set<AddedAbility>().Local.Select(c => c.Id).ToList();
            if (addedAbilities.Any())
            {
                _dbContext.Set<AddedAbility>()
                    .Include(a => a.Ability).ThenInclude(a => a.Effects)
                    .Where(a => a.GameId == gameId && addedAbilities.Contains(a.Id))
                    .Load();
            }

            var addAbilities = _dbContext.Set<AddAbility>().Local.Select(c => c.Id).ToList();
            if (addAbilities.Any())
            {
                _dbContext.Set<AddAbility>()
                    .Include(a => a.Ability).ThenInclude(a => a.Effects)
                    .Where(a => a.GameId == gameId && addAbilities.Contains(a.Id))
                    .Load();
            }

            return character;
        }

        private void LoadLevel(int gameId, string branchName, byte depth)
        {
            _dbContext.Levels
                .Include(l => l.Items).ThenInclude(i => i.Abilities).ThenInclude(a => a.Effects)
                .Include(l => l.Items).ThenInclude(i => i.ActiveEffects).ThenInclude(i => i.SourceAbility)
                .Include(l => l.Items).ThenInclude(i => i.Properties)
                .Include(l => l.Actors).ThenInclude(a => a.Inventory).ThenInclude(i => i.Abilities)
                .ThenInclude(a => a.Effects)
                .Include(l => l.Actors).ThenInclude(a => a.Inventory).ThenInclude(i => i.ActiveEffects)
                .ThenInclude(i => i.SourceAbility)
                .Include(l => l.Actors).ThenInclude(a => a.Inventory).ThenInclude(i => i.Properties)
                .Include(l => l.Actors).ThenInclude(a => a.Abilities).ThenInclude(a => a.Effects)
                .Include(l => l.Actors).ThenInclude(a => a.ActiveEffects).ThenInclude(i => i.SourceAbility)
                .Include(l => l.Actors).ThenInclude(a => a.Properties)
                .Include(l => l.Connections).ThenInclude(c => c.TargetBranch)
                .Include(l => l.IncomingConnections).ThenInclude(c => c.Level)
                .Include(l => l.Rooms)
                .Include(l => l.Branch)
                .Include(l => l.Game)
                .Include(l => l.GenerationRandom)
                .Where(l => l.GameId == gameId && l.BranchName == branchName && l.Depth == depth)
                .Load();
        }

        private void LoadEvents(Player character)
        {
            // TODO: Replace these with inline includes
            var gameId = character.GameId;
            var id = character.Id;
            _dbContext.Set<ActorMoveEvent>()
                .Where(e => e.GameId == gameId && e.SensorId == id)
                .Include(e => e.Mover)
                .Include(e => e.Movee)
                .Load();
            _dbContext.Set<AttackEvent>()
                .Where(e => e.GameId == gameId && e.SensorId == id)
                .Include(e => e.AppliedEffects)
                .Include(e => e.Attacker)
                .Include(e => e.Victim)
                .Load();
            _dbContext.Set<DeathEvent>()
                .Where(e => e.GameId == gameId && e.SensorId == id)
                .Include(e => e.Deceased)
                .Include(e => e.Corpse)
                .Load();
            _dbContext.Set<ItemConsumptionEvent>()
                .Where(e => e.GameId == gameId && e.SensorId == id)
                .Include(e => e.Consumer)
                .Include(e => e.Item)
                .Load();
            _dbContext.Set<ItemDropEvent>()
                .Where(e => e.GameId == gameId && e.SensorId == id)
                .Include(e => e.Dropper)
                .Include(e => e.Item)
                .Load();
            _dbContext.Set<ItemPickUpEvent>()
                .Where(e => e.GameId == gameId && e.SensorId == id)
                .Include(e => e.Picker)
                .Include(e => e.Item)
                .Load();
            _dbContext.Set<ItemEquipmentEvent>()
                .Where(e => e.GameId == gameId && e.SensorId == id)
                .Include(e => e.Equipper)
                .Include(e => e.Item)
                .Load();
            _dbContext.Set<ItemUnequipmentEvent>()
                .Where(e => e.GameId == gameId && e.SensorId == id)
                .Include(e => e.Unequipper)
                .Include(e => e.Item)
                .Load();
        }

        private void Initialize(Game game)
        {
            game.Services = _gameServices;
            game.Repository = new Repository(_dbContext);
        }

        private void Clean(Game game)
        {
            game = _dbContext.Games
                .Include(g => g.Random)
                .Include(g => g.Branches)
                .Include(g => g.Levels).ThenInclude(l => l.GenerationRandom)
                .Include(g => g.Connections)
                .Include(g => g.Entities)
                .Include(g => g.AbilityDefinitions)
                .Include(g => g.Effects)
                .Include(g => g.Abilities)
                .Include(g => g.AppliedEffects)
                .Include(g => g.SensoryEvents)
                .Single(g => g.Id == game.Id);

            _dbContext.Set<MeleeAttack>()
                .Where(e => e.GameId == game.Id)
                .Include(e => e.Weapon.Abilities).ThenInclude(a => a.Effects)
                .Load();

            foreach (var playerCharacter in game.Players)
            {
                LoadEvents(playerCharacter);

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
            foreach (var abilityDefinition in game.AbilityDefinitions.ToList())
            {
                _dbContext.AbilityDefinitions.Remove(abilityDefinition);
            }
            foreach (var sensoryEvent in game.SensoryEvents.ToList())
            {
                _dbContext.SensoryEvents.Remove(sensoryEvent);
            }
            foreach (var actor in game.Entities.ToList())
            {
                _dbContext.Entities.Remove(actor);
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