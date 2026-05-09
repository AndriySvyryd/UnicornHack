using System.ComponentModel.DataAnnotations.Schema;
using UnicornHack.Utils;

namespace UnicornHack.Systems.Levels;

/// <summary>
///     Generation diagnostics captured per <see cref="GameLevel" />. Persisted counters
///     are flat integer columns (mapped by EF as part of the owned
///     <see cref="GameLevel.GenerationStats" /> projection); regions group them by stage.
///     The in-memory dictionaries at the bottom are <see cref="NotMappedAttribute" /> —
///     they exist to answer "how many X were generated, and how many with tag/feature Y"
///     during a single generation run, and are not persisted.
/// </summary>
public class GenerationStats : NotificationEntity
{
    #region Level retry loop

    private int _levelAttempts;
    private int _validationFailures;

    /// <summary>
    ///     Total <see cref="LevelGenerator.EnsureGenerated" /> attempts before success.
    ///     Always &gt;= 1 on a generated level.
    /// </summary>
    public int LevelAttempts
    {
        get => _levelAttempts;
        set => SetWithNotify(value, ref _levelAttempts);
    }

    /// <summary>
    ///     Number of times a generated graph or lot layout failed validation and triggered
    ///     a retry.
    /// </summary>
    public int ValidationFailures
    {
        get => _validationFailures;
        set => SetWithNotify(value, ref _validationFailures);
    }

    #endregion

    #region Room graph

    private int _roomGraphAttempts;
    private int _roomGraphPlacementTooSmall;
    private int _roomGraphEntranceOrExitMissing;
    private int _roomGraphTrimIsolatedEndpoint;
    private int _roomGraphSpineNotFound;
    private int _roomGraphEarNotFound;
    private int _roomGraphMinDegreeViolated;
    private int _roomGraphDagConnectivityBroken;

    /// <summary>
    ///     Number of full room-graph generation passes attempted (one per level attempt
    ///     plus retries on validation failure).
    /// </summary>
    public int RoomGraphAttempts
    {
        get => _roomGraphAttempts;
        set => SetWithNotify(value, ref _roomGraphAttempts);
    }

    /// <summary><c>PlaceRooms</c> returned fewer than two nodes.</summary>
    public int RoomGraphPlacementTooSmall
    {
        get => _roomGraphPlacementTooSmall;
        set => SetWithNotify(value, ref _roomGraphPlacementTooSmall);
    }

    /// <summary>No entrance or no exit in the post-place node set.</summary>
    public int RoomGraphEntranceOrExitMissing
    {
        get => _roomGraphEntranceOrExitMissing;
        set => SetWithNotify(value, ref _roomGraphEntranceOrExitMissing);
    }

    /// <summary>After leaf trimming, entrance / exit got isolated or fewer than two nodes survived.</summary>
    public int RoomGraphTrimIsolatedEndpoint
    {
        get => _roomGraphTrimIsolatedEndpoint;
        set => SetWithNotify(value, ref _roomGraphTrimIsolatedEndpoint);
    }

    /// <summary>No simple entrance→exit path exists on the post-trim adjacency.</summary>
    public int RoomGraphSpineNotFound
    {
        get => _roomGraphSpineNotFound;
        set => SetWithNotify(value, ref _roomGraphSpineNotFound);
    }

    /// <summary>Ear decomposition couldn't cover all nodes (and restoration was exhausted).</summary>
    public int RoomGraphEarNotFound
    {
        get => _roomGraphEarNotFound;
        set => SetWithNotify(value, ref _roomGraphEarNotFound);
    }

    /// <summary>Defence-in-depth: a non-endpoint room ended up with fewer than two undirected neighbours.</summary>
    public int RoomGraphMinDegreeViolated
    {
        get => _roomGraphMinDegreeViolated;
        set => SetWithNotify(value, ref _roomGraphMinDegreeViolated);
    }

    /// <summary>Defence-in-depth: the directed graph failed the entrance→all / all→exit reachability check.</summary>
    public int RoomGraphDagConnectivityBroken
    {
        get => _roomGraphDagConnectivityBroken;
        set => SetWithNotify(value, ref _roomGraphDagConnectivityBroken);
    }

    #endregion

    #region Fragment placement

    private int _lotPlacementAttempts;
    private int _fragmentSelectionAttempts;

    /// <summary>
    ///     Number of attempts to place a fragment (lot) inside any room across the level.
    /// </summary>
    public int LotPlacementAttempts
    {
        get => _lotPlacementAttempts;
        set => SetWithNotify(value, ref _lotPlacementAttempts);
    }

    /// <summary>
    ///     Number of <see cref="UnicornHack.Generation.Map.DefiningMapFragment" /> selection attempts
    ///     (per-room fragment picks; some rooms may need multiple tries).
    /// </summary>
    public int FragmentSelectionAttempts
    {
        get => _fragmentSelectionAttempts;
        set => SetWithNotify(value, ref _fragmentSelectionAttempts);
    }

    #endregion

    #region Corridor routing

    private int _corridorRoutingAttempts;

    /// <summary>
    ///     Number of corridor-routing attempts (per pair of chunks needing connection).
    /// </summary>
    public int CorridorRoutingAttempts
    {
        get => _corridorRoutingAttempts;
        set => SetWithNotify(value, ref _corridorRoutingAttempts);
    }

    #endregion

    #region Instance counts (in-memory only)

