using CSharpScriptSerialization;
using UnicornHack.Data.Fragments;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

public class ConnectingMapFragment : MapFragment
{
    public ICollection<LevelConnection> Connections
    {
        get;
        set;
    } = new HashSet<LevelConnection>();

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
        ConnectionComponent?,                    // connection
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
            GlobalFeatureInstancesParameter,
            ConnectionParameter);

    protected override void ResetWeightFunction() => _weightFunction = null;

    public float GetWeight(RoomComponent room, in LotPlacementContext ctx, ConnectionComponent? target)
    {
        var edgeMultiplier = GetEdgeMatchMultiplier(room, ctx);
        if (edgeMultiplier == 0f)
        {
            return 0;
        }

        if (PossibleDoorwayCount < ctx.MinDoorways && !HasOpenPerimeter)
        {
            return 0;
        }

        if (_weightFunction == null)
        {
            _weightFunction = CompileWeightFunction<
                Func<string, int, string,
                    IReadOnlyDictionary<string, int>, IReadOnlyDictionary<string, int>,
                    IReadOnlyDictionary<string, int>, IReadOnlyDictionary<string, int>,
                    IReadOnlyDictionary<RoomFeatures, int>, ConnectionComponent?, float>>(_translator);
        }

        var stats = room.Level.GenerationStats;
        var gameStats = room.Level.Game.GenerationStats;
        try
        {
            return _weightFunction(
                    room.Level.Branch.Name, room.Level.Depth, SourceName,
                    stats.FragmentInstancesByName, gameStats.FragmentInstancesByName,
                    stats.FragmentInstancesByTag, gameStats.FragmentInstancesByTag,
                    gameStats.FragmentInstancesByFeature, target)
                * edgeMultiplier * GetDemandMultiplier(room, ctx) * WeightMultiplier;
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Error while evaluating the Weight for " + Name, e);
        }
    }

    public override Chunk? BuildChunk(RoomComponent room, IEnumerable<Point> points,
        int minDoorways, RoomParameters roomParameters)
    {
        var chunk = base.BuildChunk(room, points, minDoorways, roomParameters);
        if (chunk == null
            || chunk.InsidePoints.Count == 0)
        {
            return chunk;
        }

        var connectionDefinitions = Connections;
        if (connectionDefinitions.Count == 0)
        {
            connectionDefinitions = [new LevelConnection()];
        }

        foreach (var levelConnection in connectionDefinitions.Where(c => c.Glyph == null))
        {
            var connectionPoint = room.Level.GenerationRandom.Pick(chunk.BoundingRectangle,
                p => chunk.InsidePoints.Contains(p) && !room.Connections.ContainsKey(p));
            CreateConnection(room, connectionPoint, levelConnection);
        }

        return chunk;
    }

    protected override void CreateConnection(RoomComponent room, Point point, char? glyph)
        => CreateConnection(room, point, Connections.FirstOrDefault(c => c.Glyph == glyph));

    protected virtual void CreateConnection(
        RoomComponent room, Point point, LevelConnection? connectionDefinition)
        => LevelConnection.CreateLevelConnection(room, point, connectionDefinition);

    public static readonly MapFragmentLoader<ConnectingMapFragment> Loader =
        new(@"Data\Fragments\Connecting\", typeof(ConnectingMapFragmentData));

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<ConnectingMapFragment>(GetPropertyConditions<ConnectingMapFragment>());

    protected static new Dictionary<string, Func<TConnectingMapFragment, object?, bool>>
        GetPropertyConditions<TConnectingMapFragment>() where TConnectingMapFragment : ConnectingMapFragment
    {
        var propertyConditions = MapFragment.GetPropertyConditions<TConnectingMapFragment>();
        var mapCondition = propertyConditions[nameof(Map)];
        propertyConditions.Remove(nameof(Map));

        propertyConditions.Add(nameof(Connections),
            (o, v) => v != null && ((ICollection<LevelConnection>)v).Count != 0);
        propertyConditions.Add(nameof(Map), mapCondition);
        return propertyConditions;
    }

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
