using UnicornHack.Data;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Systems.Actors;

namespace UnicornHack.Hubs;

/// <summary>
///     Long-lived per-character handle that owns a <see cref="GameDbContext" /> across
///     hub invocations. Cached in <c>GameServices.SharedCache</c> with a sliding
///     1-hour expiration; eviction (expiry / capacity) disposes the session, returning
///     the pooled <see cref="GameDbContext" /> to its pool. An explicit
///     <c>cache.Remove</c> by the safety-check path is intentionally NOT disposed by
///     the eviction callback so the caller can decide whether to dispose.
///     <para>
///         Pure value-bag: the change-tracker subscriptions and the
///         <see cref="RoomChangeBuilder" /> group registration live on the
///         <see cref="GameDbContext" /> and the cached game's <see cref="GameManager" />
///         respectively; <c>GameHub</c> wires them before constructing the session and
///         unwires them before disposing it. The per-game
///         <see cref="GameDbContext.RoomChangeBuilder" /> is the single source of truth
///         for the builder; this session exposes a convenience accessor over it.
///     </para>
/// </summary>
public sealed class GameSession : IDisposable
{
    private int _disposed;

    public GameSession(GameDbContext dbContext, PlayerComponent player)
    {
        DbContext = dbContext;
        Player = player;
    }

    public GameDbContext DbContext { get; }
    public PlayerComponent Player { get; }

    /// <summary>Convenience accessor; equivalent to <c>DbContext.RoomChangeBuilder!</c>.</summary>
    public RoomChangeBuilder RoomChangeBuilder => DbContext.RoomChangeBuilder!;

    /// <summary>Convenience accessor; equivalent to <c>Player.Game</c>.</summary>
    public Game Game => Player.Game;

    /// <summary>
    ///     Serializes hub invocations against this session. EF Core <see cref="DbContext" />
    ///     and the game's <see cref="GameManager" /> are not thread-safe, so all mutating
    ///     operations against a session must run under this semaphore. Read-only queries
    ///     (e.g. <see cref="GameHub.QueryGameAsync" />) may run lock-free since they only
    ///     touch in-memory ECS state.
    /// </summary>
    public SemaphoreSlim Sync { get; } = new(1, 1);

    public bool IsDisposed => Volatile.Read(ref _disposed) != 0;

    /// <summary>
    ///     Returns true if this session is still usable for the requested player name.
    ///     Used by the cache retrieval safety check: invalid if disposed or if the cached
    ///     player name does not match (defensive — shouldn't happen with name-keyed cache,
    ///     but guards against future key collapsing or rename scenarios).
    /// </summary>
    public bool IsValidFor(string requestedPlayerName)
        => !IsDisposed
           && string.Equals(Player.ProperName, requestedPlayerName, StringComparison.OrdinalIgnoreCase);

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        try
        {
            // Disposing the pooled DbContext returns it to the pool.
            DbContext.Dispose();
        }
        catch
        {
            // Same defensive handling as elsewhere — dispose ordering can race with
            // ChangeTracker.Tracked unsubscription on the GameHub side.
        }

        Sync.Dispose();
    }
}
