using System.Security.Cryptography;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using UnicornHack.Data;
using UnicornHack.Data.Items;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Services;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs;

public class GameHub : Hub
{
    private readonly IDbContextFactory<GameDbContext> _dbContextFactory;
    private readonly GameServices _gameServices;
    private readonly GameOptions _defaultGameOptions;

    public GameHub(
        IDbContextFactory<GameDbContext> dbContextFactory,
        GameServices gameServices,
        IOptions<GameOptions> defaultGameOptions)
    {
        _dbContextFactory = dbContextFactory;
        _gameServices = gameServices;
        _defaultGameOptions = defaultGameOptions.Value;
    }

    public Task SendMessage(string message)
    {
        // TODO: Sanitize tags
        return Clients.All.SendAsync("ReceiveMessage", Context.User!.Identity!.Name, message);
    }

    public async Task<PlayerChange> GetState(string playerName)
    {
        var session = (await GetSessionAsync(playerName, createIfMissing: true).ConfigureAwait(false))!;
        if (session.IsDisposed)
        {
            throw SessionUnavailable();
        }

        try
        {
            var gameState = GameStateManager.GetState(session.Player.Entity, _gameServices);
            Debug.Assert(session.DbContext.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
            return gameState;
        }
        finally
        {
            if (_defaultGameOptions.DisableSessionCache)
            {
                await DisposeSessionAsync(session).ConfigureAwait(false);
            }
        }
    }

    public async Task ShowDialog(string playerName, int intQueryType, int[] arguments)
    {
        var results = (GameQueryType)intQueryType == GameQueryType.Clear
            ? DialogData.Deleted()
            : await QueryGameAsync(playerName, intQueryType, arguments)
                .ConfigureAwait(false);

        // TODO: only send to clients watching this player
        await Clients.All.SendAsync("ReceiveUIRequest", results).ConfigureAwait(false);
    }

    public Task<DialogData> QueryGame(string playerName, int intQueryType, int[] arguments)
        => QueryGameAsync(playerName, intQueryType, arguments);

    private async Task<DialogData> QueryGameAsync(
        string playerName, int intQueryType, int[] arguments)
    {
        var queryType = (GameQueryType)intQueryType;
        if (queryType == GameQueryType.Clear)
        {
            return DialogData.Deleted();
        }

        // Query serializers only read the in-memory game graph, so concurrent dialogs and tooltips
        // do not participate in mutation synchronization.
        var session = await GetSessionAsync(playerName, createIfMissing: false).ConfigureAwait(false);
        if (session == null || session.IsDisposed)
        {
            return DialogData.Deleted();
        }

        try
        {
            if (session.Player.Entity.Being!.IsAlive != true)
            {
                return DialogData.Deleted();
            }

            return QueryGameInternal(session, intQueryType, queryType, arguments);
        }
        finally
        {
            if (_defaultGameOptions.DisableSessionCache)
            {
                await DisposeSessionAsync(session).ConfigureAwait(false);
            }
        }
    }

    private DialogData QueryGameInternal(
        GameSession session, int intQueryType, GameQueryType queryType, int[] arguments)
    {
        var player = session.Player;
        var result = new DialogData
        {
            ChangedProperties = (ushort)(1 << (intQueryType + 1))
        };
        if (queryType == GameQueryType.SlottableAbilities)
        {
            // SlottableAbilities tags an extra bit at intQueryType to mark that a slot id follows.
            result.ChangedProperties = (ushort)(
                result.ChangedProperties.GetValueOrDefault() | (1 << intQueryType));
        }

        var manager = player.Entity.Manager;
        var context = new SerializationContext(session.DbContext, player.Entity, _gameServices);
        switch (queryType)
        {
            case GameQueryType.SlottableAbilities:
                var slot = arguments[0];
                result.AbilitySlot = slot;
                var abilities = result.SlottableAbilities;

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
                        if (ability.Type == AbilityType.DefaultMeleeAttack
                            && (ability.Trigger & ActivationType.OnMeleeAttack) != 0)
                        {
                            abilities.Add(ability.EntityId, GameQueryProcessor.SerializeSlottedAbility(slottableAbilityEntity, context));
                        }
                    }
                    else if (slot == AbilitySlottingSystem.DefaultRangedAttackSlot)
                    {
                        if (ability.Type == AbilityType.DefaultRangedAttack
                            && (ability.Trigger & ActivationType.OnRangedAttack) != 0)
                        {
                            abilities.Add(ability.EntityId, GameQueryProcessor.SerializeSlottedAbility(slottableAbilityEntity, context));
                        }
                    }
                    else if ((ability.Activation & ActivationType.Slottable) != 0
                             && !ability.Type.IsDefaultAttack())
                    {
                        abilities.Add(ability.EntityId, GameQueryProcessor.SerializeSlottedAbility(slottableAbilityEntity, context));
                    }
                }

                break;
            case GameQueryType.PlayerAttributes:
                result.PlayerAttributes = GameQueryProcessor.SerializeActorAttributes(player.Entity, true, context);
                break;
            case GameQueryType.PlayerInventory:
                result.PlayerInventory = GameQueryProcessor.SerializeItems(player.Entity, context);
                break;
            case GameQueryType.PlayerAdaptations:
                result.PlayerAdaptations = GameQueryProcessor.SerializeAdaptations(player.Entity, context);
                break;
            case GameQueryType.PlayerSkills:
                result.PlayerSkills = GameQueryProcessor.SerializeSkills(player.Entity, context);
                break;
            case GameQueryType.AbilityAttributes:
                var abilityEntity = manager.FindEntity(arguments[0])!;

                var ownerEntity = abilityEntity.Ability!.OwnerEntity!;
                var activatorEntity = abilityEntity.Ability!.OwnerEntity!.HasComponent(EntityComponent.Item)
                    ? player.Entity!
                    : ownerEntity;

                result.AbilityAttributes = GameQueryProcessor.SerializeAbilityAttributes(
                    abilityEntity, activatorEntity, context);
                break;
            case GameQueryType.ActorAttributes:
            {
                var actorKnowledge = manager.FindEntity(arguments[0])?.Knowledge;

                result.ActorAttributes = GameQueryProcessor.SerializeActorAttributes(
                    actorKnowledge?.KnownEntity, actorKnowledge?.IsIdentified ?? false, context);
                break;
            }
            case GameQueryType.ItemAttributes:
                var itemEntity = manager.FindEntity(arguments[0]);
                var item = itemEntity?.Item;
                var itemKnowledge = itemEntity?.Knowledge;

                if (item != null)
                {
                    result.ItemAttributes = GameQueryProcessor.SerializeItemAttributes(itemEntity, true, context);
                }
                else
                {
                    result.ItemAttributes = GameQueryProcessor.SerializeItemAttributes(
                        itemKnowledge?.KnownEntity, itemKnowledge?.IsIdentified ?? false, context);
                }

                break;
            case GameQueryType.LevelMap:
                result.LevelMap = GameQueryProcessor.SerializeLevelMap(player.Entity);
                break;
            case GameQueryType.PostGameStatistics:
                result.PostGameStatistics = new PostGameStatistics();
                break;
            default:
                throw new InvalidOperationException($"Query type {intQueryType} not supported");
        }

