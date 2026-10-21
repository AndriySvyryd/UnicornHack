using CSharpScriptSerialization;
using UnicornHack.Data.Fragments;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

public class NormalMapFragment : MapFragment
{
    // Lambda compiled from the fragment's GenerationWeight DSL. Signature mirrors the
    // _translator parameter list below. SourceName + the two per-name dicts back the
    // visitor-emitted `$instances` / `$globalInstances` lookups — the lookup IL is
    // baked in only when the expression actually references the variable, so when it
    // doesn't, the lambda performs zero dict reads on the hot path.
    private Func<
        string,                                  // branch
        int,                                     // depth
        string,                                  // sourceName
        IReadOnlyDictionary<string, int>,        // fragmentInstancesByName
        IReadOnlyDictionary<string, int>,        // globalFragmentInstancesByName
        IReadOnlyDictionary<string, int>,        // tagInstances
        IReadOnlyDictionary<string, int>,        // globalTagInstances
        IReadOnlyDictionary<RoomFeatures, int>,  // globalFeatureInstances
        float>? _weightFunction;

    private static readonly UnicornExpressionVisitor _translator =
        CreateInstanceAwareTranslator(
            BranchParameter,
            DepthParameter,
            SourceNameParameter,
            FragmentInstancesByNameParameter,
            GlobalFragmentInstancesByNameParameter,
            TagInstancesParameter,
            GlobalTagInstancesParameter,
            GlobalFeatureInstancesParameter);

    protected override void ResetWeightFunction() => _weightFunction = null;

    public override void EnsureWeightFunctionCompiled()
        => _weightFunction ??= CompileWeightFunction<
            Func<string, int, string,
                IReadOnlyDictionary<string, int>, IReadOnlyDictionary<string, int>,
                IReadOnlyDictionary<string, int>, IReadOnlyDictionary<string, int>,
                IReadOnlyDictionary<RoomFeatures, int>, float>>(_translator);

    public float GetWeight(RoomComponent room, in LotPlacementContext ctx)
    {
        var placementMultiplier = GetPlacementMultiplier(room, ctx);
        if (placementMultiplier == 0f)
        {
            return 0;
        }

        var level = room.Level;
        var stats = level.GenerationStats;
        var gameStats = level.Game.GenerationStats;
        var baseWeight = GetWeight(
            level.Branch.Name,
            level.Depth,
            stats.FragmentInstancesByName,
            gameStats.FragmentInstancesByName,
            stats.FragmentInstancesByTag,
            gameStats.FragmentInstancesByTag,
            gameStats.FragmentInstancesByFeature);

        return GetPlacementWeight(baseWeight, placementMultiplier);
    }

    /// <summary>
    ///     Evaluates the raw <see cref="MapFragment.GenerationWeight" /> DSL for this
    ///     fragment against the supplied stats, lazy-parsing on first use. No size bonus,
    ///     variant scaling, or room/placement
    ///     gating is applied — placement code should prefer the room/context overload
    ///     so the cheap geometric gates run before the DSL evaluation. Used by
    ///     diagnostic probes that sample raw weights across many fragments without
    ///     constructing real placement contexts.
    /// </summary>
    public float GetWeight(
        string branchName,
        int depth,
        IReadOnlyDictionary<string, int> fragmentInstancesByName,
        IReadOnlyDictionary<string, int> globalFragmentInstancesByName,
        IReadOnlyDictionary<string, int> tagInstances,
        IReadOnlyDictionary<string, int> globalTagInstances,
        IReadOnlyDictionary<RoomFeatures, int> globalFeatureInstances)
    {
        EnsureWeightFunctionCompiled();

        try
        {
            return _weightFunction!(
                    branchName,
                    depth,
                    SourceName,
                    fragmentInstancesByName,
                    globalFragmentInstancesByName,
                    tagInstances,
                    globalTagInstances,
                    globalFeatureInstances);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Error while evaluating the Weight for " + Name, e);
        }
    }

    public static readonly MapFragmentLoader<NormalMapFragment> Loader =
        new(@"Data\Fragments\Normal\", typeof(NormalMapFragmentData));

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<NormalMapFragment>(GetPropertyConditions<NormalMapFragment>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
