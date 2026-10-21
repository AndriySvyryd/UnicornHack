using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Memory;
using System.Runtime.CompilerServices;
using UnicornHack.Data;
using UnicornHack.Generation;
using UnicornHack.Services;
using UnicornHack.Services.English;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.TestUtilities;

/// <summary>
///     Seeds a playable game into a throwaway SQL Server LocalDB database exactly the way
///     <c>GameHub.CreateNewSession</c> does, persists it, and reloads it into a fresh context on the same
///     database - the round-trip that validates effect-system state (including recomputed base stats)
///     survives save/load. LocalDB is used because it is the production provider, so the model, key
///     generation and the <c>LoadGame</c> queries behave identically to production. Query splitting is
///     enabled to match production (without it the 14-Include <c>LoadRooms</c> load collapses into one
///     cartesian query that never completes).
/// </summary>
public sealed class PersistedGame : IDisposable
{
    private readonly string _connectionString;
    private bool _disposed;

    private PersistedGame(string connectionString)
    {
        _connectionString = connectionString;
        Context = null!;
        Game = null!;
    }

    public GameDbContext Context
    {
        get;
        private set;
    }

    public Game Game
    {
        get;
        private set;
    }

    public int PlayerEntityId
    {
        get;
        private set;
    }

    public GameEntity PlayerEntity => Game.Manager.FindEntity(PlayerEntityId)!;

    /// <summary>
    ///     Whether SQL Server LocalDB (the production provider) can be reached in this environment, so a
    ///     round-trip test can skip gracefully where it is not installed (e.g. some CI agents).
    /// </summary>
    public static bool IsLocalDbAvailable { get; } = ProbeLocalDb();

