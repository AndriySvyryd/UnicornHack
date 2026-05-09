using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

public class Chunk
{
    private byte _x1;
    private byte _x2;
    private byte _y1;
    private byte _y2;

    public Chunk(
        RoomComponent room,
        Rectangle boundingRectangle,
        IReadOnlySet<Point>? doorwayPoints,
        IReadOnlySet<Point>? insidePoints,
        IReadOnlySet<Point>? perimeterPoints,
        IReadOnlyList<(Point, char)> predefinedPoints,
        MapFragment fragment)
    {
        Room = room;

        BoundingRectangle = boundingRectangle;
        DoorwayPoints = doorwayPoints ?? new HashSet<Point>();
        InsidePoints = insidePoints ?? new HashSet<Point>();
        PerimeterPoints = perimeterPoints ?? new HashSet<Point>();
        PredefinedPoints = predefinedPoints;
        Fragment = fragment;
    }

    public RoomComponent Room
    {
        get;
        set;
    }

    public Rectangle BoundingRectangle
    {
        get => new Rectangle(new Point(_x1, _y1), new Point(_x2, _y2));
        set
        {
            _x1 = value.TopLeft.X;
            _y1 = value.TopLeft.Y;
            _x2 = value.BottomRight.X;
            _y2 = value.BottomRight.Y;
        }
    }

    public IReadOnlySet<Point> DoorwayPoints
    {
        get;
    }

    public IReadOnlySet<Point> InsidePoints
    {
        get;
    }

    /// <summary>
    ///     Perimeter wall tiles that the fragment writes on the chunk's boundary. Includes
    ///     every tile of the chunk's outline whether or not it qualifies as a doorway —
    ///     <see cref="DoorwayPoints" /> is a subset filtered to walls that face floor on
    ///     the outside.
    /// </summary>
    public IReadOnlySet<Point> PerimeterPoints
    {
        get;
    }

    public MapFragment Fragment
    {
        get;
    }

    public IReadOnlyList<(Point, char)> PredefinedPoints
    {
        get;
    }

    public Chunk? GetClosest(IEnumerable<Chunk> chunks)
    {
        Chunk? closest = null;
        var distance = int.MaxValue;
        foreach (var chunk in chunks)
        {
            var nextDistance = BoundingRectangle.DistanceTo(chunk.BoundingRectangle);
            if (nextDistance < distance)
            {
                distance = nextDistance;
                closest = chunk;
            }
        }

        return closest;
    }

    public Chunk? GetOrthogonallyClosest(IEnumerable<Chunk> chunks)
    {
        Chunk? closest = null;
        var distance = int.MaxValue;
        foreach (var chunk in chunks)
        {
            var nextDistance = BoundingRectangle.OrthogonalDistanceTo(chunk.BoundingRectangle);
            if (nextDistance < distance)
            {
                distance = nextDistance;
                closest = chunk;
            }
        }

        return closest;
    }

    public Point GetGoodConnectionPoint(Chunk chunk)
    {
        if (DoorwayPoints.Count == 0)
        {
            throw new InvalidOperationException(
                $"Chunk for fragment '{Fragment.Name}' at {BoundingRectangle} has no doorway points and cannot be connected.");
        }

        var targetRect = chunk.BoundingRectangle;
        byte maxDistance = 0;
        // The closer points are much more likely to get chosen. Add 1 so the farthest
        // point still has positive weight — otherwise WeightedOrder yields nothing when
        // every doorway sits at maxDistance (e.g. a single doorway), and .First() throws.
        var connectionPointDistances = new (Point Point, byte Distance)[DoorwayPoints.Count];
        var idx = 0;
        foreach (var p in DoorwayPoints)
        {
            var d = targetRect.DistanceTo(p);
            if (d > maxDistance)
            {
                maxDistance = d;
            }
            connectionPointDistances[idx++] = (p, targetRect.DistanceTo(p));
        }

        return Room.Level.GenerationRandom.WeightedOrder(connectionPointDistances, p => ((maxDistance - p.Distance) << 2) + 1).First().Point;
    }
}
