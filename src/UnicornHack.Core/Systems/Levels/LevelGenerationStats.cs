using System.Collections.ObjectModel;

namespace UnicornHack.Systems.Levels;

/// <summary>
///     Per-<see cref="GameLevel" /> generation diagnostics. Adds the retry-loop,
///     room-graph, fragment-placement, and corridor-routing counters that only make
///     sense for a single level generation pass on top of the cumulative instance
///     dictionaries inherited from <see cref="GenerationStats" />. Persisted as an
///     owned entity through <c>GameLevel.GenerationStats</c>.
/// </summary>
public class LevelGenerationStats : GenerationStats
{
    #region Level retry loop

    private int _levelAttempts;
    private int _validationFailures;

    /// <summary>
    ///     Total <see cref="UnicornHack.Generation.LevelGenerator.EnsureGenerated(GameLevel)" />
    ///     attempts before success. Always &gt;= 1 on a generated level.
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

    private readonly ObservableCollection<KeyCountPair<string>> _fragmentRoomFailuresEntries = [];
    private readonly ObservableCollection<KeyCountPair<string>> _fragmentInstancesByVariantEntries = [];

    private readonly Dictionary<string, int> _fragmentRoomFailures = [];
    private readonly Dictionary<string, int> _fragmentInstancesByVariant = [];

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

    /// <summary>
    ///     Counts the number of times a room failed to generate after a fragment was placed
    ///     in it, keyed by fragment <see cref="UnicornHack.Generation.Map.MapFragment.SourceName" />.
    ///     Both the defining fragment that the room was built around and every chunk fragment
    ///     placed prior to the failure are credited (independently incremented).
    /// </summary>
    public Dictionary<string, int> FragmentRoomFailures
    {
        get { EnsureDictionariesInitialized(); return _fragmentRoomFailures; }
    }

    /// <summary>
    ///     Counts placed fragment instances keyed by fragment
    ///     <see cref="UnicornHack.Generation.Map.MapFragment.Name" /> — i.e. with the
    ///     <see cref="UnicornHack.Generation.Map.MapFragment.Transform" /> suffix
    ///     (rotation / flip) preserved. Complements
    ///     <see cref="GenerationStats.FragmentInstancesByName" /> which is keyed by
    ///     <c>SourceName</c> and drives weight expressions. Used by diagnostic probes
    ///     to see which transformed variants are picked vs. dormant.
    /// </summary>
    public Dictionary<string, int> FragmentInstancesByVariant
    {
        get { EnsureDictionariesInitialized(); return _fragmentInstancesByVariant; }
    }

    // Entries collections (same pattern as the base class) — EF persists these via
    // field-mode owned navigations registered in GameDbContext.
    internal ObservableCollection<KeyCountPair<string>> FragmentRoomFailuresEntries
        => _fragmentRoomFailuresEntries;

    internal ObservableCollection<KeyCountPair<string>> FragmentInstancesByVariantEntries
        => _fragmentInstancesByVariantEntries;

    /// <summary>
    ///     Increments <see cref="FragmentRoomFailures" /> for <paramref name="sourceName" />.
    /// </summary>
    public void RecordFragmentRoomFailure(string sourceName)
    {
        FragmentRoomFailures.TryGetValue(sourceName, out var count);
        FragmentRoomFailures[sourceName] = count + 1;
    }

    /// <summary>
    ///     Increments <see cref="FragmentInstancesByVariant" /> for <paramref name="variantName" />.
    /// </summary>
    public void RecordFragmentVariant(string variantName)
    {
        FragmentInstancesByVariant.TryGetValue(variantName, out var count);
        FragmentInstancesByVariant[variantName] = count + 1;
    }

    #endregion

    #region Room layout demand failures

    private int _roomLayoutUnboundFeatures;
    private int _roomLayoutInsufficientDoorways;
    private int _roomLayoutConnectivityRepairFailed;

    /// <summary>
    ///     A room layout was rejected by
    ///     <see cref="UnicornHack.Generation.Map.RoomLayout.SatisfiesDemand" /> because the
    ///     fragments placed in it failed to bind every required placeable feature
    ///     (e.g. an altar / fountain glyph was demanded but never written).
    /// </summary>
    public int RoomLayoutUnboundFeatures
    {
        get => _roomLayoutUnboundFeatures;
        set => SetWithNotify(value, ref _roomLayoutUnboundFeatures);
    }

    /// <summary>
    ///     A room layout was rejected by
    ///     <see cref="UnicornHack.Generation.Map.RoomLayout.SatisfiesDemand" /> because the
    ///     placed chunks together exposed fewer doorway tiles than the inter-room edge
    ///     demand required.
    /// </summary>
    public int RoomLayoutInsufficientDoorways
    {
        get => _roomLayoutInsufficientDoorways;
        set => SetWithNotify(value, ref _roomLayoutInsufficientDoorways);
    }

    /// <summary>
    ///     A room layout was rejected because
    ///     <see cref="UnicornHack.Generation.Map.RoomLayout.TryRepairConnectivity" />
    ///     couldn't carve a corridor between two disconnected chunks (typically because
    ///     no chunk with doorway points was reachable from the unreached chunk).
    /// </summary>
    public int RoomLayoutConnectivityRepairFailed
    {
        get => _roomLayoutConnectivityRepairFailed;
        set => SetWithNotify(value, ref _roomLayoutConnectivityRepairFailed);
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

    public override void SyncEntries()
    {
        base.SyncEntries();
        Reconcile(_fragmentRoomFailures, _fragmentRoomFailuresEntries);
        Reconcile(_fragmentInstancesByVariant, _fragmentInstancesByVariantEntries);
    }

    private protected override void EnsureDictionariesInitialized()
    {
        if (_dictionariesInitialized)
        {
            return;
        }
        base.EnsureDictionariesInitialized();
        LoadFromEntries(_fragmentRoomFailures, _fragmentRoomFailuresEntries);
        LoadFromEntries(_fragmentInstancesByVariant, _fragmentInstancesByVariantEntries);
    }
}
