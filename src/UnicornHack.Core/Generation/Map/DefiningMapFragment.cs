using CSharpScriptSerialization;
using UnicornHack.Data.Fragments;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

/// <summary>
///     Per-room data for level generation. All overrides default to <c>null</c> and inherit
///     from the level's <see cref="Branch" /> when null.
/// </summary>
public class DefiningMapFragment : ConnectingMapFragment
{
    /// <summary>
    ///     Per-fragment override of <see cref="Branch.RoomHeight" />. Null = inherit from the
    ///     branch.
    /// </summary>
    public byte? RoomHeight
    {
        get;
        set;
    }

    /// <summary>
    ///     Per-fragment override of <see cref="Branch.RoomWidth" />. Null = inherit from the
    ///     branch.
    /// </summary>
    public byte? RoomWidth
    {
        get;
        set;
    }

    /// <summary>
    ///     Per-fragment override of <see cref="Branch.CreatureGenerator" />. Null = inherit from
    ///     the branch.
    /// </summary>
    public CreatureGenerator? CreatureGenerator
    {
        get;
        set;
    }

    /// <summary>
    ///     Per-fragment override of <see cref="Branch.ItemGenerator" />. Null = inherit from the
    ///     branch.
    /// </summary>
    public ItemGenerator? ItemGenerator
    {
        get;
        set;
    }

    /// <summary>
    ///     Per-fragment override of <see cref="Branch.DefaultTerrain" />. Null = inherit from
    ///     the branch.
    /// </summary>
    public MapFeature? DefaultTerrain
    {
        get;
        set;
    }

    /// <summary>
    ///     Per-fragment override of <see cref="Branch.DefaultPathTerrain" />. Null = inherit
    ///     from the branch.
    /// </summary>
    public MapFeature? DefaultPathTerrain
    {
        get;
        set;
    }

    /// <summary>
    ///     Returns the effective <see cref="Map.RoomWidth" /> for this fragment in the supplied
    ///     branch — the per-fragment override if set, otherwise the branch default.
    /// </summary>
    public byte GetEffectiveRoomWidth(Branch branch) => RoomWidth ?? branch.RoomWidth;

    /// <summary>
    ///     Returns the effective <see cref="Map.RoomHeight" /> for this fragment in the supplied
    ///     branch — the per-fragment override if set, otherwise the branch default.
    /// </summary>
    public byte GetEffectiveRoomHeight(Branch branch) => RoomHeight ?? branch.RoomHeight;

    /// <summary>
    ///     Returns the effective <see cref="MapFeature" /> for default terrain — fragment
    ///     override if set, otherwise the branch default.
    /// </summary>
    public MapFeature GetEffectiveDefaultTerrain(Branch branch) => DefaultTerrain ?? branch.DefaultTerrain;

    /// <summary>
    ///     Returns the effective <see cref="CreatureGenerator" /> — fragment override if set,
    ///     otherwise the branch default.
    /// </summary>
    public CreatureGenerator GetEffectiveCreatureGenerator(Branch branch)
        => CreatureGenerator ?? branch.CreatureGenerator;

    /// <summary>
    ///     Returns the effective <see cref="ItemGenerator" /> — fragment override if set,
    ///     otherwise the branch default.
    /// </summary>
    public ItemGenerator GetEffectiveItemGenerator(Branch branch)
        => ItemGenerator ?? branch.ItemGenerator;

    /// <summary>
    ///     Returns a single snapshot of all per-room values that callers typically need
    ///     together (width, height, default terrain, creature/item generators) so they don't
    ///     have to invoke five <c>GetEffective*</c> methods individually.
    /// </summary>
    /// <remarks>
    ///     Throws when the effective <see cref="CreatureGenerator" /> or
    ///     <see cref="ItemGenerator" /> ends up null — e.g. when neither this fragment
    ///     nor the branch supplies one. <see cref="RoomParameters" />'s constructor also
    ///     rejects zero <see cref="RoomWidth" /> / <see cref="RoomHeight" />.
    /// </remarks>
    public RoomParameters GetRoomParameters(Branch branch)
    {
        var creatures = GetEffectiveCreatureGenerator(branch)
            ?? throw new InvalidOperationException(
                $"'{Name}': no CreatureGenerator set on the fragment or branch '{branch.Name}'.");
        var items = GetEffectiveItemGenerator(branch)
            ?? throw new InvalidOperationException(
                $"'{Name}': no ItemGenerator set on the fragment or branch '{branch.Name}'.");
        return new RoomParameters(
            GetEffectiveRoomWidth(branch),
            GetEffectiveRoomHeight(branch),
            GetEffectiveDefaultTerrain(branch),
            creatures,
            items);
    }

    // Lambda compiled from the fragment's GenerationWeight DSL. Signature mirrors the
    // _translator parameter list below. SourceName + the two per-name dicts back the
    // visitor-emitted `$instances` / `$globalInstances` lookups — the lookup IL is
    // baked in only when the expression actually references the variable, so when it
    // doesn't, the lambda performs zero dict reads on the hot path.
    private Func<
        string,                                  // branch
        int,                                     // depth
        RoomFeatures,                            // roomFeatures
        string,                                  // sourceName
        IReadOnlyDictionary<string, int>,        // fragmentInstancesByName
        IReadOnlyDictionary<string, int>,        // globalFragmentInstancesByName
        IReadOnlyDictionary<string, int>,        // tagInstances
        IReadOnlyDictionary<string, int>,        // globalTagInstances
        IReadOnlyDictionary<RoomFeatures, int>,  // globalFeatureInstances
        ConnectionComponent?,                    // connection
        float>? _weightFunction;

