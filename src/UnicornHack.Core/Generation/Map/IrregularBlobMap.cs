using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

/// <summary>
///     Carves an organic, non-rectangular floor area inside the supplied bounding rectangle
///     using a random-walk frontier expansion seeded from a central cross. The cross
///     guarantees the blob reaches every edge of the bounding rectangle at its midpoint, so
///     any cardinal-aligned doorway can find a wall to anchor against — that lets the
///     fragment advertise <c>door-n/s/e/w</c> tags without lying. After the cross seed the
///     algorithm picks random frontier cells (cells adjacent to the carved set that aren't
///     yet carved) and adds them until a coverage threshold is met, producing a connected
///     blob with irregular perimeter.
/// </summary>
public class IrregularBlobMap : DynamicMap
{
    /// <summary>
    ///     Target fraction of the bounding rectangle area to carve, in the inclusive
    ///     range [0, 1]. The actual carved count is clamped to <see cref="DynamicMap.MinSize"/>
    ///     area at the bottom and the bounding-rectangle area at the top.
    /// </summary>
    public float Coverage
    {
        get;
        set;
    } = 0.7f;

    public override IEnumerable<Point> GetRoomPoints(RoomComponent room, Rectangle boundingRectangle,
        DirectionFlags preferredEdges = DirectionFlags.None, DirectionFlags flushEdges = DirectionFlags.None)
    {
        var random = room.Level.GenerationRandom;
        var topLeft = boundingRectangle.TopLeft;
        var bottomRight = boundingRectangle.BottomRight;
        var width = boundingRectangle.Width;
        var height = boundingRectangle.Height;
        var area = width * height;
        var midX = (byte)((topLeft.X + bottomRight.X) / 2);
        var midY = (byte)((topLeft.Y + bottomRight.Y) / 2);

        var carved = new HashSet<Point>();

        // Seed with a central cross so the blob always reaches every cardinal edge of the
        // bounding rectangle at the midpoint — required to honour the door-n/s/e/w tags.
        for (var y = topLeft.Y; y <= bottomRight.Y; y++)
        {
            carved.Add(new Point(midX, (byte)y));
        }

        for (var x = topLeft.X; x <= bottomRight.X; x++)
        {
            carved.Add(new Point((byte)x, midY));
        }

        // When a side is flush-pinned, force a full edge run so the chunk abuts the
        // existing neighbor along that side and doorway carving has a continuous wall to
        // work against.
        if ((flushEdges & DirectionFlags.North) != 0)
        {
            for (var x = topLeft.X; x <= bottomRight.X; x++)
            {
                carved.Add(new Point((byte)x, topLeft.Y));
            }
        }

        if ((flushEdges & DirectionFlags.South) != 0)
        {
            for (var x = topLeft.X; x <= bottomRight.X; x++)
            {
                carved.Add(new Point((byte)x, bottomRight.Y));
            }
        }

        if ((flushEdges & DirectionFlags.West) != 0)
        {
            for (var y = topLeft.Y; y <= bottomRight.Y; y++)
            {
                carved.Add(new Point(topLeft.X, (byte)y));
            }
        }

        if ((flushEdges & DirectionFlags.East) != 0)
        {
            for (var y = topLeft.Y; y <= bottomRight.Y; y++)
            {
                carved.Add(new Point(bottomRight.X, (byte)y));
            }
        }

        var minArea = MinSize.Width * MinSize.Height;
        var target = (int)(area * Coverage);
        if (target < minArea)
        {
            target = minArea;
        }

        if (target > area)
        {
            target = area;
        }

        // Build initial frontier — cells adjacent to the seed that aren't carved yet.
        // `frontierSet` mirrors `frontier` so AddFrontierNeighbors / BiasFrontier can
        // skip points already pending: a `List<Point>` is needed for O(1) random pick,
        // but unguarded appends produced O(duplicates) wasted iterations.
        var frontier = new List<Point>();
        var frontierSet = new HashSet<Point>();
        foreach (var p in carved)
        {
            AddFrontierNeighbors(p, carved, frontier, frontierSet, topLeft, bottomRight);
        }

        // Bias expansion toward preferred edges by occasionally seeding the frontier with
        // an extra cell on those sides. Cheap, and improves perceived adjacency without
        // requiring a weighted picker.
        BiasFrontier(frontier, frontierSet, carved, preferredEdges, topLeft, bottomRight);

        while (carved.Count < target && frontier.Count > 0)
        {
            var pickIndex = random.Next(0, frontier.Count);
            var pick = frontier[pickIndex];
            var last = frontier.Count - 1;
            frontier[pickIndex] = frontier[last];
            frontier.RemoveAt(last);
            frontierSet.Remove(pick);

            if (!carved.Add(pick))
            {
                continue;
            }

            AddFrontierNeighbors(pick, carved, frontier, frontierSet, topLeft, bottomRight);
        }

        // Fill any interior holes. BuildChunk's WalkPerimeter assumes a simply-connected
        // shape; uncarved cells that random expansion enclosed would each spawn their own
        // inner perimeter loop and trip the "Unconnected perimeters" guard. Flood from the
        // bounding-rect perimeter through uncarved cells to identify the exterior, then
        // carve everything left over.
        FillHoles(carved, topLeft, bottomRight);

        // Yield in row-major order so BuildChunk's neighbor walk sees rows contiguously.
        for (var y = topLeft.Y; y <= bottomRight.Y; y++)
        {
            for (var x = topLeft.X; x <= bottomRight.X; x++)
            {
                var p = new Point((byte)x, (byte)y);
                if (carved.Contains(p))
                {
                    yield return p;
                }
            }
        }
    }

