using System;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Effects;
using UnicornHack.Events;
using UnicornHack.Generation;
using UnicornHack.Models;
using UnicornHack.Models.GameViewModels;
using UnicornHack.Services;
using UnicornHack.Utils;

namespace UnicornHack.Controllers
{
    public class HomeController : Controller
    {
        private readonly GameDbContext _dbContext;

        //private readonly IHubContext<GameHub, IGameClient> _hubContext;
        private readonly GameServices _gameServices;

        public HomeController(GameDbContext dbContext, GameServices gameServices)
        {
            _dbContext = dbContext;
            _gameServices = gameServices;
        }

        public IActionResult Index() => View(new Character());

        public IActionResult Contact() => View();

        //
        // GET: /Home/Game?Name
        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Game(Character model)
        {
            if (model.Name == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(FindOrCreateCharacter(model.Name));
        }

        //
        // POST: /Home/PerformAction
        [HttpPost]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult PerformAction(string name, string action, string target, string target2)
        {
            var character = FindOrCreateCharacter(name);

            character.NextAction = action;
            character.NextActionTarget = string.IsNullOrEmpty(target) ? (int?)null : int.Parse(target);
            character.NextActionTarget2 = string.IsNullOrEmpty(target2) ? (int?)null : int.Parse(target2);

            Turn(character);
            return PartialView(nameof(Game), character);
        }

        private void Turn(Player character)
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

            var entries = _dbContext.ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Modified && e.State != EntityState.Unchanged)
                .ToList();
            _dbContext.SaveChanges();

            // Level is null if the character is dead
            if (!character.IsAlive)
            {
                character.Level = level;
            }
        }

        public IActionResult Error() => View();

        private Player FindOrCreateCharacter(string name)
        {
            var character = FindCharacter(name);
            if (character == null)
            {
                var seed = 0;
                using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
                {
                    byte[] rngData = new byte[4];
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
                _dbContext.SaveChanges();

                Turn(character);
                _dbContext.SaveChanges();
            }

            if (!character.IsAlive
                && !character.Game.Players.Any(pc => pc.IsAlive))
            {
                Clean(character.Game);
                _dbContext.SaveChanges();
                character = FindOrCreateCharacter(name);
            }

            return character;
        }

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
    }
}