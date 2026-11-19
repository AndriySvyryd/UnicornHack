namespace UnicornHack.Generation.Map;

public class MapFragmentTransformsTest
{
    [Theory]
    [InlineData(false, false, false, 8)]
    [InlineData(true, false, false, 4)]
    [InlineData(false, true, false, 2)]
    [InlineData(true, true, false, 1)]
    [InlineData(false, false, true, 1)]
    [InlineData(true, false, true, 1)]
    [InlineData(false, true, true, 1)]
    [InlineData(true, true, true, 1)]
    public void GetOrbit_respects_flags_order_and_unique_membership(
        bool noMirror, bool noTurnOver, bool noTransform, int expectedSize)
    {
        var orbit = MapFragmentTransforms.GetOrbit(noMirror, noTurnOver, noTransform);

        Assert.Equal(expectedSize, orbit.Length);
        Assert.Equal(MapFragmentTransform.Identity, orbit[0]);
        Assert.Equal(orbit.Length, orbit.ToArray().Distinct().Count());
        if (!noMirror && !noTurnOver && !noTransform)
        {
            Assert.Equal(Enum.GetValues<MapFragmentTransform>().ToHashSet(), orbit.ToArray().ToHashSet());
        }
        if (noMirror && !noTurnOver && !noTransform)
        {
            Assert.Equal(new[]
            {
                MapFragmentTransform.Identity, MapFragmentTransform.Rotate90,
                MapFragmentTransform.Rotate180, MapFragmentTransform.Rotate270
            }, orbit.ToArray());
        }
    }

    [Theory]
    [InlineData(5, 3)]
    [InlineData(3, 5)]
    [InlineData(1, 7)]
    [InlineData(4, 4)]
    public void GetTransformedSize_matches_axis_mapping_for_every_transform(byte width, byte height)
    {
        foreach (var transform in Enum.GetValues<MapFragmentTransform>())
        {
            var swapsAxes = transform is MapFragmentTransform.Rotate90 or MapFragmentTransform.Rotate270
                or MapFragmentTransform.FlipDiagonal or MapFragmentTransform.FlipAntiDiagonal;
            Assert.Equal(swapsAxes ? (height, width) : (width, height), transform.GetTransformedSize(width, height));
        }
    }

