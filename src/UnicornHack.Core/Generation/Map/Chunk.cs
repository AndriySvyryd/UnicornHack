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
        MapFragment fragment,
        IReadOnlySet<Point>? doorwayPoints,
        IReadOnlySet<Point>? insidePoints,
        IReadOnlySet<Point>? perimeterPoints,
        RoomParameters roomParameters,
        IReadOnlyList<(Point Point, RoomFeatures? Feature)>? featurePoints = null,
        IReadOnlyList<Point>? creaturePoints = null,
        IReadOnlyList<Point>? trapPoints = null)
    {
        ArgumentNullException.ThrowIfNull(roomParameters);

        Room = room;

        BoundingRectangle = boundingRectangle;
        Fragment = fragment;
        DoorwayPoints = doorwayPoints ?? new HashSet<Point>();
        InsidePoints = insidePoints ?? new HashSet<Point>();
        PerimeterPoints = perimeterPoints ?? new HashSet<Point>();
        FeaturePoints = featurePoints ?? Array.Empty<(Point, RoomFeatures?)>();
        CreaturePoints = creaturePoints ?? Array.Empty<Point>();
        TrapPoints = trapPoints ?? Array.Empty<Point>();

        DebugAssertPointsInRoom();

        // Register every cell this chunk owns on the room's generation-time
        // chunk-cell map (carried by RoomParameters) so the walkable-wins overlap
        // rule in MapFragment.Write can distinguish chunk-placed terrain (which a
        // later wall write must preserve when walkable) from the room's background
        // default fill (which is freely overwritable). The map lives only for the
        // duration of generation.
        //
        // Every fragment — including the <see cref="DefiningMapFragment" /> — claims
        // its cells: by design the only cells NOT registered are those still
        // carrying the room's <see cref="RoomParameters.DefaultTerrain" /> background
        // fill. The defining fragment is the room's first chunk and establishes
        // both its own floor/wall/feature payload and the room's default fill via
        // <see cref="LevelLayout.AllocateTerrain" />.
        //
        // Last-write-wins when chunks overlap so the most-recently-placed chunk
        // owns the cell.
        foreach (var p in InsidePoints)
        {
            roomParameters[p] = this;
        }
        foreach (var p in PerimeterPoints)
        {
            roomParameters[p] = this;
        }
    }

    [Conditional("DEBUG")]
    private void DebugAssertPointsInRoom()
    {
        var bounds = Room.BoundingRectangle;
        foreach (var p in InsidePoints)
        {
            Debug.Assert(bounds.Contains(p),
                $"Chunk '{Fragment.Name}' has InsidePoint {p} outside room bounds {bounds}.");
        }
        foreach (var p in PerimeterPoints)
        {
            Debug.Assert(bounds.Contains(p),
                $"Chunk '{Fragment.Name}' has PerimeterPoint {p} outside room bounds {bounds}.");
        }
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

    /// <summary>
    ///     Cells whose payload glyph matched a key in <see cref="MapFragment.FeatureGlyphs" />.
    /// </summary>
    public IReadOnlyList<(Point Point, RoomFeatures? Feature)> FeaturePoints
    {
        get;
    }

    /// <summary>
    ///     Cells whose payload glyph matched <see cref="MapFragment.CreatureWildcardGlyph" />,
    ///     or that an annotation pass bound to a specific creature. The creature generator
    ///     picks a difficulty-appropriate creature for each unannotated cell at spawn time.
    /// </summary>
    public IReadOnlyList<Point> CreaturePoints
    {
        get;
    }

    /// <summary>
    ///     Cells whose payload glyph matched <c>^</c>. Recorded for a follow-up pass
    ///     that materializes trap entities at these positions; until then the underlying
    ///     tile is plain stone floor.
    /// </summary>
    public IReadOnlyList<Point> TrapPoints
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
        // Doorway points are the preferred targets (carved openings facing outward),
        // but open-perimeter chunks (e.g. corner pieces with no `+` doorways) connect
        // through their open floor edge instead. Fall back to any inside point so the
        // corridor system can still reach them — the corridor will exit the chunk via
        // whichever floor cell the planner picks.
        var candidates = DoorwayPoints;
        if (candidates.Count == 0)
        {
            candidates = InsidePoints;
        }

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException(
                $"Chunk for fragment '{Fragment.Name}' at {BoundingRectangle} has no doorway or inside points and cannot be connected.");
        }

        var targetRect = chunk.BoundingRectangle;
        byte maxDistance = 0;
        // The closer points are much more likely to get chosen. Add 1 so the farthest
        // point still has positive weight — otherwise WeightedOrder yields nothing when
        // every doorway sits at maxDistance (e.g. a single doorway), and .First() throws.
        var connectionPointDistances = new (Point Point, byte Distance)[candidates.Count];
        var idx = 0;
        foreach (var p in candidates)
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
