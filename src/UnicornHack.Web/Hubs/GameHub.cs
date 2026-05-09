using System.Collections;
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
        await session.Sync.WaitAsync(60_000).ConfigureAwait(false);
        try
        {
            var gameState = GameStateManager.GetState(session.Player.Entity, _gameServices);
            Debug.Assert(session.DbContext.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
            return gameState;
        }
        finally
        {
            session.Sync.Release();
        }
    }

    public async Task ShowDialog(string playerName, int intQueryType, int[] arguments)
    {
        var results = (GameQueryType)intQueryType == GameQueryType.Clear
            ? SerializationContext.DeletedBitArray
            : await QueryGameAsync(playerName, intQueryType, arguments).ConfigureAwait(false);

        // TODO: only send to clients watching this player
        await Clients.All.SendAsync("ReceiveUIRequest", results).ConfigureAwait(false);
    }

    public Task<List<object?>> QueryGame(string playerName, int intQueryType, int[] arguments)
        => QueryGameAsync(playerName, intQueryType, arguments);

    private async Task<List<object?>> QueryGameAsync(string playerName, int intQueryType, int[] arguments)
    {
        var queryType = (GameQueryType)intQueryType;
        if (queryType == GameQueryType.Clear)
        {
            return SerializationContext.DeletedBitArray;
        }

        var session = await GetSessionAsync(playerName, createIfMissing: false).ConfigureAwait(false);
        if (session == null)
        {
            return SerializationContext.DeletedBitArray;
        }

        if (session.Player.Entity.Being!.IsAlive != true)
        {
            return SerializationContext.DeletedBitArray;
        }

        // Wait for any in-flight mutation to finish so we don't read torn state,
        // then release immediately: this query is read-only and should not block
        // other readers or the player's next action.
        await session.Sync.WaitAsync().ConfigureAwait(false);
        session.Sync.Release();

        return QueryGameInternal(session, intQueryType, queryType, arguments);
    }

    private List<object?> QueryGameInternal(
        GameSession session, int intQueryType, GameQueryType queryType, int[] arguments)
    {
        var player = session.Player;
        var result = new List<object?> { null };
        var setValues = new bool[13];
        setValues[intQueryType + 1] = true;
        if (queryType == GameQueryType.SlottableAbilities)
        {
            // SlottableAbilities tags an extra bit at intQueryType to mark that a slot id follows.
            setValues[intQueryType] = true;
        }
        result[0] = new BitArray(setValues);

        var manager = player.Entity.Manager;
        var context = new SerializationContext(session.DbContext, player.Entity, _gameServices);
        switch (queryType)
        {
            case GameQueryType.SlottableAbilities:
                var slot = arguments[0];
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
                            abilities.Add(ability.EntityId, GameQueryProcessor.SerializeSlottedAbility(slottableAbilityEntity, context)!);
                        }
                    }
                    else if (slot == AbilitySlottingSystem.DefaultRangedAttackSlot)
                    {
                        if (ability.Type == AbilityType.DefaultAttack
                            && ((WieldingAbility)ability.Template!).ItemType == ItemType.WeaponRanged)
                        {
                            abilities.Add(ability.EntityId, GameQueryProcessor.SerializeSlottedAbility(slottableAbilityEntity, context)!);
                        }
                    }
                    else if ((ability.Activation & ActivationType.Slottable) != 0
                             && ability.Type != AbilityType.DefaultAttack)
                    {
                        abilities.Add(ability.EntityId, GameQueryProcessor.SerializeSlottedAbility(slottableAbilityEntity, context)!);
                    }
                }

                break;
            case GameQueryType.PlayerAttributes:
                result.Add(GameQueryProcessor.SerializeActorAttributes(player.Entity, true, context));
                break;
            case GameQueryType.PlayerInventory:
                result.Add(GameQueryProcessor.SerializeItems(player.Entity, context));
                break;
            case GameQueryType.PlayerAdaptations:
                result.Add(GameQueryProcessor.SerializeAdaptations(player.Entity, context));
                break;
            case GameQueryType.PlayerSkills:
                result.Add(GameQueryProcessor.SerializeSkills(player.Entity, context));
                break;
            case GameQueryType.AbilityAttributes:
                var abilityEntity = manager.FindEntity(arguments[0])!;

                var ownerEntity = abilityEntity.Ability!.OwnerEntity!;
                var activatorEntity = abilityEntity.Ability!.OwnerEntity!.HasComponent(EntityComponent.Item)
                    ? player.Entity!
                    : ownerEntity;

                result.Add(GameQueryProcessor.SerializeAbilityAttributes(abilityEntity, activatorEntity, context));
                break;
            case GameQueryType.ActorAttributes:
            {
                var actorKnowledge = manager.FindEntity(arguments[0])?.Knowledge;

                result.Add(GameQueryProcessor.SerializeActorAttributes(
                    actorKnowledge?.KnownEntity, actorKnowledge?.IsIdentified ?? false, context));
                break;
            }
            case GameQueryType.ItemAttributes:
                var itemEntity = manager.FindEntity(arguments[0]);
                var item = itemEntity?.Item;
                var itemKnowledge = itemEntity?.Knowledge;

                if (item != null)
                {
                    result.Add(GameQueryProcessor.SerializeItemAttributes(itemEntity, true, context));
                }
                else
                {
                    result.Add(GameQueryProcessor.SerializeItemAttributes(
                        itemKnowledge?.KnownEntity, itemKnowledge?.IsIdentified ?? false, context));
                }

                break;
            case GameQueryType.RoomMap:
                result.Add(GameQueryProcessor.SerializeRoomMap(player.Entity));
                break;
            // TODO: Handle PostGameStatistics
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
        var session = (await GetSessionAsync(playerName, createIfMissing: true).ConfigureAwait(false))!;

        Dictionary<int, List<TurnChangeSet>> changeSets;
        int previousTick;
        await session.Sync.WaitAsync().ConfigureAwait(false);
        try
        {
            if (session.Game.CurrentTick != session.Player.NextActionTick)
            {
                // TODO: Log action sent out of turn instead
                throw new InvalidOperationException("Action sent out of turn");
            }

            session.Player.NextAction = action;
            session.Player.NextActionTarget = target;
            session.Player.NextActionTarget2 = target2;

            previousTick = session.Game.CurrentTick;
            changeSets = GameStateManager.Turn(
                session.Player, session.RoomChangeBuilder, _gameServices,
                dbContext: session.DbContext);

            session.DbContext.SaveChanges();

            // TODO: Log turn ended prematurely
            Debug.Assert(session.Game.CurrentTick == session.Player.NextActionTick, "Turn ended prematurely");
        }
        finally
        {
            session.Sync.Release();
        }

        // Sending happens after releasing the per-session lock to allow concurrent
        // ReceiveChangeSets emission to multiple players without serializing through
        // the per-session semaphore.
        var manager = session.Game.Manager;
        var died = false;
        foreach (var playerEntity in manager.Players)
        {
            if (!changeSets.TryGetValue(playerEntity.Id, out var playerChangeSets)
                && previousTick != session.Game.CurrentTick)
            {
                throw new InvalidOperationException(
                    $"No change sets for player {playerEntity.Id}. "
                    + $"Tick: {manager.Game.CurrentTick}. "
                    + $"NextActionTick: {session.Player.NextActionTick}.");
            }

            // TODO: only send to clients watching this player
            await Clients.All.SendAsync("ReceiveChangeSets", playerChangeSets ?? []).ConfigureAwait(false);

            if (!playerEntity.Being!.IsAlive)
            {
                var results = await QueryGameAsync(playerName, (int)GameQueryType.PostGameStatistics, []).ConfigureAwait(false);
                // TODO: only send to clients watching this player
                await Clients.All.SendAsync("ReceiveUIRequest", results).ConfigureAwait(false);
                died = true;
            }
        }

        if (died)
        {
            // Drop the session so a subsequent PerformAction creates a fresh character.
            // TODO: Game over screen, disallow creating a new character with the same name
            EvictSession(playerName, disposeNow: true);
        }
        else if (_defaultGameOptions.DisableSessionCache)
        {
            // Diagnostic mode: force the next hub call to reload from the database
            // so we exercise the LoadRooms query path on every action.
            EvictSession(playerName, disposeNow: true);
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
        var key = GetSessionCacheKey(playerName);
        while (true)
        {
            _gameServices.SharedCache.GetOrCreate(
                key, entry => CreateSessionCacheEntry(entry, playerName, createIfMissing));
            var cached = (Lazy<GameSession?>?)_gameServices.SharedCache.Get(key);
            if (cached == null)
            {
                // Eviction raced us between insert and read; retry.
                continue;
            }

            GameSession? session;
            try
            {
                session = await Task.Run(() => cached.Value).ConfigureAwait(false);
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

            // Safety check failed: remove (don't dispose — PostEvictionCallback recognizes
            // EvictionReason.Removed and skips disposal) then retry.
            _gameServices.SharedCache.Remove(key);
        }
    }

    private Lazy<GameSession?> CreateSessionCacheEntry(ICacheEntry entry, string playerName, bool createIfMissing)
    {
        entry.SlidingExpiration = TimeSpan.FromHours(1);
        entry.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
        {
            EvictionCallback = SessionEvictionCallback
        });
        return new Lazy<GameSession?>(
            () => LoadSession(playerName, createIfMissing),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private static void SessionEvictionCallback(object key, object? value, EvictionReason reason, object? state)
    {
        if (reason == EvictionReason.Removed)
        {
            // Explicit cache.Remove, the caller decides
            // whether to dispose; the eviction callback intentionally leaves the session alive.
            return;
        }

        if (value is Lazy<GameSession?> lazy && lazy.IsValueCreated && lazy.Value is { } session)
        {
            UnwireSession(session);
            session.Dispose();
        }
    }

    private void EvictSession(string playerName, bool disposeNow)
    {
        var key = GetSessionCacheKey(playerName);
        var cached = (Lazy<GameSession?>?)_gameServices.SharedCache.Get(key);
        _gameServices.SharedCache.Remove(key);
        if (disposeNow && cached is { IsValueCreated: true } && cached.Value is { } session)
        {
            UnwireSession(session);
            session.Dispose();
        }
    }

    private static (string, string) GetSessionCacheKey(string playerName)
        => ("GameSession", playerName.ToLowerInvariant());

    /// <summary>
    ///     Loads an existing player from DB into a fresh long-lived DbContext, or returns
    ///     null if no such player exists or the player is dead. When
    ///     <paramref name="createIfMissing" /> is <c>true</c>, falls through to
    ///     <see cref="CreateNewSession" /> in that case.
    /// </summary>
    private GameSession? LoadSession(string playerName, bool createIfMissing)
    {
        var dbContext = _dbContextFactory.CreateDbContext();
        try
        {
            var session = TryLoadSessionFromDb(dbContext, playerName);
            if (session != null)
            {
                return session;
            }

            dbContext.Dispose();
        }
        catch
        {
            dbContext.Dispose();
            throw;
        }

        return createIfMissing ? CreateNewSession(playerName) : null;
    }

    private GameSession? TryLoadSessionFromDb(GameDbContext dbContext, string playerName)
    {
        var loadedGame = dbContext.PlayerComponents
            .Where(playerComponent => playerComponent.ProperName == playerName)
            .Join(dbContext.Games,
                playerComponent => playerComponent.GameId, game => game.Id,
                (_, game) => game)
            .AsNoTracking()
            .FirstOrDefault();

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
        loadedGame = dbContext.LoadGame(loadedGame.Id);

        if (!dbContext.PlayerComponents.Local.Any(pc => pc.Entity.Being!.IsAlive))
        {
            // TODO: Preserve replays and game stats
            dbContext.Clean(loadedGame);
            return null;
        }

        var player = dbContext.PlayerComponents.Local.First(
            p => p.ProperName.Equals(playerName, StringComparison.OrdinalIgnoreCase));

        var roomChangeBuilder = GetOrCreateRoomChangeBuilder(loadedGame);
        dbContext.RoomChangeBuilder = roomChangeBuilder;
        return new GameSession(dbContext, player);
    }

    private GameSession CreateNewSession(string playerName)
    {
        var dbContext = _dbContextFactory.CreateDbContext();
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
            dbContext.SaveChanges();

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
            var bootstrapBuilder = GetOrCreateRoomChangeBuilder(game);
            dbContext.RoomChangeBuilder = bootstrapBuilder;
            GameStateManager.Turn(playerEntity.Player!, bootstrapBuilder, _gameServices,
                collectChanges: false);
            dbContext.SaveChanges();

            bootstrapBuilder.RegisterPlayerBuilder(playerEntity, manager);
            SeedRoomChangeBuilderCache(game, bootstrapBuilder);
            _gameServices.SharedCache.Set(game, game,
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1)));

            return new GameSession(dbContext, playerEntity.Player!);
        }
        catch
        {
            dbContext.Dispose();
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
            var queue = new SequentialMessageQueue<GameManager>();
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
    ///     Inverse of <see cref="WireDbContext" />. Detaches change-tracker subscriptions
    ///     and removes the per-game <see cref="RoomChangeBuilder" /> from its
    ///     <see cref="GameManager" /> groups before the session is disposed. Safe to call
    ///     during eviction (or any cleanup path) and tolerant of partial-dispose races.
    ///     <para>
    ///         <see cref="OnTracked" />/<see cref="OnStateChanged" /> are static methods,
    ///         so subscribing/unsubscribing by method group yields delegates that are
    ///         considered equal — no need to cache the delegate instance on the session.
    ///     </para>
    /// </summary>
    private static void UnwireSession(GameSession session)
    {
        try
        {
            session.DbContext.ChangeTracker.Tracked -= OnTracked;
            session.DbContext.ChangeTracker.StateChanged -= OnStateChanged;
        }
        catch
        {
            // Ignore if the context is already in a partial-dispose state.
        }

        try
        {
            session.RoomChangeBuilder.UnregisterFromGroups(session.Game.Manager);
        }
        catch
        {
            // Same defensive handling.
        }
    }

    /// <summary>
    ///     Returns the per-game <see cref="RoomChangeBuilder" />, registering listeners on
    ///     <see cref="Game.Manager" /> exactly once across all sessions. Synchronization
    ///     and single-initialization are provided by <see cref="IMemoryCache" /> +
    ///     <see cref="Lazy{T}" /> with <see cref="LazyThreadSafetyMode.ExecutionAndPublication" />:
    ///     even if two threads race past the GetOrCreate insert, both re-read the canonical
    ///     entry and converge on a single <c>Lazy</c> whose factory runs at most once.
    /// </summary>
    private RoomChangeBuilder GetOrCreateRoomChangeBuilder(Game game)
    {
        if (game.Id == 0)
        {
            var bootstrap = new RoomChangeBuilder();
            bootstrap.RegisterOnGroups(game.Manager);
            return bootstrap;
        }

        var key = GetRoomChangeBuilderCacheKey(game);

        _gameServices.SharedCache.GetOrCreate(key, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);
            return new Lazy<RoomChangeBuilder>(() =>
            {
                var builder = new RoomChangeBuilder();
                builder.RegisterOnGroups(game.Manager);
                return builder;
            }, LazyThreadSafetyMode.ExecutionAndPublication);
        });

        return ((Lazy<RoomChangeBuilder>)_gameServices.SharedCache.Get(key)!).Value;
    }

    private void SeedRoomChangeBuilderCache(Game game, RoomChangeBuilder builder)
    {
        Debug.Assert(game.Id != 0, "SeedRoomChangeBuilderCache called before SaveChanges");
        var lazy = new Lazy<RoomChangeBuilder>(() => builder, LazyThreadSafetyMode.PublicationOnly);
        _ = lazy.Value;
        _gameServices.SharedCache.Set(GetRoomChangeBuilderCacheKey(game), lazy,
            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1)));
    }

    private static (string, int) GetRoomChangeBuilderCacheKey(Game game) => ("RoomChangeBuilder", game.Id);

    private static void OnTracked(object? sender, EntityTrackedEventArgs args)
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

    private static void OnStateChanged(object? sender, EntityStateChangedEventArgs args)
    {
        if (args.NewState == EntityState.Detached && args.Entry.Entity is ITrackable trackable)
        {
            trackable.StopTracking(sender!);
        }
    }
}