        return result;
    }

    public string QueryStaticDescription(string topicId, int category)
        => _gameServices.Language.GetDescription(topicId, (DescriptionCategory)category);

    public async Task PerformAction(string playerName, int intAction, int? target, int? target2)
    {
        var action = (ActorActionType?)intAction;
        Dictionary<int, List<TurnChangeSet>> changeSets;
        int previousTick;
        var session = (await GetSessionAsync(playerName, createIfMissing: true).ConfigureAwait(false))!;
        await session.MutationLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (session.IsDisposed)
            {
                throw SessionUnavailable();
            }

            if (session.Player.Game.CurrentTick != session.Player.NextActionTick)
            {
                // TODO: Log action sent out of turn instead
                throw new InvalidOperationException("Action sent out of turn");
            }

            session.Player.NextAction = action;
            session.Player.NextActionTarget = target;
            session.Player.NextActionTarget2 = target2;
            previousTick = session.Player.Game.CurrentTick;
            changeSets = GameStateManager.Turn(
                session.Player, session.RoomChangeBuilder, _gameServices,
                dbContext: session.DbContext);
            await session.DbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        finally
        {
            session.MutationLock.Release();
        }

        // TODO: Log turn ended prematurely
        Debug.Assert(session.Player.Game.CurrentTick == session.Player.NextActionTick, "Turn ended prematurely");
        var manager = session.Player.Game.Manager;
        var changeSetResponses = new List<List<TurnChangeSet>>(manager.Players.Count);
        List<DialogData>? uiResponses = null;
        var died = false;
        foreach (var playerEntity in manager.Players)
        {
            if (!changeSets.TryGetValue(playerEntity.Id, out var playerChangeSets)
                && previousTick != session.Player.Game.CurrentTick)
            {
                throw new InvalidOperationException(
                    $"No change sets for player {playerEntity.Id}. "
                    + $"Tick: {manager.Game.CurrentTick}. "
                    + $"NextActionTick: {session.Player.NextActionTick}.");
            }

            changeSetResponses.Add(playerChangeSets ?? []);
            if (!playerEntity.Being!.IsAlive)
            {
                var results = QueryGameInternal(
                    session, (int)GameQueryType.PostGameStatistics,
                    GameQueryType.PostGameStatistics, []);
                (uiResponses ??= []).Add(results);
                died = true;
            }
        }

        try
        {
            foreach (var response in changeSetResponses)
            {
                // TODO: only send to clients watching this player
                await Clients.All.SendAsync("ReceiveChangeSets", response).ConfigureAwait(false);
            }

            if (uiResponses != null)
            {
                foreach (var response in uiResponses)
                {
                    // TODO: only send to clients watching this player
                    await Clients.All.SendAsync("ReceiveUIRequest", response).ConfigureAwait(false);
                }
            }
        }
        finally
        {
            if (_defaultGameOptions.DisableSessionCache)
            {
                await DisposeSessionAsync(session).ConfigureAwait(false);
            }
            else if (died)
            {
                // TODO: Game over screen, disallow creating a new character with the same name
                var key = GetSessionCacheKey(playerName);
                _gameServices.SharedCache.Remove(key);
                await DisposeSessionAsync(session).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    ///     Returns the cached session for the given player name, optionally creating a
    ///     new character if none exists on disk. When <paramref name="createIfMissing" />
    ///     is <c>false</c>, returns <c>null</c> for players with no saved state. When it
    ///     is <c>true</c>, the result is always non-null.
    /// </summary>
    private async Task<GameSession?> GetSessionAsync(string playerName, bool createIfMissing)
    {
        if (_defaultGameOptions.DisableSessionCache)
        {
            return await LoadSessionAsync(playerName, createIfMissing).ConfigureAwait(false);
        }

        var key = GetSessionCacheKey(playerName);
        _gameServices.SharedCache.GetOrCreate(
            key, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(1);
                entry.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
                {
                    EvictionCallback = SessionEvictionCallback
                });
                return new Lazy<Task<GameSession?>>(
                    () => LoadSessionAsync(playerName, createIfMissing),
                    LazyThreadSafetyMode.ExecutionAndPublication);
            });
        var cached = (Lazy<Task<GameSession?>>?)_gameServices.SharedCache.Get(key);
        if (cached == null)
        {
            throw SessionUnavailable();
        }

        GameSession? session;
        try
        {
            session = await cached.Value.ConfigureAwait(false);
        }
        catch
        {
            _gameServices.SharedCache.Remove(key);
            throw;
        }

        if (session == null)
        {
            // Only reachable when createIfMissing == false and the player has no save.
            _gameServices.SharedCache.Remove(key);
            return null;
        }

        if (session.IsValidFor(playerName))
        {
            return session;
        }

        // Explicit-removal callbacks intentionally skip cleanup. Remove the stale cache entry first,
        // then await disposal of its materialized session before failing this invocation.
        _gameServices.SharedCache.Remove(key);
        await DisposeSessionAsync(session).ConfigureAwait(false);
        throw SessionUnavailable();
    }

    private static async void SessionEvictionCallback(
        object key, object? value, EvictionReason reason, object? state)
    {
        if (reason == EvictionReason.Removed)
        {
            // Every explicit removal site either has no materialized session (null/faulted Lazy) or
            // awaits its disposal after removal. Only automatic eviction disposes here.
            return;
        }

        if (value is Lazy<Task<GameSession?>> lazy && lazy.IsValueCreated)
        {
            try
            {
                if (await lazy.Value.ConfigureAwait(false) is { } session)
                {
                    await DisposeSessionAsync(session).ConfigureAwait(false);
                }
            }
            catch
            {
                // The load path owns cleanup when session construction fails.
            }
        }
    }

    private static async Task DisposeSessionAsync(GameSession session)
    {
        await session.MutationLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (session.IsDisposed)
            {
                return;
            }

            session.MarkDisposed();
            DetachAndUnwire(session.DbContext);
            await session.DisposeContextAsync().ConfigureAwait(false);
        }
        finally
        {
            session.MutationLock.Release();
        }
    }

    private static HubException SessionUnavailable()
        => new("The game session changed while processing the request. Retry the request.");

    private static (string, string) GetSessionCacheKey(string playerName)
        => ("GameSession", playerName.ToLowerInvariant());

    private async Task<GameSession?> LoadSessionAsync(string playerName, bool createIfMissing)
    {
        var dbContext = await _dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);
        var sessionLoaded = false;
        try
        {
            var session = await TryLoadSessionFromDbAsync(dbContext, playerName).ConfigureAwait(false);
            if (session != null)
            {
                sessionLoaded = true;
                return session;
            }
        }
        finally
        {
            if (!sessionLoaded)
            {
                DetachAndUnwire(dbContext);
                await dbContext.DisposeAsync().ConfigureAwait(false);
            }
        }

        return createIfMissing
            ? await CreateNewSessionAsync(playerName).ConfigureAwait(false)
            : null;
    }

    private async Task<GameSession?> TryLoadSessionFromDbAsync(
        GameDbContext dbContext, string playerName)
    {
        var loadedGame = await dbContext.PlayerComponents
            .Where(playerComponent => playerComponent.ProperName == playerName)
            .Join(dbContext.Games,
                playerComponent => playerComponent.GameId, game => game.Id,
                (_, game) => game)
            .AsNoTracking()
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        if (loadedGame == null)
        {
            return null;
        }

        InitializeGame(loadedGame);
        // Wire the DbContext into the game BEFORE we start touching entities, because
        // GameManager.CreateEntityNoReference performs Game.Repository.Add(entity) and
        // OnTracked needs DbContext.Manager set before SaveChanges fires the events.
        WireDbContext(dbContext, loadedGame);
        EntityAttacher.Attach(loadedGame, dbContext);
        loadedGame = await dbContext.LoadGameAsync(loadedGame.Id).ConfigureAwait(false);

        if (!dbContext.PlayerComponents.Local.Any(pc => pc.Entity.Being!.IsAlive))
        {
            // TODO: Preserve replays and game stats
            await dbContext.CleanAsync(loadedGame).ConfigureAwait(false);
            return null;
        }

        var player = dbContext.PlayerComponents.Local.First(
            p => string.Equals(p.ProperName, playerName, StringComparison.OrdinalIgnoreCase));

        var roomChangeBuilder = new RoomChangeBuilder();
        roomChangeBuilder.RegisterOnGroups(loadedGame.Manager);
        return new GameSession(dbContext, player, roomChangeBuilder);
    }

    private async Task<GameSession> CreateNewSessionAsync(string playerName)
    {
        var dbContext = await _dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);
        RoomChangeBuilder? roomChangeBuilder = null;
        try
        {
            uint seed;
            if (_defaultGameOptions.InitialSeed != 0)
            {
                seed = _defaultGameOptions.InitialSeed;
            }
            else
            {
                using var rng = RandomNumberGenerator.Create();
                var rngData = new byte[4];
                rng.GetBytes(rngData);
                seed = (uint)(rngData[0] | rngData[1] << 8 | rngData[2] << 16 | rngData[3] << 24);
            }

            var options = new GameOptions
            {
                InitialSeed = seed
            };
            var game = new Game { Options = options, Random = new SimpleRandom { Seed = options.InitialSeed } };
            InitializeGame(game);
            // Wire the DbContext into the game BEFORE adding/saving/generating, because
            // GameManager.CreateEntityNoReference performs Game.Repository.Add(entity) and
            // OnTracked needs DbContext.Manager set before SaveChanges fires the events.
            WireDbContext(dbContext, game);
            dbContext.Games.Add(game);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            var manager = game.Manager;
            var surfaceBranch = _gameServices.Data.Branches.Find("surface")!.Instantiate(game);
            var surfaceLevel = LevelGenerator.CreateEmpty(surfaceBranch, depth: 1, seed, manager);
            LevelGenerator.EnsureGenerated(surfaceLevel);
            var surfaceRoom = surfaceLevel.Rooms.First();

            var initialLevelConnection = surfaceRoom.Connections.Single().Value.Connection!;
            var initialLevelEntity = initialLevelConnection.TargetRoomEntity;
            LevelGenerator.EnsureGenerated(initialLevelEntity.Room!);

            // TODO: Set correct sex
            var playerEntity = PlayerRace.InstantiatePlayer(
                playerName, Sex.Male, initialLevelEntity.Room!, initialLevelConnection.TargetCell!.Value);

            manager.Queue.ProcessQueue(manager);

            // TODO: Only add these items if configured in the game options
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

            manager.Queue.ProcessQueue(manager);
            manager.LoggingSystem.WriteLog(game.Services.Language.Welcome(playerEntity), playerEntity, manager);

            // Run the first turn without collecting changes to seed initial state.
            roomChangeBuilder = new RoomChangeBuilder();
            roomChangeBuilder.RegisterOnGroups(game.Manager);
            GameStateManager.Turn(playerEntity.Player!, roomChangeBuilder, _gameServices,
                collectChanges: false);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            roomChangeBuilder.RegisterPlayerBuilder(playerEntity, manager);

            return new GameSession(dbContext, playerEntity.Player!, roomChangeBuilder);
        }
        catch
        {
            roomChangeBuilder?.UnregisterFromGroups(dbContext.Manager);
            DetachAndUnwire(dbContext);
            await dbContext.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    private void InitializeGame(Game game)
    {
        game.Services = _gameServices;
        // RevealLevels/RevealRooms are not persisted - apply current environment values
        // to every game on load and creation.
        game.Options.RevealLevels = _defaultGameOptions.RevealLevels;
        game.Options.RevealRooms = _defaultGameOptions.RevealRooms;
        if (game.Manager == null!)
        {
            var queue = new SequentialMessageQueue<GameEntity>();
            var gameManager = new GameManager { Game = game };
            gameManager.Initialize(queue);
            game.Manager = gameManager;
        }
    }

    /// <summary>
    ///     Connects a freshly-loaded or freshly-created <see cref="Game" /> to its owning
    ///     <see cref="GameDbContext" />. Must be called BEFORE any entity creation or
    ///     <see cref="DbContext.SaveChanges" />: <see cref="GameManager.CreateEntityNoReference" />
    ///     calls <c>Game.Repository.Add(entity)</c>, and <see cref="OnTracked" /> dereferences
    ///     <c>DbContext.Manager</c> for newly-tracked <see cref="GameEntity" /> instances
    ///     loaded from queries.
    /// </summary>
    private void WireDbContext(GameDbContext dbContext, Game game)
    {
        game.Repository = dbContext;
        dbContext.Manager = game.Manager;
        dbContext.ChangeTracker.Tracked += OnTracked;
        dbContext.ChangeTracker.StateChanged += OnStateChanged;
    }

    /// <summary>
    ///     Detaches every tracked entry while <see cref="OnStateChanged" /> is still subscribed, so the
    ///     <see cref="EntityState.Detached" /> transitions fire <c>ITrackable.StopTracking</c> — resetting
    ///     each entity/component tracking flag and releasing its EF-tracking reference — then removes the
    ///     change-tracker subscriptions. This MUST run before the pooled <see cref="GameDbContext" /> is
    ///     disposed: <see cref="DbContext.Dispose" /> returns the context to its pool and EF's pool reset
    ///     clears tracked entries WITHOUT raising <c>Detached</c>, so the release would otherwise be
    ///     skipped. Also used by the load/create error paths that dispose a wired context before a
    ///     <see cref="GameSession" /> exists; tolerant of never-wired or mid-fault contexts.
    /// </summary>
    private static void DetachAndUnwire(GameDbContext dbContext)
    {
        try
        {
            // TODO: Use a method that doesn't allocate a new list for each call
            foreach (var entry in dbContext.ChangeTracker.Entries().ToList())
            {
                entry.State = EntityState.Detached;
            }

            dbContext.ChangeTracker.Tracked -= OnTracked;
            dbContext.ChangeTracker.StateChanged -= OnStateChanged;
        }
        catch
        {
            // Tolerant of partial-dispose / never-wired / mid-fault contexts.
        }
    }

    private static void OnTracked(object? sender, EntityTrackedEventArgs args)
    {
        var entry = args.Entry;
        // This makes sure that the entity is deleted from the database when it's no longer needed..
        if (entry.Entity is ITrackable trackable)
        {
            trackable.StartTracking(sender!);
        }

        if (args.FromQuery
            && entry.Entity is GameEntity entity)
        {
            ((GameDbContext)entry.Context).Manager.AddEntity(entity);
        }
    }

    private static void OnStateChanged(object? sender, EntityStateChangedEventArgs args)
    {
        if (args.NewState == EntityState.Detached && args.Entry.Entity is ITrackable trackable)
        {
            trackable.StopTracking(sender!);
        }
    }
}
