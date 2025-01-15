using System.Linq.Expressions;
using CSharpScriptSerialization;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataLoading;

namespace UnicornHack.Generation.Map;

public abstract class MapFragment : ICSScriptSerializable, ILoadable
{
    private string? _generationWeight;

    public string Name
    {
        get;
        set;
    } = null!;

    public string Map
    {
        get;
        set;
    } = "";

    public DynamicMap? DynamicMap
    {
        get;
        set;
    }

    public byte[]? ByteMap
    {
        get;
        set;
    }

    public byte Width
    {
        get;
        set;
    }

    public byte Height
    {
        get;
        set;
    }

    public Rectangle PayloadArea
    {
        get;
        set;
    }

    public string? GenerationWeight
    {
        get => _generationWeight;
        set
        {
            ResetWeightFunction();
            _generationWeight = value;
        }
    }

    public ICollection<string>? Tags
    {
        get;
        set;
    }

    public bool NoRandomDoorways
    {
        get;
        set;
    }

    // TODO: properties: NoMirror (for reflection symmetry, only 3 turns),
    // NoTurnOver(for rotational symmetry, only mirror and half-turn),
    // NoTransform(implies previous two), NoOverwrite

    public bool ConditionalFirstRow
    {
        get;
        set;
    }

    public bool ConditionalLastRow
    {
        get;
        set;
    }

    public bool ConditionalFirstColumn
    {
        get;
        set;
    }

    public bool ConditionalLastColumn
    {
        get;
        set;
    }

    public short[,]? PointToIndex
    {
        get;
        private set;
    }

    public Point[]? IndexToPoint
    {
        get;
        private set;
    }

    protected static readonly string DefaultWeight = "1";

    protected static readonly ParameterExpression BranchParameter =
        Expression.Parameter(typeof(string), name: "branch");

    protected static readonly ParameterExpression DepthParameter =
        Expression.Parameter(typeof(int), name: "depth");

    protected static readonly ParameterExpression InstancesParameter =
        Expression.Parameter(typeof(int), name: "instances");

    protected static readonly ParameterExpression TagInstancesParameter =
        Expression.Parameter(typeof(int), name: "tagInstances");

    protected static readonly ParameterExpression ConnectionParameter =
        Expression.Parameter(typeof(ConnectionComponent), name: "connection");

    protected abstract void ResetWeightFunction();

    // TODO: Characters that can be used as conditions for neighbors:
    // ~ - marks the edge row and/or column as conditional
    // X - should be outside the level
    [MemberNotNull(nameof(ByteMap))]
    [MemberNotNull(nameof(PointToIndex))]
    [MemberNotNull(nameof(IndexToPoint))]
    public void EnsureInitialized(Game game)
    {
        if (ByteMap == null)
        {
            byte x = 0, y = 0;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Map.Length; i++)
            {
                switch (Map[i])
                {
                    case '\r':
                        continue;
                    case '\n':
                        if (x == 0 && y == 0)
                        {
                            continue;
                        }

                        if (x > Width)
                        {
                            Width = x;
                        }

                        x = 0;
                        y++;
                        continue;
                    default:
                        x++;
                        break;
                }
            }

            if (x > 0)
            {
                if (x > Width)
                {
                    Width = x;
                }

                y++;
            }

            if (y > Height)
            {
                Height = y;
            }

            (PointToIndex, IndexToPoint) = Rectangle.GetPointIndex(game.Services!.SharedCache, Width, Height);

            x = 0;
            y = 0;
            ByteMap = new byte[Width * Height];
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Map.Length; i++)
            {
                var character = Map[i];
                switch (character)
                {
                    case '\r':
                        continue;
                    case '\n':
                        if (x == 0 && y == 0)
                        {
                            continue;
                        }

                        while (x != Width)
                        {
                            ByteMap[PointToIndex[x, y]] = (byte)' ';
                            x++;
                        }

                        x = 0;
                        y++;
                        continue;
                    case '~':
                        if (x == 0)
                        {
                            ConditionalFirstColumn = true;
                        }
                        else if (x == Width - 1)
                        {
                            ConditionalLastColumn = true;
                        }
                        else if (y != 0 && y != Height - 1)
                        {
                            throw new InvalidOperationException(
                                $"{Name}: '~' can only be either the first or the last character in a row if not in the first or last row.");
                        }

                        if (y == 0)
                        {
                            ConditionalFirstRow = true;
                        }
                        else if (y == Height - 1)
                        {
                            ConditionalLastRow = true;
                        }

                        ByteMap[PointToIndex[x, y]] = (byte)character;
                        x++;
                        break;
                    default:
                        ByteMap[PointToIndex[x, y]] = (byte)character;
                        x++;
                        break;
                }
            }