    private static readonly UnicornExpressionVisitor _translator =
        CreateInstanceAwareTranslator(
            BranchParameter,
            DepthParameter,
            RoomFeaturesParameter,
            SourceNameParameter,
            FragmentInstancesByNameParameter,
            GlobalFragmentInstancesByNameParameter,
            TagInstancesParameter,
            GlobalTagInstancesParameter,
            GlobalFeatureInstancesParameter,
            ConnectionParameter);

    protected override void ResetWeightFunction() => _weightFunction = null;

    /// <summary>
    ///     Returns this fragment's selection weight for a level- or room-scoped pick. The
    ///     <paramref name="roomFeatures" /> argument is <see cref="RoomFeatures.Normal" /> for
    ///     level-scoped picks (the level-defining fragment) and the room's flags for per-room
    ///     picks. Existing weight expressions that don't reference <c>$roomFeatures</c> are
    ///     unaffected.
    /// </summary>
    public float GetWeight(
        string branchName,
        byte depth,
        RoomFeatures roomFeatures,
        IReadOnlyDictionary<string, int> fragmentInstancesByName,
        IReadOnlyDictionary<string, int> globalFragmentInstancesByName,
        IReadOnlyDictionary<string, int> tagInstances,
        IReadOnlyDictionary<string, int> globalTagInstances,
        IReadOnlyDictionary<RoomFeatures, int> globalFeatureInstances)
    {
        if (_weightFunction == null)
        {
            _weightFunction = CompileWeightFunction<
                Func<string, int, RoomFeatures, string,
                    IReadOnlyDictionary<string, int>, IReadOnlyDictionary<string, int>,
                    IReadOnlyDictionary<string, int>, IReadOnlyDictionary<string, int>,
                    IReadOnlyDictionary<RoomFeatures, int>, ConnectionComponent?, float>>(_translator);
        }

        try
        {
            return _weightFunction(
                    branchName, depth, roomFeatures, SourceName,
                    fragmentInstancesByName, globalFragmentInstancesByName,
                    tagInstances, globalTagInstances, globalFeatureInstances, null)
                * WeightMultiplier;
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Error while evaluating the Weight for " + Name, e);
        }
    }

    /// <summary>
    ///     Backward-compatible overload for tests that pass the scalar per-fragment
    ///     instance counts directly. Wraps them into single-entry dicts keyed by
    ///     <see cref="MapFragment.SourceName" /> so the visitor's <c>$instances</c> /
    ///     <c>$globalInstances</c> lookups resolve to the supplied values.
    /// </summary>
    public float GetWeight(
        string branchName, byte depth, RoomFeatures roomFeatures,
        int instances, IReadOnlyDictionary<string, int> tagInstances,
        int globalInstances, IReadOnlyDictionary<string, int> globalTagInstances,
        IReadOnlyDictionary<RoomFeatures, int> globalFeatureInstances)
    {
        var key = SourceName;
        return GetWeight(
            branchName, depth, roomFeatures,
            new Dictionary<string, int>(StringComparer.Ordinal) { [key] = instances },
            new Dictionary<string, int>(StringComparer.Ordinal) { [key] = globalInstances },
            tagInstances, globalTagInstances, globalFeatureInstances);
    }

    /// <summary>
    ///     Backward-compatible overload; treats per-level and game-wide instance counts as
    ///     equivalent for callers that don't track the game-wide totals explicitly.
    /// </summary>
    public float GetWeight(
        string branchName, byte depth, RoomFeatures roomFeatures, int instances, IReadOnlyDictionary<string, int> tagInstances)
        => GetWeight(branchName, depth, roomFeatures, instances, tagInstances, instances, tagInstances,
            EmptyFeatureInstances);

    private static readonly IReadOnlyDictionary<RoomFeatures, int> EmptyFeatureInstances =
        new Dictionary<RoomFeatures, int>();

    /// <summary>
    ///     Backward-compatible overload; delegates to the room-aware overload with
    ///     <see cref="RoomFeatures.Normal" />.
    /// </summary>
    public float GetWeight(string branchName, byte depth, int instances, IReadOnlyDictionary<string, int> tagInstances)
        => GetWeight(branchName, depth, RoomFeatures.Normal, instances, tagInstances);

    public static new readonly MapFragmentLoader<DefiningMapFragment> Loader =
        new(@"Data\Fragments\Defining\", typeof(DefiningMapFragmentData));

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<DefiningMapFragment>(GetPropertyConditions<DefiningMapFragment>());

    protected static new Dictionary<string, Func<TDefiningMapFragment, object?, bool>>
        GetPropertyConditions<TDefiningMapFragment>() where TDefiningMapFragment : DefiningMapFragment
    {
        var propertyConditions = ConnectingMapFragment.GetPropertyConditions<TDefiningMapFragment>();
        var mapCondition = propertyConditions[nameof(Map)];
        propertyConditions.Remove(nameof(Map));

        propertyConditions.Add(nameof(NoRandomDoorways), (_, v) => (bool)v!);
        propertyConditions.Add(nameof(RoomHeight), (_, v) => v != null);
        propertyConditions.Add(nameof(RoomWidth), (_, v) => v != null);
        propertyConditions.Add(nameof(CreatureGenerator), (_, v) => v != null);
        propertyConditions.Add(nameof(ItemGenerator), (_, v) => v != null);
        propertyConditions.Add(nameof(DefaultTerrain), (_, v) => v != null);
        propertyConditions.Add(nameof(DefaultPathTerrain), (_, v) => v != null);
        propertyConditions.Add(nameof(Map), mapCondition);
        return propertyConditions;
    }

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