    private static bool ProbeLocalDb()
    {
        try
        {
            using var connection = new SqlConnection(
                @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=True;Connect Timeout=30;Pooling=False;");
            connection.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static PersistedGame CreateAndSave(
        string playerName = "Dudley", uint seed = 1,
        [CallerFilePath] string testClassFile = "")
    {
        var className = Path.GetFileNameWithoutExtension(testClassFile);
        var connectionString =
            $@"Server=(localdb)\MSSQLLocalDB;Database=UnicornHackTest_{className};Integrated Security=True;Connect Timeout=60;";

        var persisted = new PersistedGame(connectionString);
        persisted.Seed(playerName, seed);
        return persisted;
    }

    private void Seed(string playerName, uint seed)
    {
        var game = new Game
        {
            Options = { InitialSeed = seed },
            Random = new SimpleRandom { Seed = seed },
            Services = new GameServices(
                new EnglishLanguageService(), new MemoryCache(new MemoryCacheOptions()), new TestDataProvider())
        };

        InitializeGame(game);
        var context = CreateContext();
        var wired = false;
        try
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            WireDbContext(context, game);
            wired = true;

            context.Games.Add(game);
            context.SaveChanges();

            var manager = game.Manager;
            var playerRoom = TestHelper.BuildRoom(@"
.....
.....
.....", game);
            var retainedRoom = TestHelper.BuildRoom("...", game, depth: 1);

            using (var connectionReference = manager.CreateEntity())
            {
                var connectionEntity = connectionReference.Referenced;
                var position = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
                position.RoomEntity = playerRoom.Entity;
                position.Cell = new Point(0, 0);
                connectionEntity.Position = position;

                var connection = manager.CreateComponent<ConnectionComponent>(EntityComponent.Connection);
                connection.TargetRoomId = retainedRoom.EntityId;
                connection.TargetCell = new Point(0, 0);
                connection.Directionality = Directionality.Source;
                connectionEntity.Connection = connection;
            }

            var playerEntity = PlayerRace.InstantiatePlayer(
                playerName, Sex.Male, playerRoom, new Point(2, 1));

            manager.Queue.ProcessQueue(manager);
            context.SaveChanges();

            Context = context;
            Game = game;
            PlayerEntityId = playerEntity.Id;
        }
        catch
        {
            if (wired)
            {
                Unwire(context);
            }

            context.Dispose();
            throw;
        }
    }

    /// <summary>
    ///     Disposes the current context and reloads the game into a fresh one against the same database,
    ///     mirroring <c>GameHub.TryLoadSessionFromDb</c>. After this returns, <see cref="Game" /> and
    ///     <see cref="Context" /> refer to the reloaded instances.
    /// </summary>
    public void Reload(Action<TestDataProvider>? configureData = null)
        => Reload(useAsync: false, configureData: configureData).GetAwaiter().GetResult();

    public ValueTask ReloadAsync() => Reload(useAsync: true);

    public void ReloadRooms(IReadOnlyList<int> roomIds)
        => Reload(useAsync: false, roomIds).GetAwaiter().GetResult();

    private async ValueTask Reload(
        bool useAsync, IReadOnlyList<int>? roomIds = null, Action<TestDataProvider>? configureData = null)
    {
        var tracked = Context.ChangeTracker.Entries()
            .Select(entry => entry.Entity).OfType<ITrackable>().ToArray();
        Context.ChangeTracker.Clear();
        foreach (var entity in tracked)
        {
            entity.StopTracking(Context.ChangeTracker);
        }

        Unwire(Context);
        if (useAsync)
        {
            await Context.DisposeAsync();
        }
        else
        {
            Context.Dispose();
        }

        var context = CreateContext();
        var loadedGame = useAsync
            ? await context.Games.FirstAsync()
            : context.Games.First();

        InitializeGame(loadedGame);
        configureData?.Invoke((TestDataProvider)loadedGame.Services.Data);
        WireDbContext(context, loadedGame);
        EntityAttacher.Attach(loadedGame, context);

        if (roomIds == null)
        {
            loadedGame = useAsync
                ? await context.LoadGameAsync(loadedGame.Id)
                : context.LoadGame(loadedGame.Id);
        }
        else if (useAsync)
        {
            await context.LoadRoomsAsync(roomIds);
        }
        else
        {
            context.LoadRooms(roomIds);
        }

        Context = context;
        Game = loadedGame;
    }

    private GameDbContext CreateContext()
    {
        var context = new GameDbContext(BuildOptions(_connectionString));
        context.Database.SetCommandTimeout(120);
        return context;
    }

    private static DbContextOptions<GameDbContext> BuildOptions(string connectionString)
        => new DbContextOptionsBuilder<GameDbContext>()
            .UseSqlServer(connectionString, sql => sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
            .AddInterceptors(PooledMaterializationInterceptor.Instance)
            .Options;

    private static void InitializeGame(Game game)
    {
        // Services are runtime (not persisted), so a freshly loaded game needs them re-attached, exactly
        // as GameHub.InitializeGame does for a real session.
        game.Services ??= new GameServices(
            new EnglishLanguageService(), new MemoryCache(new MemoryCacheOptions()), new TestDataProvider());

        if (game.Manager == null!)
        {
            var manager = new GameManager { Game = game };
            manager.Initialize(new SequentialMessageQueue<GameEntity>());
            game.Manager = manager;
        }
    }

    private void WireDbContext(GameDbContext context, Game game)
    {
        game.Repository = context;
        context.Manager = game.Manager;
        context.ChangeTracker.Tracked += OnTracked;
        context.ChangeTracker.StateChanged += OnStateChanged;
    }

    private void Unwire(GameDbContext context)
    {
        context.ChangeTracker.Tracked -= OnTracked;
        context.ChangeTracker.StateChanged -= OnStateChanged;
    }

    private void OnTracked(object? sender, EntityTrackedEventArgs args)
    {
        var entry = args.Entry;
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

    private void OnStateChanged(object? sender, EntityStateChangedEventArgs args)
    {
        if (args.NewState == EntityState.Detached
            && args.Entry.Entity is ITrackable trackable)
        {
            trackable.StopTracking(sender!);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        try
        {
            Unwire(Context);
        }
        catch (ObjectDisposedException)
        {
        }

        Context.Dispose();

        try
        {
            using var cleanup = CreateContext();
            cleanup.Database.EnsureDeleted();
        }
        catch
        {
            // Best-effort cleanup of the disposable per-test-class database.
        }
    }
}
