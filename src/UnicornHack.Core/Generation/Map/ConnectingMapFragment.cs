using CSharpScriptSerialization;
using UnicornHack.Data.Fragments;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataLoading;

namespace UnicornHack.Generation.Map;

public class ConnectingMapFragment : MapFragment
{
    public ICollection<LevelConnection> Connections
    {
        get;
        set;
    } = new HashSet<LevelConnection>();

    private Func<string, int, int, int, ConnectionComponent?, float>? _weightFunction;

    private static readonly UnicornExpressionVisitor _translator =
        new([BranchParameter, DepthParameter, InstancesParameter, TagInstancesParameter, ConnectionParameter]);

    protected override void ResetWeightFunction() => _weightFunction = null;

    public static Func<string, int, int, int, ConnectionComponent?, float> CreateWeightFunction(string expression)
        => _translator.Translate<Func<string, int, int, int, ConnectionComponent?, float>, float>(expression);

    public float GetWeight(RoomComponent room, Rectangle boundingRectangle, ConnectionComponent? target)
    {
        // TODO: take transformations into account
        if (PayloadArea.Width > boundingRectangle.Width || PayloadArea.Height > boundingRectangle.Height)
        {
            return 0;
        }

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
            return _weightFunction(room.Level.Branch.Name, room.Level.Depth, 0, 0, target);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Error while evaluating the Weight for " + Name, e);
        }
    }

    public override Chunk? BuildChunk(RoomComponent room, IEnumerable<Point> points, Action<Point> insideAction,
        Action<Point> perimeterAction, Action<Point> outsideAction, int minDoorways = 0)
    {
        var chunk = base.BuildChunk(room, points, insideAction, perimeterAction, outsideAction, minDoorways);
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
    {
        foreach (var connectionEntity in room.IncomingConnections)
        {
            var connection = connectionEntity.Connection!;
            if (connection.TargetLevelX == null)
            {
                var target = connection.TargetRoomEntity.Room!;
                if ((connectionDefinition?.BranchName == null
                     || connectionDefinition.BranchName == target.Level.BranchName)
                    && (connectionDefinition?.Depth == null
                        || connectionDefinition.Depth == target.Level.Depth)
                    && LevelConnection.AreMatchingDirectionality(connection.Directionality, connectionDefinition?.Direction))
                {
                    LevelConnection.CreateReceivingConnection(room, point, connection,
                        connectionDefinition?.Direction);
                    return;
                }
            }
        }

        LevelConnection.CreateSourceConnection(room, point, connectionDefinition);
    }

    public static readonly CSScriptLoader<ConnectingMapFragment> Loader =
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