    private static void AddFrontierNeighbors(
        Point center, HashSet<Point> carved, List<Point> frontier, HashSet<Point> frontierSet,
        Point topLeft, Point bottomRight)
    {
        // Cardinal directions are at even indices in MovementDirections (E, N, W, S);
        // odd indices are intercardinals which we skip — the blob expands only
        // orthogonally so its 4-connectivity matches doorway carving's assumptions.
        for (var directionIndex = 0; directionIndex < 8; directionIndex += 2)
        {
            var direction = Vector.MovementDirections[directionIndex];
            var nx = center.X + direction.X;
            var ny = center.Y + direction.Y;
            if (nx < topLeft.X || nx > bottomRight.X || ny < topLeft.Y || ny > bottomRight.Y)
            {
                continue;
            }

            var neighbor = new Point((byte)nx, (byte)ny);
            if (carved.Contains(neighbor))
            {
                continue;
            }

            if (frontierSet.Add(neighbor))
            {
                frontier.Add(neighbor);
            }
        }
    }

    private static void FillHoles(HashSet<Point> carved, Point topLeft, Point bottomRight)
    {
        var exterior = new HashSet<Point>();
        var queue = new Queue<Point>();

        // Seed the flood with every uncarved cell on the bounding-rect perimeter.
        for (var x = topLeft.X; x <= bottomRight.X; x++)
        {
            TrySeed(new Point((byte)x, topLeft.Y), carved, exterior, queue);
            TrySeed(new Point((byte)x, bottomRight.Y), carved, exterior, queue);
        }

        for (var y = topLeft.Y; y <= bottomRight.Y; y++)
        {
            TrySeed(new Point(topLeft.X, (byte)y), carved, exterior, queue);
            TrySeed(new Point(bottomRight.X, (byte)y), carved, exterior, queue);
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            for (var directionIndex = 0; directionIndex < 8; directionIndex += 2)
            {
                var direction = Vector.MovementDirections[directionIndex];
                var nx = current.X + direction.X;
                var ny = current.Y + direction.Y;
                if (nx < topLeft.X || nx > bottomRight.X || ny < topLeft.Y || ny > bottomRight.Y)
                {
                    continue;
                }

                TrySeed(new Point((byte)nx, (byte)ny), carved, exterior, queue);
            }
        }

        // Anything not in the exterior and not already carved is an interior hole.
        for (var y = topLeft.Y; y <= bottomRight.Y; y++)
        {
            for (var x = topLeft.X; x <= bottomRight.X; x++)
            {
                var p = new Point((byte)x, (byte)y);
                if (!carved.Contains(p) && !exterior.Contains(p))
                {
                    carved.Add(p);
                }
            }
        }
    }

    private static void TrySeed(Point p, HashSet<Point> carved, HashSet<Point> exterior, Queue<Point> queue)
    {
        if (carved.Contains(p))
        {
            return;
        }

        if (exterior.Add(p))
        {
            queue.Enqueue(p);
        }
    }

    private static void BiasFrontier(
        List<Point> frontier, HashSet<Point> frontierSet, HashSet<Point> carved,
        DirectionFlags preferredEdges, Point topLeft, Point bottomRight)
    {
        if (preferredEdges == DirectionFlags.None)
        {
            return;
        }

        if ((preferredEdges & DirectionFlags.North) != 0)
        {
            for (var x = topLeft.X; x <= bottomRight.X; x++)
            {
                TryAddFrontier(new Point((byte)x, topLeft.Y), carved, frontier, frontierSet);
            }
        }

        if ((preferredEdges & DirectionFlags.South) != 0)
        {
            for (var x = topLeft.X; x <= bottomRight.X; x++)
            {
                TryAddFrontier(new Point((byte)x, bottomRight.Y), carved, frontier, frontierSet);
            }
        }

        if ((preferredEdges & DirectionFlags.West) != 0)
        {
            for (var y = topLeft.Y; y <= bottomRight.Y; y++)
            {
                TryAddFrontier(new Point(topLeft.X, (byte)y), carved, frontier, frontierSet);
            }
        }

        if ((preferredEdges & DirectionFlags.East) != 0)
        {
            for (var y = topLeft.Y; y <= bottomRight.Y; y++)
            {
                TryAddFrontier(new Point(bottomRight.X, (byte)y), carved, frontier, frontierSet);
            }
        }
    }

    private static void TryAddFrontier(
        Point point, HashSet<Point> carved, List<Point> frontier, HashSet<Point> frontierSet)
    {
        if (carved.Contains(point))
        {
            return;
        }
        if (frontierSet.Add(point))
        {
            frontier.Add(point);
        }
    }
}
