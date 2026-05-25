namespace UnicornHack.Generation.Map;

/// <summary>
///     Direct shape-correctness tests for <see cref="IrregularBlobMap" /> — the random-walk
///     blob carver previously only exercised end-to-end through full level generation.
/// </summary>
public class IrregularBlobMapTest
{
    [Fact]
    public void Carves_at_least_the_min_area()
    {
        var (room, _) = MapFragmentTest.BuildRoomForBlobTest(roomSize: 12);
        var bounds = new Rectangle(new Point(1, 1), 8, 8);
        var minSize = new Dimensions(3, 3);
        var map = new IrregularBlobMap { MinSize = minSize, Coverage = 0f };

        var points = map.GetRoomPoints(room, bounds).ToList();

        Assert.True(points.Count >= minSize.Width * minSize.Height,
            $"Expected at least {minSize.Width * minSize.Height} cells, got {points.Count}.");
    }

    [Fact]
    public void Caps_at_the_bounding_rectangle_area()
    {
        var (room, _) = MapFragmentTest.BuildRoomForBlobTest(roomSize: 12);
        var bounds = new Rectangle(new Point(1, 1), 6, 6);
        var map = new IrregularBlobMap { MinSize = new Dimensions(2, 2), Coverage = 5f };

        var points = map.GetRoomPoints(room, bounds).ToList();

        Assert.True(points.Count <= bounds.Area,
            $"Carved {points.Count} cells but bounds area is {bounds.Area}.");
    }

    [Fact]
    public void Carved_points_lie_inside_bounding_rectangle()
    {
        var (room, _) = MapFragmentTest.BuildRoomForBlobTest(roomSize: 12);
        var bounds = new Rectangle(new Point(2, 2), 7, 7);
        var map = new IrregularBlobMap { MinSize = new Dimensions(2, 2), Coverage = 0.5f };

        foreach (var p in map.GetRoomPoints(room, bounds))
        {
            Assert.True(bounds.Contains(p), $"Carved point {p} lies outside bounds {bounds}.");
        }
    }

    [Fact]
    public void Carved_points_are_unique_and_yielded_in_row_major_order()
    {
        var (room, _) = MapFragmentTest.BuildRoomForBlobTest(roomSize: 12);
        var bounds = new Rectangle(new Point(1, 1), 7, 7);
        var map = new IrregularBlobMap { MinSize = new Dimensions(2, 2), Coverage = 0.7f };

        var points = map.GetRoomPoints(room, bounds).ToList();
        var distinct = new HashSet<Point>(points);

        Assert.Equal(points.Count, distinct.Count);
        for (var i = 1; i < points.Count; i++)
        {
            var prev = points[i - 1];
            var curr = points[i];
            var prevKey = (prev.Y * 256) + prev.X;
            var currKey = (curr.Y * 256) + curr.X;
            Assert.True(currKey > prevKey,
                $"Point {curr} at index {i} is not strictly after {prev} in row-major order.");
        }
    }

    [Fact]
    public void Central_cross_always_reaches_every_cardinal_midpoint()
    {
        // The doc-comment promises the central cross hits every cardinal midpoint so
        // door-n/s/e/w tags aren't a lie. Verify with Coverage=0 (only the seed cross
        // gets carved).
        var (room, _) = MapFragmentTest.BuildRoomForBlobTest(roomSize: 12);
        var bounds = new Rectangle(new Point(1, 1), 9, 9);
        var map = new IrregularBlobMap { MinSize = new Dimensions(1, 1), Coverage = 0f };

        var carved = map.GetRoomPoints(room, bounds).ToHashSet();
        var midX = (byte)((bounds.TopLeft.X + bounds.BottomRight.X) / 2);
        var midY = (byte)((bounds.TopLeft.Y + bounds.BottomRight.Y) / 2);

        Assert.Contains(new Point(midX, bounds.TopLeft.Y), carved);
        Assert.Contains(new Point(midX, bounds.BottomRight.Y), carved);
        Assert.Contains(new Point(bounds.TopLeft.X, midY), carved);
        Assert.Contains(new Point(bounds.BottomRight.X, midY), carved);
    }

    [Fact]
    public void Carved_region_is_orthogonally_connected()
    {
        // Frontier expansion is 4-connectivity by construction; the FillHoles pass adds
        // interior cells that were enclosed but never separates the blob. Verify with
        // a BFS over the carved set.
        var (room, _) = MapFragmentTest.BuildRoomForBlobTest(roomSize: 14);
        var bounds = new Rectangle(new Point(1, 1), 10, 10);
        var map = new IrregularBlobMap { MinSize = new Dimensions(3, 3), Coverage = 0.6f };

        var carved = map.GetRoomPoints(room, bounds).ToHashSet();
        Assert.NotEmpty(carved);

        var seed = carved.First();
        var reached = new HashSet<Point> { seed };
        var queue = new Queue<Point>();
        queue.Enqueue(seed);
        Span<(int dx, int dy)> deltas = stackalloc (int, int)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
        while (queue.Count > 0)
        {
            var p = queue.Dequeue();
            foreach (var (dx, dy) in deltas)
            {
                var nx = p.X + dx;
                var ny = p.Y + dy;
                if (nx < 0 || ny < 0 || nx > byte.MaxValue || ny > byte.MaxValue)
                {
                    continue;
                }
                var n = new Point((byte)nx, (byte)ny);
                if (carved.Contains(n) && reached.Add(n))
                {
                    queue.Enqueue(n);
                }
            }
        }

        Assert.Equal(carved.Count, reached.Count);
    }

    [Fact]
    public void Flush_north_pin_carves_entire_north_edge_run()
    {
        var (room, _) = MapFragmentTest.BuildRoomForBlobTest(roomSize: 12);
        var bounds = new Rectangle(new Point(1, 1), 8, 8);
        var map = new IrregularBlobMap { MinSize = new Dimensions(2, 2), Coverage = 0f };

        var carved = map.GetRoomPoints(room, bounds, flushEdges: DirectionFlags.North).ToHashSet();

        for (var x = bounds.TopLeft.X; x <= bounds.BottomRight.X; x++)
        {
            Assert.Contains(new Point((byte)x, bounds.TopLeft.Y), carved);
        }
    }
}