    [Fact]
    public void Identity_returns_same_instance()
    {
        var fragment = Initialize(@"
##.
#..
...");

        var variant = fragment.CreateVariant(MapFragmentTransform.Identity);

        Assert.Same(fragment, variant);
    }

    [Fact]
    public void Rotate90_swaps_dimensions_and_rotates_glyphs_clockwise()
    {
        var fragment = Initialize(@"
ABC
DEF");

        var variant = fragment.CreateVariant(MapFragmentTransform.Rotate90);

        Assert.Equal(2, variant.Width);
        Assert.Equal(3, variant.Height);
        // Source row 0 ABC becomes the rightmost column top-to-bottom: A at (1,0), B at (1,1), C at (1,2)
        AssertGlyphAt(variant, 1, 0, 'A');
        AssertGlyphAt(variant, 1, 1, 'B');
        AssertGlyphAt(variant, 1, 2, 'C');
        // Source row 1 DEF becomes the leftmost column
        AssertGlyphAt(variant, 0, 0, 'D');
        AssertGlyphAt(variant, 0, 1, 'E');
        AssertGlyphAt(variant, 0, 2, 'F');
    }

    [Fact]
    public void Four_rotations_round_trip_to_identity()
    {
        var fragment = Initialize(@"
ABCD
EFGH
IJKL");

        var rotated = fragment
            .CreateVariant(MapFragmentTransform.Rotate90)
            .CreateVariant(MapFragmentTransform.Rotate90)
            .CreateVariant(MapFragmentTransform.Rotate90)
            .CreateVariant(MapFragmentTransform.Rotate90);

        AssertGeometryEqual(fragment, rotated);
    }

    [Fact]
    public void Rotate180_is_self_inverse()
    {
        var fragment = Initialize(@"
~SFU~
.....
~,#A~");

        var doubled = fragment
            .CreateVariant(MapFragmentTransform.Rotate180)
            .CreateVariant(MapFragmentTransform.Rotate180);

        AssertGeometryEqual(fragment, doubled);
    }

    [Fact]
    public void FlipHorizontal_is_self_inverse()
    {
        var fragment = Initialize(@"
~SFU~
.....
~,#A~");

        var doubled = fragment
            .CreateVariant(MapFragmentTransform.FlipHorizontal)
            .CreateVariant(MapFragmentTransform.FlipHorizontal);

        AssertGeometryEqual(fragment, doubled);
    }

    [Fact]
    public void Rotate90_rotates_conditional_flags_and_edges()
    {
        var fragment = Initialize(@"
~SFU.~
......
~....~");

        Assert.True(fragment.ConditionalFirstRow);
        Assert.True(fragment.ConditionalLastRow);
        Assert.True(fragment.ConditionalFirstColumn);
        Assert.True(fragment.ConditionalLastColumn);
        var sourceNorth = fragment.NorthEdge!;

        var variant = fragment.CreateVariant(MapFragmentTransform.Rotate90);

        // All four conditional flags remain set (rotation permutes them but they were all true).
        Assert.True(variant.ConditionalFirstRow);
        Assert.True(variant.ConditionalLastRow);
        Assert.True(variant.ConditionalFirstColumn);
        Assert.True(variant.ConditionalLastColumn);

        // Per the D4 edge table, the source north edge lands on the new east edge unreversed.
        Assert.NotNull(variant.EastEdge);
        Assert.Equal(sourceNorth.Length, variant.EastEdge!.Length);
        for (var i = 0; i < sourceNorth.Length; i++)
        {
            Assert.Equal(sourceNorth[i], variant.EastEdge[i]);
        }
    }

    [Fact]
    public void FlipHorizontal_reverses_north_edge()
    {
        var fragment = Initialize(@"
~SFU.~
......
~....~");

        var sourceNorth = fragment.NorthEdge!;

        var variant = fragment.CreateVariant(MapFragmentTransform.FlipHorizontal);

        Assert.NotNull(variant.NorthEdge);
        Assert.Equal(sourceNorth.Length, variant.NorthEdge!.Length);
        for (var i = 0; i < sourceNorth.Length; i++)
        {
            Assert.Equal(sourceNorth[i], variant.NorthEdge[sourceNorth.Length - 1 - i]);
        }
    }

    [Fact]
    public void Variant_records_applied_transform_and_unique_name()
    {
        var fragment = Initialize(@"
##
..");

        var variant = fragment.CreateVariant(MapFragmentTransform.Rotate90);

        Assert.Equal(MapFragmentTransform.Rotate90, variant.Transform);
        Assert.NotEqual(fragment.Name, variant.Name);
        Assert.EndsWith("Rotate90", variant.Name);
    }

    [Fact]
    public void PossibleDoorwayCount_is_preserved_across_rotation()
    {
        var fragment = Initialize(@"
.+.
...");

        Assert.Equal(1, fragment.PossibleDoorwayCount);

        var variant = fragment.CreateVariant(MapFragmentTransform.Rotate90);

        Assert.Equal(fragment.PossibleDoorwayCount, variant.PossibleDoorwayCount);
    }

    private static void AssertGlyphAt(MapFragment fragment, int x, int y, char expected)
    {
        var actual = (char)fragment.ByteMap![fragment.PointToIndex![x, y]];
        Assert.Equal(expected, actual);
    }

    private static void AssertGeometryEqual(MapFragment expected, MapFragment actual)
    {
        Assert.Equal(expected.Width, actual.Width);
        Assert.Equal(expected.Height, actual.Height);
        Assert.Equal(expected.PayloadArea, actual.PayloadArea);
        Assert.Equal(expected.ConditionalFirstRow, actual.ConditionalFirstRow);
        Assert.Equal(expected.ConditionalLastRow, actual.ConditionalLastRow);
        Assert.Equal(expected.ConditionalFirstColumn, actual.ConditionalFirstColumn);
        Assert.Equal(expected.ConditionalLastColumn, actual.ConditionalLastColumn);

        Assert.NotNull(actual.ByteMap);
        Assert.Equal(expected.ByteMap!.Length, actual.ByteMap!.Length);
        for (byte y = 0; y < expected.Height; y++)
        {
            for (byte x = 0; x < expected.Width; x++)
            {
                Assert.Equal(
                    expected.ByteMap[expected.PointToIndex![x, y]],
                    actual.ByteMap[actual.PointToIndex![x, y]]);
            }
        }

        AssertEdgeEqual(expected.NorthEdge, actual.NorthEdge);
        AssertEdgeEqual(expected.SouthEdge, actual.SouthEdge);
        AssertEdgeEqual(expected.WestEdge, actual.WestEdge);
        AssertEdgeEqual(expected.EastEdge, actual.EastEdge);
    }

    private static void AssertEdgeEqual(EdgeConditionKind[]? expected, EdgeConditionKind[]? actual)
    {
        if (expected is null)
        {
            Assert.Null(actual);
            return;
        }

        Assert.NotNull(actual);
        Assert.Equal(expected.Length, actual!.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], actual[i]);
        }
    }

    private static TestDefiningMapFragment Initialize(string map)
    {
        var fragment = new TestDefiningMapFragment
        {
            Name = "TestFragment",
            Map = map,
            NoRandomDoorways = true
        };
        fragment.EnsureInitialized();
        return fragment;
    }
}
