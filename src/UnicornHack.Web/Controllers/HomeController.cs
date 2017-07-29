using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Effects;
using UnicornHack.Events;
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

        public IActionResult Index()
        {
            return View(new Character());
        }

        public IActionResult Contact()
        {
            return View();
        }

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
            character.NextActionTarget = string.IsNullOrEmpty(target) ? (int?)null : Int32.Parse(target);
            character.NextActionTarget2 = string.IsNullOrEmpty(target2) ? (int?)null : Int32.Parse(target2);

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

            _dbContext.SaveChanges();

            // Level is null if the character is dead
            if (!character.IsAlive)
            {
                character.Level = level;
            }
        }

        public IActionResult Error()
        {
            return View();
        }

        private Player FindOrCreateCharacter(string name)
        {
            var character = FindCharacter(name);
            if (character == null)
            {
                var seed = Environment.TickCount;
                var game = new Game
                {
                    InitialSeed = seed
                };
                game.Random = new SimpleRandom {Seed = seed};
                Initialize(game);
                _dbContext.Games.Add(game);
                _dbContext.SaveChanges();

                var surfaceBranch = Branch.Get("surface").Instantiate(game);
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
                .Include(c => c.Races).ThenInclude(r => r.Abilities).ThenInclude(a => a.Effects)
                .Include(c => c.Skills)
                //.Include(c => c.SensedEvents)
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

            var levelsToLoad = new List<Tuple<string, byte>>
            {
                Tuple.Create(character.Level.BranchName, character.Level.Depth)
            };

            // Preload adjacent levels
            var connection = character.Level.Connections.SingleOrDefault(s =>
                s.LevelX == character.LevelX
                && s.LevelY == character.LevelY);
            if (connection != null)
            {
                levelsToLoad.Add(Tuple.Create(connection.TargetBranchName, connection.TargetLevelDepth));
            }

            _dbContext.Levels
                .Include(l => l.Items).ThenInclude(i => i.Abilities).ThenInclude(a => a.Effects)
                .Include(l => l.Actors).ThenInclude(a => a.Inventory).ThenInclude(i => i.Abilities).ThenInclude(a => a.Effects)
                .Include(l => l.Actors).ThenInclude(a => a.Abilities).ThenInclude(a => a.Effects)
                .Include(l => l.Connections).ThenInclude(c => c.TargetBranch)
                .Include(l => l.IncomingConnections).ThenInclude(c => c.Level)
                .Include(l => l.Branch)
                .Include(l => l.Game)
                .Include(l => l.GenerationRandom)
                .Where(l => l.GameId == character.GameId)
                .Where(l => levelsToLoad.Contains(new Tuple<string, byte>(l.BranchName, l.Depth)))
                .Load();

            // TODO: Pregenerate all connected levels to ensure the order
            connection?.TargetLevel.EnsureGenerated();

            LoadEvents(character);

            var loadedContainerIds = _dbContext.Items.Local.OfType<Container>().Select(c => c.Id).ToList();
            _dbContext.Set<Container>()
                .Include(c => c.Items).ThenInclude(i => i.Abilities).ThenInclude(a => a.Effects)
                .Where(i => loadedContainerIds.Contains(i.Id))
                .Load();

            var meleeAttacks = _dbContext.Set<MeleeAttack>().Local.Select(c => c.Id).ToList();
            if (meleeAttacks.Any())
            {
                _dbContext.Set<MeleeAttack>()
                    .Include(e => e.Weapon.Abilities).ThenInclude(a => a.Effects)
                    .Where(e => meleeAttacks.Contains(e.Id))
                    .Load();
            }

            return character;
        }

        private void LoadEvents(Player character)
        {
            // TODO: Replace these with inline includes
            _dbContext.Set<ActorMoveEvent>()
                .Where(e => e.GameId == character.GameId && e.SensorId == character.Id)
                .Include(e => e.Mover)
                .Include(e => e.Movee)
                .Load();
            _dbContext.Set<AttackEvent>()
                .Where(e => e.GameId == character.GameId && e.SensorId == character.Id)
                .Include(e => e.Ability.Effects)
                .Include(e => e.Attacker)
                .Include(e => e.Victim)
                .Load();
            _dbContext.Set<DeathEvent>()
                .Where(e => e.GameId == character.GameId && e.SensorId == character.Id)
                .Include(e => e.Deceased)
                .Include(e => e.Corpse)
                .Load();
            _dbContext.Set<ItemConsumptionEvent>()
                .Where(e => e.GameId == character.GameId && e.SensorId == character.Id)
                .Include(e => e.Consumer)
                .Include(e => e.Item)
                .Load();
            _dbContext.Set<ItemDropEvent>()
                .Where(e => e.GameId == character.GameId && e.SensorId == character.Id)
                .Include(e => e.Dropper)
                .Include(e => e.Item)
                .Load();
            _dbContext.Set<ItemPickUpEvent>()
                .Where(e => e.GameId == character.GameId && e.SensorId == character.Id)
                .Include(e => e.Picker)
                .Include(e => e.Item)
                .Load();
            _dbContext.Set<ItemEquipmentEvent>()
                .Where(e => e.GameId == character.GameId && e.SensorId == character.Id)
                .Include(e => e.Equipper)
                .Include(e => e.Item)
                .Load();
            _dbContext.Set<ItemUnequipmentEvent>()
                .Where(e => e.GameId == character.GameId && e.SensorId == character.Id)
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
                .Include(g => g.Actors)
                .Include(g => g.Items)
                .Include(g => g.Abilities)
                .Include(g => g.Effects)
                //.Include(g => g.SensoryEvents)
                .Single(g => g.Id == game.Id);

            _dbContext.Set<MeleeAttack>()
                .Where(e => e.GameId == game.Id)
                .Include(e => e.Weapon.Abilities).ThenInclude(a => a.Effects)
                .Load();

            _dbContext.Set<SimpleRandom>().Remove(game.Random);

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
            foreach (var ability in game.Abilities.ToList())
            {
                _dbContext.Abilities.Remove(ability);
            }
            foreach (var item in game.Items.ToList())
            {
                _dbContext.Items.Remove(item);
            }
            foreach (var sensoryEvent in game.SensoryEvents.ToList())
            {
                _dbContext.SensoryEvents.Remove(sensoryEvent);
            }
            foreach (var actor in game.Actors.ToList())
            {
                _dbContext.Actors.Remove(actor);
            }
            foreach (var level in game.Levels.ToList())
            {
                game.Repository.Delete(level);
                game.Repository.Delete(level.GenerationRandom);
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