            if (Width == 0)
            {
                return;
            }

            if (x > 0)
            {
                while (x != Width)
                {
                    ByteMap[x + Width * y] = (byte)' ';
                    x++;
                }
            }

            var payloadOrigin = new Point((byte)(ConditionalFirstColumn ? 1 : 0),
                (byte)(ConditionalFirstRow ? 1 : 0));
            PayloadArea = new Rectangle(payloadOrigin,
                (byte)(Width - payloadOrigin.X - (ConditionalLastColumn ? 1 : 0)),
                (byte)(Height - payloadOrigin.Y - (ConditionalLastRow ? 1 : 0)));
        }

        if (PointToIndex == null
            || IndexToPoint == null)
        {
            (PointToIndex, IndexToPoint) = Rectangle.GetPointIndex(game.Services!.SharedCache, Width, Height);
        }
    }

    public Room? TryPlace(LevelComponent level, Rectangle boundingRectangle)
    {
        try
        {
            EnsureInitialized(level.Game);

            return ByteMap.Length != 0
                ? TryPlaceStaticMap(level, boundingRectangle)
                : TryPlace(level, boundingRectangle, DynamicMap);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error while placing fragment " + Name, ex);
        }
    }

    protected virtual Room TryPlaceStaticMap(LevelComponent level, Rectangle boundingRectangle)
    {
        // TODO: take transformations into account
        var target = boundingRectangle.PlaceInside(PayloadArea, level.GenerationRandom);
        if (!target.HasValue)
        {
            throw new InvalidOperationException($"Couldn't fit fragment {Name} into {boundingRectangle}");
        }

        var doorwayPoints = new List<Point>();
        var perimeterPoints = new List<Point>();
        var insidePoints = new List<Point>();
        var points = new List<Point>();
        var predefinedPoints = new List<(Point, char)>();
        WriteMap(target.Value, level, Write,
            (doorwayPoints, perimeterPoints, insidePoints, points, predefinedPoints));

        if (!NoRandomDoorways && doorwayPoints.Count == 0)
        {
            // TODO: find doorway candidates in the perimeter walls
        }

        return new Room(level,
            new Rectangle(target.Value, PayloadArea.Width, PayloadArea.Height),
            doorwayPoints,
            insidePoints,
            predefinedPoints,
            this);
    }

    protected void Write(
        char c,
        Point point,
        LevelComponent level,
        (List<Point> doorwayPoints,
            List<Point> perimeterPoints,
            List<Point> insidePoints,
            List<Point> points,
            List<(Point, char)> predefinedPoints) state)
    {
        var (doorwayPoints, _, insidePoints, points, predefinedPoints) = state;
        var feature = MapFeature.Default;
        switch (c)
        {
            case '.':
                feature = MapFeature.StoneFloor;
                insidePoints.Add(point);
                goto case '\u0001';
            case ',':
                feature = MapFeature.RockFloor;
                insidePoints.Add(point);
                goto case '\u0001';
            case '?':
                feature = MapFeature.StoneFloor;
                doorwayPoints.Add(point);
                goto case '\u0001';
            case '#':
                feature = MapFeature.StoneWall;
                goto case '\u0001';
            case 'A':
                feature = MapFeature.StoneArchway;
                goto case '\u0001';
            case '=':
                feature = MapFeature.Pool;
                goto case '\u0001';
            case '\u0001':
                points.Add(point);
                break;
            case '<':
            case '{':
            case '[':
            case '>':
            case '}':
            case ']':
                feature = MapFeature.StoneFloor;
                points.Add(point);
                CreateConnection(level, point, c);
                break;
            case ' ':
                break;
            default:
                feature = MapFeature.StoneFloor;
                insidePoints.Add(point);
                predefinedPoints.Add((point, c));
                break;
        }

        TerrainSystem.SetTerrain(feature, point, level);
    }

    protected virtual void CreateConnection(LevelComponent level, Point point, char? glyph)
    {
    }

    protected Room? TryPlace(LevelComponent level, Rectangle boundingRectangle, DynamicMap? map)
    {
        if (map == null || !boundingRectangle.Contains(map.MinSize))
        {
            return null;
        }

        // TODO: Read the defaults from the defining fragment
        return BuildRoom(level, map.GetRoomPoints(level, boundingRectangle),
            p => TerrainSystem.SetTerrain(MapFeature.StoneFloor, p, level),
            p => TerrainSystem.SetTerrain(MapFeature.StoneWall, p, level),
            _ => { });
    }

    public void WriteMap<TState>(Point target, LevelComponent level,
        Action<char, Point, LevelComponent, TState> write, TState state)
    {
        EnsureInitialized(level.Game);

        var map = ByteMap;
        var x = target.X;
        var y = target.Y;
        var lastFragmentY = 0;
        foreach (var fragmentPoint in PayloadArea)
        {
            var mapPoint = (char)map[PointToIndex[fragmentPoint.X, fragmentPoint.Y]];

            if (lastFragmentY != fragmentPoint.Y)
            {
                lastFragmentY = fragmentPoint.Y;
                x = target.X;
                y++;
            }

            write(mapPoint, new Point(x, y), level, state);
            x++;
        }
    }

    public virtual Room? BuildRoom(LevelComponent level, IEnumerable<Point> points, Action<Point> insideAction,
        Action<Point> perimeterAction, Action<Point> outsideAction)
    {
        var insidePoints = new List<Point>();
        var neighbors = new byte[level.TileCount];
        var firstPoint = new Point();
        var lastPoint = new Point();
        var left = byte.MaxValue;
        var right = byte.MinValue;
        var occupiedPointCount = 0;
        foreach (var roomPoint in points)
        {
            if (occupiedPointCount == 0)
            {
                firstPoint = roomPoint;
                right = firstPoint.X;
                left = firstPoint.X;
            }
            else if (lastPoint.Y != roomPoint.Y)
            {
                right = Math.Max(right, lastPoint.X);
                left = Math.Min(left, roomPoint.X);
            }

            lastPoint = roomPoint;

            for (var directionIndex = 0; directionIndex < 8; directionIndex++)
            {
                var direction = Vector.MovementDirections[directionIndex];
                var newLocationX = (byte)(roomPoint.X + direction.X);
                var newLocationY = (byte)(roomPoint.Y + direction.Y);

                if (newLocationX >= level.Width || newLocationY >= level.Height)
                {
                    continue;
                }

                var newLocationIndex = level.PointToIndex![newLocationX, newLocationY];
                var newNeighbors = (byte)(neighbors[newLocationIndex] |
                                           1 << Vector.OppositeDirectionIndexes[directionIndex]);
                if (newNeighbors == (byte)DirectionFlags.All &&
                    (neighbors[level.PointToIndex[roomPoint.X, roomPoint.Y]] & 1 << directionIndex) != 0)
                {
                    var insidePoint = new Point(newLocationX, newLocationY);
                    insideAction.Invoke(insidePoint);
                    insidePoints.Add(insidePoint);
                }

                neighbors[newLocationIndex] = newNeighbors;
            }

            occupiedPointCount++;
        }

        if (occupiedPointCount == 0)
        {
            return null;
        }

        var perimeter = WalkPerimeter(firstPoint, neighbors, level.PointToIndex!, perimeterAction, outsideAction);

        var doorwayPoints = perimeter.Count > 7 ? FindDoorwayPoints(perimeter, level) : null;

        return new Room(level,
            new Rectangle(new Point(left, firstPoint.Y), new Point(right, lastPoint.Y)),
            doorwayPoints,
            insidePoints,
            new List<(Point, char)>(),
            this);
    }

    private static List<Point> FindDoorwayPoints(
        IReadOnlyList<Point> perimeter, LevelComponent level, bool allowCorners = false)
    {
        var doorwayPoints = new List<Point>();
        for (var i = 0; i < perimeter.Count; i++)
        {
            var point = perimeter[i];
            var nextPoint = perimeter[i == perimeter.Count - 1 ? 0 : i + 1];
            if (point.DistanceTo(nextPoint) > 1)
            {
                i++;
                continue;
            }

            var previousPoint = perimeter[i == 0 ? perimeter.Count - 1 : i - 1];
            if (point.DistanceTo(previousPoint) > 1)
            {
                continue;
            }

            var direction = previousPoint.DifferenceTo(nextPoint).GetUnit();
            if (!allowCorners && direction.X != 0 && direction.Y != 0)
            {
                continue;
            }

            var firstOrthogonal = direction.GetOrthogonal();
            var secondOrthogonal = firstOrthogonal.GetInverse();

            var corridorCandidatePoint = point.Translate(firstOrthogonal);
            if (!level.IsValid(corridorCandidatePoint) || !Layout.CanPlaceCorridor(corridorCandidatePoint, level))
            {
                continue;
            }

            var doorwayCandidatePoint = point.Translate(secondOrthogonal);
            if (!level.IsValid(doorwayCandidatePoint) || !Layout.CanPlaceCorridor(doorwayCandidatePoint, level))
            {
                continue;
            }

            doorwayPoints.Add(point);
        }

        return doorwayPoints;
    }

    private static IReadOnlyList<Point> WalkPerimeter(Point firstPoint, byte[] neighbors, short[,] pointToIndex,
        Action<Point> perimeterAction, Action<Point> outsideAction) =>
        WalkPerimeter(new Point(firstPoint.X, (byte)(firstPoint.Y - 1)), DirectionFlags.South, Direction.North,
            new Dictionary<Point, int>(), neighbors, pointToIndex, perimeterAction, outsideAction) ??
        new List<Point>();

    private static List<Point>? WalkPerimeter(Point firstPoint, DirectionFlags currentNeighborMap,
        Direction previousPointDirection, Dictionary<Point, int> visitedIntersections, byte[] neighbors,
        short[,] pointToIndex, Action<Point> perimeterAction, Action<Point> outsideAction)
    {
        // Assuming both the perimeter and the inner area are contiguous
        List<Point>? perimeter = null;
        var currentPoint = firstPoint;
        var previousPointDirectionIndex = (int)previousPointDirection;
        var perimeterLooped = false;

        for (var j = 0; j < neighbors.Length + 2; j++)
        {
            var perimeterPointFound = false;
            for (var i = 3; i > 0; i--)
            {
                var directionIndexToCheck = (previousPointDirectionIndex + i * 2) % 8;
                var neighborDirection = (DirectionFlags)(1 << directionIndexToCheck);
                if ((currentNeighborMap & neighborDirection) == neighborDirection)
                {
                    var directionToCheck = Vector.MovementDirections[directionIndexToCheck];
                    var newLocationX = (byte)(currentPoint.X + directionToCheck.X);
                    var newLocationY = (byte)(currentPoint.Y + directionToCheck.Y);
                    var nextPoint = new Point(newLocationX, newLocationY);
                    if (nextPoint.Equals(firstPoint))
                    {
                        break;
                    }

                    var nextNeighborMap = (DirectionFlags)neighbors[pointToIndex[newLocationX, newLocationY]];
                    var nextIsPerimeter = IsPerimeter(nextNeighborMap);
                    if (nextIsPerimeter)
                    {
                        if (perimeterLooped)
                        {
                            break;
                        }

                        perimeter ??= new List<Point>();

                        if (perimeter.Count == 0 || !nextPoint.Equals(perimeter[0]))
                        {
                            perimeter.Add(nextPoint);
                            perimeterAction.Invoke(nextPoint);
                        }
                        else
                        {
                            perimeterLooped = true;
                        }

                        currentPoint = nextPoint;
                        currentNeighborMap = nextNeighborMap;
                        previousPointDirectionIndex = Vector.OppositeDirectionIndexes[directionIndexToCheck];
                        perimeterPointFound = true;
                        break;
                    }

                    if (visitedIntersections.TryGetValue(nextPoint, out var _))
                    {
                        continue;
                    }

                    visitedIntersections[nextPoint] = directionIndexToCheck;

                    outsideAction.Invoke(nextPoint);

                    var subPerimeter = WalkPerimeter(nextPoint, nextNeighborMap,
                        (Direction)Vector.OppositeDirectionIndexes[directionIndexToCheck], visitedIntersections,
                        neighbors, pointToIndex, perimeterAction, outsideAction);

                    if (subPerimeter != null)
                    {
                        if (perimeter != null)
                        {
                            throw new InvalidOperationException("Unconnected perimeters");
                        }

                        perimeter = subPerimeter;
                    }
                }
            }

            if (!perimeterPointFound)
            {
                return perimeter;
            }
        }

        throw new InvalidOperationException("Infinite loop finding the room perimeter");
    }

    private static bool IsPerimeter(DirectionFlags neighborMap) =>
        (neighborMap & DirectionFlags.NorthwestCorner) == DirectionFlags.NorthwestCorner
        || (neighborMap & DirectionFlags.NortheastCorner) == DirectionFlags.NortheastCorner
        || (neighborMap & DirectionFlags.SoutheastCorner) == DirectionFlags.SoutheastCorner
        || (neighborMap & DirectionFlags.SouthwestCorner) == DirectionFlags.SouthwestCorner;

    protected static Dictionary<string, Func<TMapFragment, object?, bool>> GetPropertyConditions<TMapFragment>()
        where TMapFragment : MapFragment => new()
    {
        { nameof(Name), (_, v) => v != null },
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        { nameof(GenerationWeight), (_, v) => v != null && (string)v != DefaultWeight },
        { nameof(Map), (_, v) => !string.IsNullOrEmpty((string)v!) },
        { nameof(ByteMap), (_, v) => false },
        { nameof(PayloadArea), (_, v) => false },
        { nameof(Width), (_, v) => false },
        { nameof(Height), (_, v) => false },
        { nameof(ConditionalFirstColumn), (_, v) => false },
        { nameof(ConditionalFirstRow), (_, v) => false },
        { nameof(ConditionalLastColumn), (_, v) => false },
        { nameof(ConditionalLastRow), (_, v) => false }
    };

    public abstract ICSScriptSerializer GetSerializer();
}
