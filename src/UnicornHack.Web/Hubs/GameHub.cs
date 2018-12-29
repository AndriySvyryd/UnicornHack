using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Memory;
using UnicornHack.Data;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Services;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Time;
using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs
{
    public class GameHub : Hub
    {
        private readonly GameDbContext _dbContext;
        private readonly GameServices _gameServices;
        private readonly GameTransmissionProtocol _protocol;

        public GameHub(GameDbContext dbContext, GameServices gameServices, GameTransmissionProtocol protocol)
        {
            _dbContext = dbContext;
            _gameServices = gameServices;
            _protocol = protocol;
        }

        public Task SendMessage(string message)
            => Clients.All.SendAsync("ReceiveMessage", Context.User.Identity.Name, message);

        public List<object> GetState(string playerName)
        {
            var player = FindPlayer(playerName) ?? CreatePlayer(playerName);

            return _protocol.Serialize(player.Entity, EntityState.Added, null,
                new SerializationContext(_dbContext, player.Entity, _gameServices));
        }

        public Task QueryGame(string playerName, int intQueryType, int[] arguments)
        {
            var queryType = (GameQueryType)intQueryType;
            var result = new List<object> { intQueryType };

            if (queryType == GameQueryType.Clear)
            {
                // TODO: only send to clients watching this player
                return Clients.All.SendAsync("ReceiveUIRequest", result);
            }

            var player = FindPlayer(playerName);

            if (player != null
                && player.Entity.Being.IsAlive)
            {
                var manager = player.Entity.Manager;
                switch (queryType)
                {
                    case GameQueryType.SlottableAbilities:
                        var slot = arguments[0];
                        result.Add(slot);
                        var abilities = new List<object>();
                        result.Add(abilities);

                        var context = new SerializationContext(_dbContext, player.Entity, _gameServices);
                        foreach (var abilityEntity in manager.AbilitiesToAffectableRelationship[player.EntityId])
                        {
                            var ability = abilityEntity.Ability;
                            if (!ability.IsUsable)
                            {
                                continue;
                            }

                            if (slot == AbilitySlottingSystem.DefaultAttackSlot)
                            {
                                if (manager.SkillAbilitiesSystem.CanBeDefaultAttack(ability))
                                {
                                    abilities.Add(AbilitySnapshot.Serialize(abilityEntity, null, context));
                                }
                            }
                            else if ((ability.Activation & ActivationType.Slottable) != 0
                                    && !manager.SkillAbilitiesSystem.CanBeDefaultAttack(ability))
                            {
                                abilities.Add(AbilitySnapshot.Serialize(abilityEntity, null, context));
                            }
                        }

                        break;
                    default:
                        throw new InvalidOperationException($"Query type {intQueryType} not supported");
                }
            }

            // TODO: only send to clients watching this player
            return Clients.All.SendAsync("ReceiveUIRequest", result);
        }

        public async Task PerformAction(string playerName, int intAction, int? target, int? target2)
        {
            var action = (PlayerAction?)intAction;
            var currentPlayer = FindPlayer(playerName);
            if (currentPlayer == null)
            {
                currentPlayer = CreatePlayer(playerName);
                action = null;
                target = null;
                target2 = null;
            }
            else
            {
                if (currentPlayer.Game.CurrentTick != currentPlayer.NextActionTick)
                {
                    // TODO: Log action sent out of turn
                    return;
                }

                if (_dbContext.Snapshot == null)
                {
                    _dbContext.Snapshot = new GameSnapshot();
                }

                _dbContext.Snapshot.Snapshot(_dbContext, _gameServices);
            }

            currentPlayer.NextAction = action;
            currentPlayer.NextActionTarget = target;
            currentPlayer.NextActionTarget2 = target2;

            Turn(currentPlayer);

            _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            foreach (var playerEntity in currentPlayer.Game.Manager.Players)
            {
                var newPlayer = action == null;
                var serializedPlayer = _protocol.Serialize(
                    playerEntity, newPlayer ? EntityState.Added : EntityState.Modified, _dbContext.Snapshot,
                    new SerializationContext(_dbContext, playerEntity, _gameServices));

                // TODO: only send to clients watching this player
                await Clients.All.SendAsync("ReceiveState", serializedPlayer);
            }

            _dbContext.ChangeTracker.AutoDetectChangesEnabled = true;

            _dbContext.ChangeTracker.AcceptAllChanges();
        }

        private void Turn(PlayerComponent currentPlayer)
        {
            var manager = currentPlayer.Game.Manager;
            foreach (var levelEntity in manager.Levels)
            {
                var level = levelEntity.Level;
                if (level.TerrainChanges != null)
                {
                    level.TerrainChanges.Clear();
                }
                else
                {
                    level.TerrainChanges = new Dictionary<int, byte>();
                }

                if (level.KnownTerrainChanges != null)
                {
                    level.KnownTerrainChanges.Clear();
                }
                else
                {
                    level.KnownTerrainChanges = new Dictionary<int, byte>();
                }

                if (level.WallNeighboursChanges != null)
                {
                    level.WallNeighboursChanges.Clear();
                }
                else
                {
                    level.WallNeighboursChanges = new Dictionary<int, byte>();
                }

                if (_dbContext.Snapshot?.LevelSnapshots.ContainsKey(levelEntity.Id) != true)
                {
                    level.VisibleTerrainSnapshot = null;
                }

                level.VisibleNeighboursChanged = false;
            }

            currentPlayer.Game.ActingPlayer = null;
            TimeSystem.AdvanceToNextPlayerTurn(manager);

            //TODO: If all players are dead end the game

            _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            foreach (var levelEntity in manager.Levels)
            {
                var level = levelEntity.Level;

                var levelEntry = _dbContext.Entry(level);
                if (levelEntry.State == EntityState.Added)
                {
                    continue;
                }

                if (level.TerrainChanges == null
                    || level.TerrainChanges.Count > 0)
                {
                    levelEntry.Property(l => l.Terrain).IsModified = true;
                }

                if (level.KnownTerrainChanges == null
                    || level.KnownTerrainChanges.Count > 0)
                {
                    levelEntry.Property(l => l.KnownTerrain).IsModified = true;
                }

                if (level.WallNeighboursChanges == null
                    || level.WallNeighboursChanges.Count > 0)
                {
                    levelEntry.Property(l => l.WallNeighbours).IsModified = true;
                }

                // TODO: Move VisibleTerrainSnapshot and VisibleTerrainChanges to the snapshot
                if (level.VisibleTerrainChanges == null)
                {
                    level.VisibleTerrainChanges = new Dictionary<int, byte>();
                }
                else
                {
                    level.VisibleTerrainChanges.Clear();
                }

                if (level.VisibleTerrainSnapshot != null)
                {
                    for (var i = 0; i < level.VisibleTerrain.Length; i++)
                    {
                        var newValue = level.VisibleTerrain[i];
                        if (newValue != level.VisibleTerrainSnapshot[i])
                        {
                            level.VisibleTerrainChanges.Add(i, newValue);
                        }
                    }
                }

                if (level.VisibleTerrainSnapshot == null
                    || level.VisibleTerrainChanges.Count > 0)
                {
                    levelEntry.Property(l => l.VisibleTerrain).IsModified = true;
                }

                if (level.VisibleNeighboursChanged)
                {
                    levelEntry.Property(l => l.VisibleNeighbours).IsModified = true;
                }
            }

            _dbContext.ChangeTracker.AutoDetectChangesEnabled = true;

            _dbContext.SaveChanges(acceptAllChangesOnSuccess: false);
        }

        private PlayerComponent CreatePlayer(string name)
        {
            uint seed;
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                var rngData = new byte[4];
                rng.GetBytes(rngData);

                seed = (uint)(rngData[0] | rngData[1] << 8 | rngData[2] << 16 | rngData[3] << 24);
            }

            var game = new Game
            {
                InitialSeed = seed,
                Random = new SimpleRandom {Seed = seed}
            };
            Initialize(game);
            _dbContext.Games.Add(game);
            _dbContext.SaveChanges();

            var manager = game.Manager;
            var surfaceBranch = Branch.Loader.Find("surface").Instantiate(game);
            var surfaceLevel = LevelGenerator.CreateEmpty(surfaceBranch, depth: 1, seed, manager);
            LevelGenerator.EnsureGenerated(surfaceLevel);

            var initialLevelConnection =
                manager.ConnectionsToLevelRelationship[surfaceLevel.EntityId].Single().Connection;
            var initialLevelEntity = manager.FindEntity(initialLevelConnection.TargetLevelId);
            LevelGenerator.EnsureGenerated(initialLevelEntity.Level);

            // TODO: Set correct sex
            var playerEntity = PlayerRace.InstantiatePlayer(
                name, Sex.Male, initialLevelEntity, initialLevelConnection.TargetLevelCell.Value);

            ItemData.PotionOfHealing.Instantiate(playerEntity);
            ItemData.MailArmor.Instantiate(playerEntity);
            ItemData.LongSword.Instantiate(playerEntity);
            ItemData.Dagger.Instantiate(playerEntity);
            ItemData.Shortbow.Instantiate(playerEntity);
            ItemData.ThrowingKnives.Instantiate(playerEntity);
            ItemData.FireStaff.Instantiate(playerEntity);
            ItemData.FreezingFocus.Instantiate(playerEntity);
            ItemData.PotionOfOgreness.Instantiate(playerEntity);
            ItemData.PotionOfElfness.Instantiate(playerEntity);
            ItemData.PotionOfDwarfness.Instantiate(playerEntity);
            ItemData.PotionOfExperience.Instantiate(playerEntity, quantity: 6);
            ItemData.SkillbookOfConjuration.Instantiate(playerEntity);

            manager.LoggingSystem.WriteLog(game.Services.Language.Welcome(playerEntity), playerEntity, manager);

            Turn(playerEntity.Player);
            _dbContext.ChangeTracker.AcceptAllChanges();

            _gameServices.SharedCache.Set(game, game,
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1)));

            return playerEntity.Player;
        }

        private PlayerComponent FindPlayer(string name)
        {
            var loadedGame = _dbContext.PlayerComponents.Where(playerComponent => playerComponent.ProperName == name)
                .Join(_dbContext.Games,
                    playerComponent => playerComponent.GameId, game => game.Id,
                    (_, game) => game)
                .AsNoTracking()
                .FirstOrDefault();

            if (loadedGame == null)
            {
                return null;
            }

            // TODO: Perf: Cache the DbContext as well to avoid reattaching
            if (_gameServices.SharedCache.TryGetValue(loadedGame, out var cachedGameObject)
                && cachedGameObject is Game cachedGame
                && cachedGame.CurrentTick == loadedGame.CurrentTick)
            {
                // TODO: Unload non-adjacent levels when too many loaded
                loadedGame = cachedGame;

                Initialize(loadedGame);

                _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
                EntityAttacher.Attach(cachedGame, _dbContext);
                _dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            }
            else
            {
                Initialize(loadedGame);
                _dbContext.Attach(loadedGame);

                loadedGame = _dbContext.LoadGame(loadedGame.Id);

                _gameServices.SharedCache.Set(loadedGame, loadedGame,
                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1)));
            }

            // TODO: Preserve old games
            if (!_dbContext.PlayerComponents.Local.Any(pc => pc.Entity.Being.IsAlive))
            {
                _dbContext.Clean(loadedGame);
                return null;
            }

            return _dbContext.PlayerComponents.Local.First(p => p.ProperName == name);
        }

        private void Initialize(Game game)
        {
            game.Services = _gameServices;
            game.Repository = _dbContext;
            if (game.Manager == null)
            {
                var queue = new SequentialMessageQueue<GameManager>();
                var gameManager = new GameManager {Game = game};
                gameManager.Initialize(queue);
                game.Manager = gameManager;
            }

            _dbContext.Manager = game.Manager;

            _dbContext.ChangeTracker.Tracked += OnTracked;
            _dbContext.ChangeTracker.StateChanged += OnStateChanged;
        }

        private void OnTracked(object sender, EntityTrackedEventArgs args)
        {
            var entry = args.Entry;
            if (entry.State == EntityState.Added
                && entry.Entity is ITrackable trackable)
            {
                trackable.StartTracking(sender);
            }

            if (args.FromQuery
                && entry.Entity is GameEntity entity)
            {
                ((GameDbContext)entry.Context).Manager.AddEntity(entity);
            }
        }

        private void OnStateChanged(object sender, EntityStateChangedEventArgs args)
        {
            if (args.NewState == EntityState.Detached && args.Entry.Entity is ITrackable trackable)
            {
                trackable.StopTracking(sender);
            }
        }
    }
}