    /// <summary>Map-fragment instance counts keyed by fragment name.</summary>
    [NotMapped]
    public Dictionary<string, int> FragmentInstancesByName { get; } = new();

    /// <summary>Map-fragment instance counts keyed by tag (one increment per tag on the picked fragment).</summary>
    [NotMapped]
    public Dictionary<string, int> FragmentInstancesByTag { get; } = new();

    /// <summary>
    ///     Fragment instance counts keyed by the <see cref="RoomFeatures" /> bit they were
    ///     picked for. A pick for a room with multiple feature bits increments every one.
    /// </summary>
    [NotMapped]
    public Dictionary<RoomFeatures, int> FragmentInstancesByFeature { get; } = new();

    /// <summary>Creature instance counts keyed by variant name.</summary>
    [NotMapped]
    public Dictionary<string, int> CreatureInstancesByName { get; } = new();

    /// <summary>
    ///     Creature instance counts keyed by each <see cref="CreatureGenerationFlags" />
    ///     bit set on the spawned variant.
    /// </summary>
    [NotMapped]
    public Dictionary<CreatureGenerationFlags, int> CreatureInstancesByFlag { get; } = new();

    /// <summary>Item instance counts keyed by variant name. Stacks count once, not per unit.</summary>
    [NotMapped]
    public Dictionary<string, int> ItemInstancesByName { get; } = new();

    /// <summary>
    ///     Item instance counts keyed by <see cref="Generation.ItemType" />. Items without an
    ///     explicit type are skipped.
    /// </summary>
    [NotMapped]
    public Dictionary<ItemType, int> ItemInstancesByType { get; } = new();

    #endregion

    #region Recording helpers

    /// <summary>
    ///     Returns a snapshot of every <c>RoomGraph*</c> failure counter keyed by counter
    ///     name (without the <c>RoomGraph</c> prefix), so tests can diff two snapshots
    ///     across a single <see cref="UnicornHack.Generation.Map.LevelLayout.BuildGraph" />
    ///     call to attribute which failure happened.
    /// </summary>
    public Dictionary<string, int> GraphFailureSnapshot() => new()
    {
        [nameof(RoomGraphPlacementTooSmall)] = RoomGraphPlacementTooSmall,
        [nameof(RoomGraphEntranceOrExitMissing)] = RoomGraphEntranceOrExitMissing,
        [nameof(RoomGraphTrimIsolatedEndpoint)] = RoomGraphTrimIsolatedEndpoint,
        [nameof(RoomGraphSpineNotFound)] = RoomGraphSpineNotFound,
        [nameof(RoomGraphEarNotFound)] = RoomGraphEarNotFound,
        [nameof(RoomGraphMinDegreeViolated)] = RoomGraphMinDegreeViolated,
        [nameof(RoomGraphDagConnectivityBroken)] = RoomGraphDagConnectivityBroken
    };

    /// <summary>
    ///     Records a placed map-fragment instance: bumps the per-name counter, the
    ///     per-tag counters for every tag on the fragment, and the per-feature counters
    ///     for every set bit in <paramref name="roomFeatures" />.
    /// </summary>
    public void RecordFragmentInstance(
        string name,
        IEnumerable<string>? tags,
        RoomFeatures roomFeatures)
    {
        Increment(FragmentInstancesByName, name);
        if (tags is not null)
        {
            foreach (var tag in tags)
            {
                Increment(FragmentInstancesByTag, tag);
            }
        }
        foreach (var feature in EnumerateSetBits(roomFeatures))
        {
            Increment(FragmentInstancesByFeature, feature);
        }
    }

    /// <summary>
    ///     Records a spawned creature instance: bumps the per-name counter and the
    ///     per-flag counters for every set bit in <paramref name="flags" />.
    /// </summary>
    public void RecordCreatureInstance(string name, CreatureGenerationFlags flags)
    {
        Increment(CreatureInstancesByName, name);
        foreach (var flag in EnumerateSetBits(flags))
        {
            Increment(CreatureInstancesByFlag, flag);
        }
    }

    /// <summary>
    ///     Records a placed item instance (one event per <c>Instantiate</c> call regardless
    ///     of stack size). Variants without an <see cref="ItemType" /> contribute only to
    ///     the per-name counter.
    /// </summary>
    public void RecordItemInstance(string name, ItemType? type)
    {
        Increment(ItemInstancesByName, name);
        if (type is { } actualType)
        {
            Increment(ItemInstancesByType, actualType);
        }
    }

    #endregion

    private static void Increment<TKey>(Dictionary<TKey, int> bucket, TKey key) where TKey : notnull
    {
        bucket[key] = bucket.TryGetValue(key, out var count) ? count + 1 : 1;
    }

    private static IEnumerable<RoomFeatures> EnumerateSetBits(RoomFeatures value)
    {
        foreach (RoomFeatures flag in Enum.GetValues(typeof(RoomFeatures)))
        {
            if (flag != RoomFeatures.Normal && (value & flag) == flag)
            {
                yield return flag;
            }
        }
    }

    private static IEnumerable<CreatureGenerationFlags> EnumerateSetBits(CreatureGenerationFlags value)
    {
        foreach (CreatureGenerationFlags flag in Enum.GetValues(typeof(CreatureGenerationFlags)))
        {
            if (flag != CreatureGenerationFlags.None && (value & flag) == flag)
            {
                yield return flag;
            }
        }
    }
}
