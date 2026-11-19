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

    [Theory]
    [InlineData(0, 0, 4, 4)]
    [InlineData(6, 1, 8, 7)]
    [InlineData(6, 7, 6, 7)]
    [InlineData(0, 2, 2, 3)]
    [InlineData(1, 2, 5, 7)]
    public void Properties_and_constructor_forms_agree(byte left, byte top, byte right, byte bottom)
    {
        var topLeft = new Point(left, top);
        var bottomRight = new Point(right, bottom);
        var width = (byte)(right - left + 1);
        var height = (byte)(bottom - top + 1);
        Rectangle[] rectangles =
        [
            new(topLeft, bottomRight),
            new(topLeft, width, height),
            new(topLeft, new Dimensions(width, height)),
            new(new Segment(left, right), new Segment(top, bottom))
        ];
        foreach (var rectangle in rectangles)
        {
            Assert.Equal(rectangles[0], rectangle);
            Assert.Equal(topLeft, rectangle.TopLeft);
            Assert.Equal(bottomRight, rectangle.BottomRight);
            Assert.Equal(new Point(right, top), rectangle.TopRight);
            Assert.Equal(new Point(left, bottom), rectangle.BottomLeft);
            Assert.Equal(width, rectangle.Width);
            Assert.Equal(height, rectangle.Height);
            Assert.Equal(width * height, rectangle.Area);
            Assert.Equal(new Dimensions(width, height), rectangle.Dimensions);
            Assert.Equal(new Segment(left, right), rectangle.XProjection);
            Assert.Equal(new Segment(top, bottom), rectangle.YProjection);
        }
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

    [Theory]
    [InlineData(1, 2, 3, 3)]
    [InlineData(0, 0, 1, 1)]
    [InlineData(3, 4, 3, 4)]
    public void Both_enumerator_interfaces_yield_row_major_points(byte left, byte top, byte right, byte bottom)
    {
        var rect = new Rectangle(new Point(left, top), new Point(right, bottom));
        var expected = new List<Point>();
        for (var row = top; row <= bottom; row++)
        {
            for (var column = left; column <= right; column++)
            {
                expected.Add(new Point(column, row));
            }
        }
        Assert.Equal(expected, rect.ToList());
        var enumerable = (System.Collections.IEnumerable)rect;
        var points = new List<Point>();
        foreach (var point in enumerable)
        {
            points.Add((Point)point);
        }
        Assert.Equal(expected, points);
    }

    [Theory]
    [InlineData(4, 4)]
    [InlineData(2, 6)]
    [InlineData(6, 2)]
    [InlineData(1, 1)]
    public void GetInside_excludes_perimeter_and_handles_thin_rectangles(byte width, byte height)
    {
        var rect = new Rectangle(new Point(0, 0), width, height);
        var inside = new List<Point>();
        var enumerator = rect.GetInside();
        while (enumerator.MoveNext())
        {
            inside.Add(enumerator.Current);
        }
        var expected = new List<Point>();
        for (byte row = 1; row < height - 1; row++)
        {
            for (byte column = 1; column < width - 1; column++)
            {
                expected.Add(new Point(column, row));
            }
        }
        Assert.Equal(expected, inside);
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

    [Theory]
    [InlineData(5, 3)]
    [InlineData(3, 5)]
    [InlineData(1, 1)]
    public void GetPointIndex_round_trips_and_reuses_the_dimension_cache(byte width, byte height)
    {
        var (pointToIndex, indexToPoint) = Rectangle.GetPointIndex(width, height);
        var cached = Rectangle.GetPointIndex(width, height);
        Assert.Same(pointToIndex, cached.Item1);
        Assert.Same(indexToPoint, cached.Item2);
        Assert.Equal(width, pointToIndex.GetLength(0));
        Assert.Equal(height, pointToIndex.GetLength(1));
        Assert.Equal(width * height, indexToPoint.Length);

        for (byte y = 0; y < height; y++)
        {
            for (byte x = 0; x < width; x++)
            {
                Assert.Equal(x + y * width, pointToIndex[x, y]);
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
