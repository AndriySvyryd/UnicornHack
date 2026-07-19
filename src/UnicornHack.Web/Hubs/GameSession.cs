using UnicornHack.Data;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Systems.Actors;

namespace UnicornHack.Hubs;

/// <summary>
///     Long-lived per-character handle that owns a <see cref="GameDbContext" /> across
///     hub invocations. Cached in <c>GameServices.SharedCache</c> with a sliding
///     1-hour expiration; eviction (expiry / capacity) disposes the session, returning
///     the pooled <see cref="GameDbContext" /> to its pool. Explicit cache removals
///     await disposal of their materialized session at the removal site; their
///     eviction callback deliberately does nothing to avoid duplicate or deferred cleanup.
///     Owns all runtime state associated with the loaded game: the game graph, player,
///     change builder, database context and synchronization boundary.
/// </summary>
public sealed class GameSession
{
    private int _disposed;

    public GameSession(
        GameDbContext dbContext, PlayerComponent player, RoomChangeBuilder roomChangeBuilder)
    {
        DbContext = dbContext;
        Player = player;
        RoomChangeBuilder = roomChangeBuilder;
    }

    public GameDbContext DbContext { get; }
    public PlayerComponent Player { get; }
    public RoomChangeBuilder RoomChangeBuilder { get; }

    /// <summary>
    ///     Serializes turns and disposal, including their asynchronous database work.
    ///     Read-only serialization does not acquire this lock.
    /// </summary>
    public SemaphoreSlim MutationLock { get; } = new(1, 1);

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

    internal void MarkDisposed() => Volatile.Write(ref _disposed, 1);

    internal async ValueTask DisposeContextAsync()
    {
        try
        {
            RoomChangeBuilder.UnregisterFromGroups(Player.Game.Manager);
        }
        catch
        {
            // Tolerate partially initialized or already-unregistered builders.
        }

        try
        {
            // Disposing the pooled DbContext returns it to the pool.
            await DbContext.DisposeAsync().ConfigureAwait(false);
        }
        catch
        {
            // Same defensive handling as elsewhere — dispose ordering can race with
            // ChangeTracker.Tracked unsubscription on the GameHub side.
        }
    }
}
