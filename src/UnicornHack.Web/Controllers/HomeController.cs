using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Effects;
using UnicornHack.Events;
using UnicornHack.Hubs;
using UnicornHack.Models;
using UnicornHack.Models.GameViewModels;
using UnicornHack.Services;

namespace UnicornHack.Controllers
{
    public class HomeController : Controller
    {
        private readonly GameDbContext _dbContext;
        private readonly IHubContext<GameHub, IGameClient> _hubContext;
        private readonly GameServices _gameServices;

        public HomeController(GameDbContext dbContext, IHubContext<GameHub, IGameClient> hubContext,
            GameServices gameServices)
        {
            _dbContext = dbContext;
            _hubContext = hubContext;
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
                var game = new Game
                {
                    RandomSeed = Environment.TickCount
                };
                Initialize(game);
                _dbContext.Games.Add(game);
                _dbContext.SaveChanges();

                var initialLevel = Level.CreateLevel(game, Level.MainBranchName, depth: 1);
                var upStairs = initialLevel.UpStairs.First();
                character = (Player)Player.Get("player human")
                    .Instantiate(initialLevel, upStairs.DownLevelX, upStairs.DownLevelY);
                character.Name = name;

                character.WriteLog(game.Services.Language.Welcome(character));

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
                .Include(c => c.Game)
                .Include(c => c.Log)
                .Include(c => c.Skills)
                //.Include(c => c.SensedEvents)
                .Include(c => c.Level.DownStairs)
                .Include(c => c.Level.UpStairs)
                .Include(c => c.Level.Game)
                .Where(c => c.GameId == character.GameId)
                .Load();

            if (character.Level == null)
            {
                return character;
            }

            var levelsToLoad = new List<int> {character.Level.Id};

            // Preload adjacent levels
            var upStairs = character.Level.UpStairs.SingleOrDefault(s =>
                s.DownLevelX == character.LevelX
                && s.DownLevelY == character.LevelY);
            if (upStairs != null)
            {
                if (upStairs.UpId.HasValue)
                {
                    levelsToLoad.Add(upStairs.UpId.Value);
                }
                else
                {
                    Level.CreateUpLevel(upStairs);
                }
            }

            var downStairs = character.Level.DownStairs.SingleOrDefault(s =>
                s.UpLevelX == character.LevelX
                && s.UpLevelY == character.LevelY);
            if (downStairs != null)
            {
                if (downStairs.DownId.HasValue)
                {
                    levelsToLoad.Add(downStairs.DownId.Value);
                }
                else
                {
                    Level.CreateDownLevel(downStairs);
                }
            }

            _dbContext.Levels
                .Include(l => l.Items).ThenInclude(i => i.Abilities).ThenInclude(a => a.Effects)
                .Include(l => l.Actors)
                .ThenInclude(a => a.Inventory)
                .ThenInclude(i => i.Abilities)
                .ThenInclude(a => a.Effects)
                .Include(l => l.Actors).ThenInclude(a => a.Abilities).ThenInclude(a => a.Effects)
                .Include(l => l.DownStairs)
                .Include(l => l.UpStairs)
                .Include(l => l.Game)
                .Where(l => levelsToLoad.Contains(l.Id))
                .Load();

            LoadEvents(character);

            var loadedContainerIds = _dbContext.Items.Local.OfType<Container>().Select(c => c.Id).ToList();
            _dbContext.Items
                .Where(i => i.ContainerId.HasValue && loadedContainerIds.Contains(i.ContainerId.Value))
                .Load();

            var meleeAttacks = _dbContext.Set<MeleeAttack>().Local.Select(c => c.Id).ToList();
            if (meleeAttacks.Any())
            {
                _dbContext.Set<MeleeAttack>().Where(e => meleeAttacks.Contains(e.Id))
                    .Include(e => e.Weapon.Abilities).ThenInclude(a => a.Effects)
                    .Load();
            }

            Initialize(character.Game);

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
            game.Delete = Delete;
        }

        private static readonly MethodInfo _genericDelete = typeof(HomeController).GetMethod(nameof(GenericDelete),
            BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly Dictionary<Type, MethodInfo> _deleteDelegates = new Dictionary<Type, MethodInfo>();

        private void Delete(object entity)
        {
            var type = entity.GetType();
            MethodInfo delete;
            if (!_deleteDelegates.TryGetValue(type, out delete))
            {
                delete = _genericDelete.MakeGenericMethod(type);
                _deleteDelegates.Add(type, delete);
            }

            delete.Invoke(this, new[] {entity});
        }

        private void GenericDelete<TEntity>(TEntity entity)
            where TEntity : class
        {
            var local = _dbContext.Set<TEntity>().Local;
            if (local.Contains(entity))
            {
                local.Remove(entity);
            }
        }

        private void Clean(Game game)
        {
            game = _dbContext.Games
                .Include(g => g.Levels)
                .Include(g => g.Stairs)
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
            foreach (var stairs in game.Stairs.ToList())
            {
                _dbContext.Stairs.Remove(stairs);
            }
            foreach (var level in game.Levels.ToList())
            {
                _dbContext.Levels.Remove(level);
            }

            _dbContext.Games.Remove(game);
            _dbContext.SaveChanges();
        }
    }
}