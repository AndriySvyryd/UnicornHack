using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation;

/// <summary>
///     Test-only helpers over <see cref="LevelGenerationStats" /> for diffing failure
///     counters across a single <see cref="LevelLayout.BuildGraph" /> call. Lives in
///     the test assembly because it's purely a diagnostic convenience.
/// </summary>
internal static class LevelGenerationStatsTestExtensions
{
    /// <summary>
    ///     Returns a snapshot of every <c>RoomGraph*</c> failure counter keyed by counter
    ///     name (without the <c>RoomGraph</c> prefix), so tests can diff two snapshots
    ///     across a single <see cref="LevelLayout.BuildGraph" /> call to attribute
    ///     which failure happened.
    /// </summary>
    public static Dictionary<string, int> GraphFailureSnapshot(this LevelGenerationStats stats) => new()
    {
        [nameof(stats.RoomGraphPlacementTooSmall)] = stats.RoomGraphPlacementTooSmall,
        [nameof(stats.RoomGraphEntranceOrExitMissing)] = stats.RoomGraphEntranceOrExitMissing,
        [nameof(stats.RoomGraphTrimIsolatedEndpoint)] = stats.RoomGraphTrimIsolatedEndpoint,
        [nameof(stats.RoomGraphSpineNotFound)] = stats.RoomGraphSpineNotFound,
        [nameof(stats.RoomGraphEarNotFound)] = stats.RoomGraphEarNotFound,
        [nameof(stats.RoomGraphMinDegreeViolated)] = stats.RoomGraphMinDegreeViolated,
        [nameof(stats.RoomGraphDagConnectivityBroken)] = stats.RoomGraphDagConnectivityBroken
    };
}
