using System.Collections;
using System.Security.Cryptography;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Memory;
using UnicornHack.Data;
using UnicornHack.Data.Items;
using UnicornHack.Services;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs;

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
        => Clients.All.SendAsync("ReceiveMessage", Context.User!.Identity!.Name, message);

    public List<object?> GetState(string playerName)
    {
        // TODO: Avoid using a DbContext
        var player = FindPlayer(playerName) ?? CreatePlayer(playerName);

        // TODO: Get a read lock or make serialization thread-safe
        var gameState = _protocol.Serialize(player.Entity, EntityState.Added, null,
            new SerializationContext(_dbContext, player.Entity, _gameServices));

        Debug.Assert(_dbContext.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));

        return gameState;
    }

    public Task ShowDialog(string playerName, int intQueryType, int[] arguments)
    {
        var results = (GameQueryType)intQueryType == GameQueryType.Clear
            ? SerializationContext.DeletedBitArray
            : QueryGame(playerName, intQueryType, arguments);

        // TODO: only send to clients watching this player
        return Clients.All.SendAsync("ReceiveUIRequest", results);
    }

    public List<object?> QueryGame(string playerName, int intQueryType, int[] arguments)
    {
        var queryType = (GameQueryType)intQueryType;

        var player = FindPlayer(playerName);
        if (player?.Entity.Being!.IsAlive != true)
        {
            return SerializationContext.DeletedBitArray;
        }

        var result = new List<object?> { null };
        var setValues = new bool[12];
        setValues[intQueryType + 1] = true;
        result[0] = new BitArray(setValues);

        var manager = player.Entity.Manager;
        var context = new SerializationContext(_dbContext, player.Entity, _gameServices);
        switch (queryType)
        {
            case GameQueryType.SlottableAbilities:
                var slot = arguments[0];
                setValues[intQueryType] = true;
                result.Add(slot);
                var abilities = new Dictionary<int, List<object?>>();
                result.Add(abilities);

                foreach (var slottableAbilityEntity in manager.AbilitiesToAffectableRelationship.GetDependents(
                             player.Entity))
                {
                    var ability = slottableAbilityEntity.Ability!;
                    if (!ability.IsUsable)
                    {
                        continue;
                    }

                    if (slot == AbilitySlottingSystem.DefaultMeleeAttackSlot)
                    {
                        if (ability.Type == AbilityType.DefaultAttack
                            && ((WieldingAbility)ability.Template!).ItemType == ItemType.WeaponMelee)
                        {
                            abilities.Add(ability.EntityId, AbilitySnapshot.Serialize(slottableAbilityEntity, null, context)!);
                        }
                    }
                    else if (slot == AbilitySlottingSystem.DefaultRangedAttackSlot)
                    {
                        if (ability.Type == AbilityType.DefaultAttack
                            && ((WieldingAbility)ability.Template!).ItemType == ItemType.WeaponRanged)
                        {
                            abilities.Add(ability.EntityId, AbilitySnapshot.Serialize(slottableAbilityEntity, null, context)!);
                        }
                    }
                    else if ((ability.Activation & ActivationType.Slottable) != 0
                             && ability.Type != AbilityType.DefaultAttack)
                    {
                        abilities.Add(ability.EntityId, AbilitySnapshot.Serialize(slottableAbilityEntity, null, context)!);
                    }
                }

                break;
            case GameQueryType.PlayerAttributes:
                result.Add(LevelActorSnapshot.SerializeAttributes(player.Entity, SenseType.Sight, context));
                break;
            case GameQueryType.PlayerInventory:
                result.Add(PlayerSnapshot.SerializeItems(player.Entity, context));
                break;
            case GameQueryType.PlayerAdaptations:
                result.Add(PlayerSnapshot.SerializeAdaptations(player.Entity, context));
                break;
            case GameQueryType.PlayerSkills:
                result.Add(PlayerSnapshot.SerializeSkills(player.Entity, context));
                break;
            case GameQueryType.AbilityAttributes:
                var abilityEntity = manager.FindEntity(arguments[0])!;

                var ownerEntity = abilityEntity.Ability!.OwnerEntity!;
                var activatorEntity = abilityEntity.Ability!.OwnerEntity!.HasComponent(EntityComponent.Item)
                    ? player.Entity!
                    : ownerEntity;

                result.Add(AbilitySnapshot.SerializeAttributes(abilityEntity, activatorEntity, context));
                break;
            case GameQueryType.ActorAttributes:
            {
                var actorKnowledge = manager.FindEntity(arguments[0])?.Knowledge;

                result.Add(LevelActorSnapshot.SerializeAttributes(
                    actorKnowledge?.KnownEntity, actorKnowledge?.SensedType ?? SenseType.None, context));
                break;
            }
            case GameQueryType.ItemAttributes:
                var itemEntity = manager.FindEntity(arguments[0]);
                var item = itemEntity?.Item;
                var itemKnowledge = itemEntity?.Knowledge;

                if (item != null)
                {
                    result.Add(InventoryItemSnapshot.SerializeAttributes(itemEntity, SenseType.Sight, context));
                }
                else
                {
                    result.Add(InventoryItemSnapshot.SerializeAttributes(
                        itemKnowledge?.KnownEntity, itemKnowledge?.SensedType ?? SenseType.None, context));
                }

                break;
                // TODO: Handle StaticDescription and PostGameStatistics
            default:
                throw new InvalidOperationException($"Query type {intQueryType} not supported");
        }

        return result;
    }

    public Task ShowStaticDescriptionDialog(string playerName, string topicId, DescriptionCategory category) =>
        // TODO: only send to clients watching this player
        Clients.All.SendAsync("ReceiveUIRequest", QueryStaticDescription(topicId, category));

    public List<object> QueryStaticDescription(string topicId, DescriptionCategory category)
        => new(2) { GameQueryType.StaticDescription, _gameServices.Language.GetDescription(topicId, category) };

    public async Task PerformAction(string playerName, int intAction, int? target, int? target2)
    {
        var action = (ActorAction?)intAction;
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
                Debug.Assert(false, "Action sent out of turn");
                // TODO: Log action sent out of turn
                return;
            }

            if (_dbContext.Snapshot == null)
            {
                _dbContext.Snapshot = new GameSnapshot();
            }

            _dbContext.Snapshot.CaptureState(_dbContext, _gameServices);
        }

        // TODO: Get a lock
        currentPlayer.NextAction = action;
        currentPlayer.NextActionTarget = target;
        currentPlayer.NextActionTarget2 = target2;

        Turn(currentPlayer);

        // TODO: Log turn ended prematurely
        Debug.Assert(currentPlayer.Game.CurrentTick == currentPlayer.NextActionTick, "Turn ended prematurely");

        _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
        foreach (var playerEntity in currentPlayer.Game.Manager.Players)
        {
            var newPlayer = action == null;
            var serializedPlayer = _protocol.Serialize(
                playerEntity, newPlayer ? EntityState.Added : EntityState.Modified, _dbContext.Snapshot,
                new SerializationContext(_dbContext, playerEntity, _gameServices));

            // TODO: only send to clients watching this player
            await Clients.All.SendAsync("ReceiveState", serializedPlayer).ConfigureAwait(false);

            if (!playerEntity.Being!.IsAlive)
            {
                var results = new List<object> { GameQueryType.PostGameStatistics, new List<object>() };

                // TODO: only send to clients watching this player
                await Clients.All.SendAsync("ReceiveUIRequest", results).ConfigureAwait(false);
            }
        }

        _dbContext.ChangeTracker.AutoDetectChangesEnabled = true;

        _dbContext.ChangeTracker.AcceptAllChanges();
    }

    private void Turn(PlayerComponent currentPlayer)
    {
        var manager = currentPlayer.Game.Manager;
        foreach (var levelEntity in manager.Levels)
        {
            var level = levelEntity.Level!;
            if (level.TerrainChanges != null)
            {
                level.TerrainChanges.Clear();
            }
            else
            {
                level.TerrainChanges = new Dictionary<short, byte>();
            }

            if (level.KnownTerrainChanges != null)
            {
                level.KnownTerrainChanges.Clear();
            }
            else
            {
                level.KnownTerrainChanges = new Dictionary<short, byte>();
            }

            if (level.WallNeighborsChanges != null)
            {
                level.WallNeighborsChanges.Clear();
            }
            else
            {
                level.WallNeighborsChanges = new Dictionary<short, byte>();
            }

            if (_dbContext.Snapshot?.LevelSnapshots.ContainsKey(levelEntity.Id) != true)
            {
                level.VisibleTerrainSnapshot = null;
            }

            level.VisibleNeighborsChanged = false;
        }

        currentPlayer.Game.ActingPlayer = null;

        manager.TimeSystem.AdvanceToNextPlayerTurn(manager);

        //TODO: If all players are dead end the game

        _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
        foreach (var levelEntity in manager.Levels)
        {
            var level = levelEntity.Level!;

            var levelEntry = _dbContext.Entry(level);
            if (levelEntry.State == EntityState.Added
                || level.Width == 0)
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

            if (level.WallNeighborsChanges == null
                || level.WallNeighborsChanges.Count > 0)
            {
                levelEntry.Property(l => l.WallNeighbors).IsModified = true;
            }

            // TODO: Move VisibleTerrainSnapshot and VisibleTerrainChanges to the snapshot
            if (level.VisibleTerrainChanges == null)
            {
                level.VisibleTerrainChanges = new Dictionary<short, byte>();
            }
            else
            {
                level.VisibleTerrainChanges.Clear();
            }

            if (level.VisibleTerrainSnapshot != null)
            {
                var tileCount = level.TileCount;
                for (short i = 0; i < tileCount; i++)
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

            if (level.VisibleNeighborsChanged)
            {
                levelEntry.Property(l => l.VisibleNeighbors).IsModified = true;
            }
        }

        _dbContext.ChangeTracker.AutoDetectChangesEnabled = true;

        _dbContext.SaveChanges(acceptAllChangesOnSuccess: false);
    }

    private PlayerComponent CreatePlayer(string name)
    {
        uint seed;
        using (var rng = RandomNumberGenerator.Create())
        {
            var rngData = new byte[4];
            rng.GetBytes(rngData);

            seed = (uint)(rngData[0] | rngData[1] << 8 | rngData[2] << 16 | rngData[3] << 24);
        }

        var game = new Game { InitialSeed = seed, Random = new SimpleRandom { Seed = seed } };
        Initialize(game);
        _dbContext.Games.Add(game);
        _dbContext.SaveChanges();

        var manager = game.Manager;
        var surfaceBranch = Branch.Loader.Find("surface")!.Instantiate(game);
        var surfaceLevel = LevelGenerator.CreateEmpty(surfaceBranch, depth: 1, seed, manager);
        LevelGenerator.EnsureGenerated(surfaceLevel);

        var initialLevelConnection = surfaceLevel.Connections.Single().Value.Connection!;
        var initialLevelEntity = initialLevelConnection.TargetLevelEntity;
        LevelGenerator.EnsureGenerated(initialLevelEntity.Level!);

        // TODO: Set correct sex
        var playerEntity = PlayerRace.InstantiatePlayer(
            name, Sex.Male, initialLevelEntity.Level!, initialLevelConnection.TargetLevelCell!.Value);

        manager.Queue.ProcessQueue(manager);

        ItemData.FlaskOfHealing.Instantiate(playerEntity);
        ItemData.MailArmor.Instantiate(playerEntity);
        ItemData.LongSword.Instantiate(playerEntity);
        ItemData.Dagger.Instantiate(playerEntity);
        ItemData.Shortbow.Instantiate(playerEntity);
        ItemData.ThrowingKnives.Instantiate(playerEntity);
        ItemData.FireStaff.Instantiate(playerEntity);
        ItemData.FreezingFocus.Instantiate(playerEntity);
        ItemData.FieryAegis.Instantiate(playerEntity);
        ItemData.PotionOfOgreness.Instantiate(playerEntity);
        ItemData.PotionOfElfness.Instantiate(playerEntity);
        ItemData.PotionOfDwarfness.Instantiate(playerEntity);
        ItemData.SkillbookOfConjuration.Instantiate(playerEntity);

        manager.LoggingSystem.WriteLog(game.Services.Language.Welcome(playerEntity), playerEntity, manager);

        manager.Queue.ProcessQueue(manager);
        Turn(playerEntity.Player!);
        _dbContext.ChangeTracker.AcceptAllChanges();

        _gameServices.SharedCache.Set(game, game,
            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1)));

        return playerEntity.Player!;
    }

    private PlayerComponent? FindPlayer(string name)
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
            // TODO: Unload old levels
            loadedGame = cachedGame;

            InitializeContext(loadedGame);

            _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            EntityAttacher.Attach(loadedGame, _dbContext);
            _dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
        }
        else
        {
            Initialize(loadedGame);
            EntityAttacher.Attach(loadedGame, _dbContext);

            loadedGame = _dbContext.LoadGame(loadedGame.Id);

            _gameServices.SharedCache.Set(loadedGame, loadedGame,
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1)));
        }

        // TODO: Preserve replays
        if (!_dbContext.PlayerComponents.Local.Any(pc => pc.Entity.Being!.IsAlive))
        {
            _dbContext.Clean(loadedGame);
            _dbContext.ChangeTracker.Tracked -= OnTracked;
            _dbContext.ChangeTracker.StateChanged -= OnStateChanged;

            _gameServices.SharedCache.Remove(loadedGame);
            return null;
        }

        return _dbContext.PlayerComponents.Local.First(p
            => p.ProperName.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private void Initialize(Game game)
    {
        game.Services = _gameServices;
        game.Repository = _dbContext;
        if (game.Manager == null!)
        {
            var queue = new SequentialMessageQueue<GameManager>();
            var gameManager = new GameManager { Game = game };
            gameManager.Initialize(queue);
            game.Manager = gameManager;
        }

        InitializeContext(game);
        _dbContext.Snapshot = null;
    }

    private void InitializeContext(Game game)
    {
        _dbContext.Manager = game.Manager;
        _dbContext.ChangeTracker.Tracked += OnTracked;
        _dbContext.ChangeTracker.StateChanged += OnStateChanged;
    }

    private void OnTracked(object? sender, EntityTrackedEventArgs args)
    {
        var entry = args.Entry;
        if (entry.State == EntityState.Added
            && entry.Entity is ITrackable trackable)
        {
            trackable.StartTracking(sender!);
        }

        if (args.FromQuery
            && entry.Entity is GameEntity entity)
        {
            ((GameDbContext)entry.Context).Manager.AddEntity(entity);
        }
    }

    private void OnStateChanged(object? sender, EntityStateChangedEventArgs args)
    {
        if (args.NewState == EntityState.Detached && args.Entry.Entity is ITrackable trackable)
        {
            trackable.StopTracking(sender!);
        }
    }
}
