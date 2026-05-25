namespace UnicornHack.Utils.DataStructures;

public class RectangleTest
{
    [Fact]
    public void Constructor_validates_arguments()
    {
        // (Point, Point) ctor rejects topLeft > bottomRight on either axis.
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Rectangle(new Point(5, 0), new Point(3, 0)));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Rectangle(new Point(0, 5), new Point(0, 3)));

        // (Point, width, height) ctor rejects zero on either axis.
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Rectangle(new Point(1, 1), 0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Rectangle(new Point(1, 1), 1, 0));
    }

    [Fact]
    public void Properties()
    {
        Assert.Equal(new Segment(0, 4), new Rectangle(new Point(0, 0), new Point(4, 4)).XProjection);
        Assert.Equal(new Segment(1, 7), new Rectangle(new Point(6, 1), new Point(8, 7)).YProjection);
        Assert.Equal(1, new Rectangle(new Point(6, 1), 1, 2).Width);
        Assert.Equal(2, new Rectangle(new Point(6, 1), 1, 2).Height);
        Assert.Equal(1, new Rectangle(new Point(6, 7), new Point(6, 7)).Width);
        Assert.Equal(1, new Rectangle(new Point(6, 7), new Point(6, 7)).Height);
        Assert.Equal(6, new Rectangle(new Point(0, 2), new Point(2, 3)).Area);

        // (Segment, Segment) ctor maps projections to corners.
        var fromSegments = new Rectangle(new Segment(2, 6), new Segment(3, 9));
        Assert.Equal(new Point(2, 3), fromSegments.TopLeft);
        Assert.Equal(new Point(6, 9), fromSegments.BottomRight);
    }

    [Fact]
    public void Intersection()
    {
        Assert.Equal(new Rectangle(new Point(3, 3), new Point(4, 4)),
            new Rectangle(new Point(0, 0), new Point(4, 4))
                .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
        Assert.Equal(new Rectangle(new Point(6, 3), new Point(7, 7)),
            new Rectangle(new Point(6, 1), new Point(8, 7))
                .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
        Assert.Equal(new Rectangle(new Point(6, 7), new Point(7, 7)),
            new Rectangle(new Point(6, 7), new Point(10, 9))
                .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
        Assert.Equal(new Rectangle(new Point(3, 3), new Point(3, 7)),
            new Rectangle(new Point(1, 2), new Point(3, 10))
                .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
        Assert.Equal(new Rectangle(new Point(4, 4), new Point(6, 6)),
            new Rectangle(new Point(4, 4), new Point(6, 6))
                .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
        Assert.Equal(new Rectangle(new Point(5, 5), new Point(5, 5)),
            new Rectangle(new Point(5, 5), new Point(5, 5))
                .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
        Assert.Equal(new Rectangle(new Point(3, 3), new Point(7, 7)),
            new Rectangle(new Point(0, 0), new Point(10, 10))
                .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
        Assert.Equal(new Rectangle(new Point(3, 5), new Point(7, 6)),
            new Rectangle(new Point(0, 5), new Point(9, 6))
                .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
        Assert.Equal(new Rectangle(new Point(3, 7), new Point(7, 7)),
            new Rectangle(new Point(2, 7), new Point(10, 9))
                .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
        Assert.Equal(new Rectangle(new Point(4, 3), new Point(5, 7)),
            new Rectangle(new Point(4, 1), new Point(5, 8))
                .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
        Assert.Equal(new Rectangle(new Point(3, 3), new Point(7, 7)),
            new Rectangle(new Point(2, 3), new Point(8, 7))
                .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
        Assert.Equal(new Rectangle(new Point(3, 3), new Point(7, 7)),
            new Rectangle(new Point(3, 0), new Point(7, 10))
                .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
        Assert.Null(
            new Rectangle(new Point(1, 1), new Point(8, 2))
                .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
        Assert.Null(
            new Rectangle(new Point(1, 4), new Point(2, 6))
                .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
        Assert.Null(
            new Rectangle(new Point(8, 2), new Point(10, 9))
                .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
    }

    [Fact]
    public void Perimeter()
    {
        Assert.Equal(
            new[]
            {
                new Point(1, 2), new Point(2, 2), new Point(3, 2), new Point(3, 3), new Point(3, 4),
                new Point(2, 4), new Point(1, 4), new Point(1, 3)
            },
            new Rectangle(new Point(1, 2), new Point(3, 4)).GetPerimeter());

        Assert.Equal(new[] { new Point(0, 2), new Point(0, 3), new Point(0, 4) },
            new Rectangle(new Point(0, 2), new Point(0, 4)).GetPerimeter());

        Assert.Equal(new[] { new Point(1, 0), new Point(2, 0), new Point(3, 0) },
            new Rectangle(new Point(1, 0), new Point(3, 0)).GetPerimeter());

        Assert.Equal(new[] { new Point(1, 1) },
            new Rectangle(new Point(1, 1), new Point(1, 1)).GetPerimeter());
    }

    [Fact]
    public void TopRight_and_BottomLeft_derive_from_corners()
    {
        var rect = new Rectangle(new Point(1, 2), new Point(5, 7));
        Assert.Equal(new Point(5, 2), rect.TopRight);
        Assert.Equal(new Point(1, 7), rect.BottomLeft);
    }

    [Fact]
    public void Dimensions_property_matches_width_height()
    {
        var rect = new Rectangle(new Point(0, 0), 4, 7);
        Assert.Equal(new Dimensions(4, 7), rect.Dimensions);

        // The (Point, Dimensions) ctor is equivalent to (Point, byte, byte).
        var fromDims = new Rectangle(new Point(2, 3), new Dimensions(4, 5));
        var fromBytes = new Rectangle(new Point(2, 3), 4, 5);
        Assert.Equal(fromBytes, fromDims);
    }

    [Fact]
    public void Overlaps_matches_intersection()
    {
        var anchor = new Rectangle(new Point(3, 3), new Point(7, 7));
        Assert.True(anchor.Overlaps(new Rectangle(new Point(5, 5), new Point(9, 9))));
        Assert.True(anchor.Overlaps(new Rectangle(new Point(7, 7), new Point(8, 8)))); // corner touch
        Assert.False(anchor.Overlaps(new Rectangle(new Point(8, 0), new Point(9, 9)))); // strictly right
        Assert.False(anchor.Overlaps(new Rectangle(new Point(0, 8), new Point(9, 9)))); // strictly below
    }

    [Fact]
    public void Encloses_requires_strict_containment_on_both_axes()
    {
        var outer = new Rectangle(new Point(0, 0), new Point(10, 10));
        // Strictly inside on both axes.
        Assert.True(outer.Encloses(new Rectangle(new Point(1, 1), new Point(9, 9))));
        // Touching the outer edge fails the strict check.
        Assert.False(outer.Encloses(new Rectangle(new Point(0, 1), new Point(9, 9))));
        Assert.False(outer.Encloses(new Rectangle(new Point(1, 1), new Point(10, 9))));
        // Equality fails the strict check.
        Assert.False(outer.Encloses(outer));
    }

    [Fact]
    public void Contains_Rectangle_allows_edge_equality()
    {
        var outer = new Rectangle(new Point(0, 0), new Point(10, 10));
        Assert.True(outer.Contains(outer));
        Assert.True(outer.Contains(new Rectangle(new Point(0, 0), new Point(10, 10))));
        Assert.True(outer.Contains(new Rectangle(new Point(2, 2), new Point(8, 8))));
        Assert.False(outer.Contains(new Rectangle(new Point(0, 0), new Point(11, 10))));
    }

    [Fact]
    public void Contains_Point_is_inclusive_on_all_edges()
    {
        var rect = new Rectangle(new Point(2, 3), new Point(5, 7));
        Assert.True(rect.Contains(new Point(2, 3))); // TopLeft
        Assert.True(rect.Contains(new Point(5, 7))); // BottomRight
        Assert.True(rect.Contains(new Point(5, 3))); // TopRight
        Assert.True(rect.Contains(new Point(2, 7))); // BottomLeft
        Assert.True(rect.Contains(new Point(3, 5))); // Interior
        Assert.False(rect.Contains(new Point(1, 5))); // West of
        Assert.False(rect.Contains(new Point(6, 5))); // East of
        Assert.False(rect.Contains(new Point(3, 2))); // North of
        Assert.False(rect.Contains(new Point(3, 8))); // South of
    }

    [Fact]
    public void Contains_Dimensions_checks_fit()
    {
        var rect = new Rectangle(new Point(0, 0), 5, 7);
        Assert.True(rect.Contains(new Dimensions(5, 7)));
        Assert.True(rect.Contains(new Dimensions(1, 1)));
        Assert.False(rect.Contains(new Dimensions(6, 1)));
        Assert.False(rect.Contains(new Dimensions(1, 8)));
    }

    [Fact]
    public void DistanceTo_Rectangle_uses_max_axis_gap()
    {
        var a = new Rectangle(new Point(0, 0), new Point(2, 2));
        // Overlapping → 0 on both axes.
        Assert.Equal(0, a.DistanceTo(new Rectangle(new Point(1, 1), new Point(3, 3))));
        // 4 cells east, 0 vertical → max = 4.
        Assert.Equal(4, a.DistanceTo(new Rectangle(new Point(6, 0), new Point(8, 2))));
        // 0 horizontal, 5 cells south → max = 5.
        Assert.Equal(5, a.DistanceTo(new Rectangle(new Point(0, 7), new Point(2, 9))));
        // Diagonal: X-gap 3, Y-gap 6 → max = 6 (regression test for the previous
        // bug where Y always evaluated to 0).
        Assert.Equal(6, a.DistanceTo(new Rectangle(new Point(5, 8), new Point(9, 10))));
    }

    [Fact]
    public void DistanceTo_Point_uses_max_axis_gap()
    {
        var rect = new Rectangle(new Point(2, 3), new Point(5, 7));
        Assert.Equal(0, rect.DistanceTo(new Point(3, 5))); // inside
        Assert.Equal(0, rect.DistanceTo(new Point(2, 3))); // corner
        Assert.Equal(3, rect.DistanceTo(new Point(8, 5))); // east
        Assert.Equal(4, rect.DistanceTo(new Point(3, 11))); // south
        Assert.Equal(5, rect.DistanceTo(new Point(10, 1))); // X-gap 5, Y-gap 2 → 5
    }

    [Fact]
    public void OrthogonalDistanceTo_sums_axis_gaps()
    {
        var a = new Rectangle(new Point(0, 0), new Point(2, 2));
        Assert.Equal(0, a.OrthogonalDistanceTo(new Rectangle(new Point(1, 1), new Point(3, 3))));
        Assert.Equal(4, a.OrthogonalDistanceTo(new Rectangle(new Point(6, 0), new Point(8, 2))));
        // X-gap 3, Y-gap 6 → 9
        Assert.Equal(9, a.OrthogonalDistanceTo(new Rectangle(new Point(5, 8), new Point(9, 10))));

        var rect = new Rectangle(new Point(2, 3), new Point(5, 7));
        Assert.Equal(0, rect.OrthogonalDistanceTo(new Point(3, 5)));
        // X-gap 5, Y-gap 2 → 7
        Assert.Equal(7, rect.OrthogonalDistanceTo(new Point(10, 1)));
    }

    [Fact]
    public void GetEnumerator_yields_row_major_points()
    {
        var rect = new Rectangle(new Point(1, 2), new Point(3, 3));
        var points = rect.ToList();
        Assert.Equal(
            new[]
            {
                new Point(1, 2), new Point(2, 2), new Point(3, 2),
                new Point(1, 3), new Point(2, 3), new Point(3, 3)
            },
            points);
    }

    [Fact]
    public void GetEnumerator_via_non_generic_interface_walks_same_sequence()
    {
        var rect = new Rectangle(new Point(0, 0), new Point(1, 1));
        var enumerable = (System.Collections.IEnumerable)rect;
        var points = new List<Point>();
        foreach (var p in enumerable)
        {
            points.Add((Point)p);
        }
        Assert.Equal(new[] { new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1) }, points);
    }

    [Fact]
    public void GetInside_excludes_perimeter()
    {
        var rect = new Rectangle(new Point(0, 0), new Point(3, 3));
        var inside = new List<Point>();
        var enumerator = rect.GetInside();
        while (enumerator.MoveNext())
        {
            inside.Add(enumerator.Current);
        }
        Assert.Equal(new[] { new Point(1, 1), new Point(2, 1), new Point(1, 2), new Point(2, 2) }, inside);
    }

    [Fact]
    public void GetInside_returns_empty_for_thin_rectangles()
    {
        Assert.False(new Rectangle(new Point(0, 0), new Point(1, 5)).GetInside().MoveNext()); // width 2
        Assert.False(new Rectangle(new Point(0, 0), new Point(5, 1)).GetInside().MoveNext()); // height 2
        Assert.False(new Rectangle(new Point(0, 0), new Point(0, 0)).GetInside().MoveNext()); // 1x1
    }

    [Fact]
    public void PlaceInside_returns_null_when_payload_too_big()
    {
        var lot = new Rectangle(new Point(0, 0), 4, 4);
        var payload = new Rectangle(new Point(0, 0), 5, 1);
        Assert.Null(lot.PlaceInside(payload, new SimpleRandom { Seed = 1u }));
        var tall = new Rectangle(new Point(0, 0), 1, 5);
        Assert.Null(lot.PlaceInside(tall, new SimpleRandom { Seed = 1u }));
    }

    [Fact]
    public void PlaceInside_returns_origin_when_payload_equals_lot()
    {
        var lot = new Rectangle(new Point(3, 4), 4, 4);
        var payload = new Rectangle(new Point(0, 0), 4, 4);
        Assert.Equal(new Point(3, 4), lot.PlaceInside(payload, new SimpleRandom { Seed = 1u }));
    }

    [Fact]
    public void PlaceInside_returns_point_inside_lot()
    {
        var lot = new Rectangle(new Point(5, 5), 10, 10);
        var payload = new Rectangle(new Point(0, 0), 3, 3);
        var random = new SimpleRandom { Seed = 42u };
        for (var i = 0; i < 50; i++)
        {
            var point = lot.PlaceInside(payload, random);
            Assert.NotNull(point);
            // Placed payload's top-left must allow the payload to fit entirely.
            Assert.InRange(point!.Value.X, lot.TopLeft.X, (byte)(lot.BottomRight.X - payload.Width + 1));
            Assert.InRange(point.Value.Y, lot.TopLeft.Y, (byte)(lot.BottomRight.Y - payload.Height + 1));
        }
    }

    [Fact]
    public void CreateRandom_produces_rectangle_inside_bounds()
    {
        var bounds = new Rectangle(new Point(2, 3), new Point(20, 25));
        var random = new SimpleRandom { Seed = 7u };
        for (var i = 0; i < 50; i++)
        {
            var rect = Rectangle.CreateRandom(random, bounds);
            Assert.True(bounds.Contains(rect));
            Assert.True(rect.Width >= 1);
            Assert.True(rect.Height >= 1);
        }
    }

    [Fact]
    public void CreateRandom_respects_minSize()
    {
        var bounds = new Rectangle(new Point(0, 0), new Point(20, 20));
        var minSize = new Dimensions(5, 4);
        var random = new SimpleRandom { Seed = 11u };
        for (var i = 0; i < 50; i++)
        {
            var rect = Rectangle.CreateRandom(random, bounds, minSize);
            Assert.True(bounds.Contains(rect));
            Assert.True(rect.Width >= minSize.Width);
            Assert.True(rect.Height >= minSize.Height);
        }
    }

    [Fact]
    public void CreateRandomNearEdge_flush_pins_each_cardinal_side()
    {
        var bounds = new Rectangle(new Point(2, 3), new Point(15, 20));
        var minSize = new Dimensions(2, 2);
        var random = new SimpleRandom { Seed = 13u };
        for (var i = 0; i < 20; i++)
        {
            var west = Rectangle.CreateRandomNearEdge(
                random, bounds, minSize, DirectionFlags.None, DirectionFlags.West);
            Assert.Equal(bounds.TopLeft.X, west.TopLeft.X);

            var east = Rectangle.CreateRandomNearEdge(
                random, bounds, minSize, DirectionFlags.None, DirectionFlags.East);
            Assert.Equal(bounds.BottomRight.X, east.BottomRight.X);

            var north = Rectangle.CreateRandomNearEdge(
                random, bounds, minSize, DirectionFlags.None, DirectionFlags.North);
            Assert.Equal(bounds.TopLeft.Y, north.TopLeft.Y);

            var south = Rectangle.CreateRandomNearEdge(
                random, bounds, minSize, DirectionFlags.None, DirectionFlags.South);
            Assert.Equal(bounds.BottomRight.Y, south.BottomRight.Y);
        }
    }

    [Fact]
    public void CreateRandomNearEdge_bias_keeps_result_inside_bounds()
    {
        var bounds = new Rectangle(new Point(0, 0), new Point(30, 30));
        var minSize = new Dimensions(2, 2);
        var random = new SimpleRandom { Seed = 17u };
        for (var i = 0; i < 50; i++)
        {
            var rect = Rectangle.CreateRandomNearEdge(
                random, bounds, minSize,
                preferredEdges: DirectionFlags.West | DirectionFlags.North,
                flushEdges: DirectionFlags.None);
            Assert.True(bounds.Contains(rect),
                $"Biased rectangle {rect} escaped bounds {bounds}.");
        }
    }

    [Fact]
    public void GetPointIndex_returns_cached_instance_for_same_dimensions()
    {
        var (pti1, itp1) = Rectangle.GetPointIndex(4, 3);
        var (pti2, itp2) = Rectangle.GetPointIndex(4, 3);
        Assert.Same(pti1, pti2);
        Assert.Same(itp1, itp2);
    }

    [Fact]
    public void GetPointIndex_round_trips_points_to_indices()
    {
        const byte width = 5;
        const byte height = 3;
        var (pointToIndex, indexToPoint) = Rectangle.GetPointIndex(width, height);

        Assert.Equal(new[,]
            {
                { (short)0, (short)5, (short)10 },
                { (short)1, (short)6, (short)11 },
                { (short)2, (short)7, (short)12 },
                { (short)3, (short)8, (short)13 },
                { (short)4, (short)9, (short)14 }
            },
            pointToIndex);

        for (byte y = 0; y < height; y++)
        {
            for (byte x = 0; x < width; x++)
            {
                Assert.Equal(new Point(x, y), indexToPoint[pointToIndex[x, y]]);
            }
        }
    }

    [Fact]
    public void Equality_compares_corners()
    {
        var a = new Rectangle(new Point(1, 2), new Point(3, 4));
        var b = new Rectangle(new Point(1, 2), new Point(3, 4));
        var c = new Rectangle(new Point(1, 2), new Point(3, 5));

        Assert.True(a.Equals(b));
        Assert.True(a.Equals((object)b));
        Assert.False(a.Equals(c));
        Assert.False(a.Equals((object?)null));
        Assert.False(a.Equals("not a rectangle"));
        Assert.True(a == b);
        Assert.True(a != c);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ToString_shows_both_corners()
    {
        var rect = new Rectangle(new Point(1, 2), new Point(3, 4));
        var rendered = rect.ToString();
        Assert.Contains("(1, 2)", rendered);
        Assert.Contains("(3, 4)", rendered);
    }
}
