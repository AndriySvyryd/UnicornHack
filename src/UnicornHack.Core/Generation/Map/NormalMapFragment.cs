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

    public float GetWeight(RoomComponent room, in LotPlacementContext ctx)
    {
        // Cheapest gates first. The doorway-capability check is two field reads
        // against an integer demand; the demand multiplier is a tag/feature scan with
        // no geometric queries. Only when both pass do we run the expensive edge-match
        // anchor sweep that drives the geometric partition / interval-tree queries.
        if (PossibleDoorwayCount < ctx.MinDoorways && !HasOpenPerimeter)
        {
            return 0;
        }

        var demandMultiplier = GetDemandMultiplier(room, ctx);
        if (demandMultiplier == 0f)
        {
            return 0;
        }

        var edgeMultiplier = GetEdgeMatchMultiplier(room, ctx);
        if (edgeMultiplier == 0f)
        {
            return 0;
        }

        var stats = room.Level.GenerationStats;
        var gameStats = room.Level.Game.GenerationStats;
        return GetWeight(
                room.Level.Branch.Name,
                room.Level.Depth,
                stats.FragmentInstancesByName,
                gameStats.FragmentInstancesByName,
                stats.FragmentInstancesByTag,
                gameStats.FragmentInstancesByTag,
                gameStats.FragmentInstancesByFeature)
            * edgeMultiplier * demandMultiplier;
    }

    /// <summary>
    ///     Evaluates the raw <see cref="MapFragment.GenerationWeight" /> DSL for this
    ///     fragment against the supplied stats, lazy-parsing on first use and
    ///     multiplying by <see cref="MapFragment.WeightMultiplier" />. No room/placement
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
        if (_weightFunction == null)
        {
            _weightFunction = CompileWeightFunction<
                Func<string, int, string,
                    IReadOnlyDictionary<string, int>, IReadOnlyDictionary<string, int>,
                    IReadOnlyDictionary<string, int>, IReadOnlyDictionary<string, int>,
                    IReadOnlyDictionary<RoomFeatures, int>, float>>(_translator);
        }

        try
        {
            return _weightFunction(
                    branchName,
                    depth,
                    SourceName,
                    fragmentInstancesByName,
                    globalFragmentInstancesByName,
                    tagInstances,
                    globalTagInstances,
                    globalFeatureInstances)
                * WeightMultiplier;
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
