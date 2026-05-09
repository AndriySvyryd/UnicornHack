using CSharpScriptSerialization;
using UnicornHack.Data.Fragments;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataLoading;

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
    public RoomParameters GetRoomParameters(Branch branch)
        => new(
            GetEffectiveRoomWidth(branch),
            GetEffectiveRoomHeight(branch),
            GetEffectiveDefaultTerrain(branch),
            GetEffectiveCreatureGenerator(branch),
            GetEffectiveItemGenerator(branch));

    private Func<string, int, RoomFeatures, int, int, ConnectionComponent?, float>? _weightFunction;

    private static readonly UnicornExpressionVisitor _translator =
        new(
        [
            BranchParameter,
            DepthParameter,
            RoomFeaturesParameter,
            InstancesParameter,
            TagInstancesParameter,
            ConnectionParameter
        ]);

    protected override void ResetWeightFunction() => _weightFunction = null;

    public static new Func<string, int, RoomFeatures, int, int, ConnectionComponent?, float> CreateWeightFunction(
        string expression)
        => _translator.Translate<Func<string, int, RoomFeatures, int, int, ConnectionComponent?, float>, float>(expression);

    /// <summary>
    ///     Returns this fragment's selection weight for a level- or room-scoped pick. The
    ///     <paramref name="roomFeatures" /> argument is <see cref="RoomFeatures.Normal" /> for
    ///     level-scoped picks (the level-defining fragment) and the room's flags for per-room
    ///     picks. Existing weight expressions that don't reference <c>$roomFeatures</c> are
    ///     unaffected.
    /// </summary>
    public float GetWeight(string branchName, byte depth, RoomFeatures roomFeatures, int instances, int tagInstances)
    {
        if (_weightFunction == null)
        {
            try
            {
                _weightFunction = CreateWeightFunction(GenerationWeight ?? DefaultWeight);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error while parsing the GenerationWeight for " + Name, e);
            }
        }

        try
        {
            return _weightFunction(branchName, depth, roomFeatures, instances, tagInstances, null);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Error while evaluating the Weight for " + Name, e);
        }
    }

    /// <summary>
    ///     Backward-compatible overload; delegates to the room-aware overload with
    ///     <see cref="RoomFeatures.Normal" />.
    /// </summary>
    public float GetWeight(string branchName, byte depth, int instances, int tagInstances)
        => GetWeight(branchName, depth, RoomFeatures.Normal, instances, tagInstances);

    public static new readonly CSScriptLoader<DefiningMapFragment> Loader =
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
