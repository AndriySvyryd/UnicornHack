using System.Text;
using UnicornHack.Systems.Levels;

namespace UnicornHack;

public static class TestAssert
{
    public static void Equal(IEnumerable<Point> expected, IEnumerable<Point> actual, string message)
    {
        var sortedExpected = expected.OrderBy(r => r.X).ThenBy(r => r.Y).ToList();
        var sortedActual = actual.OrderBy(r => r.X).ThenBy(r => r.Y).ToList();
        EqualSorted(sortedExpected, sortedActual, message);
    }

    public static void Equal(IEnumerable<Rectangle> expected, IEnumerable<Rectangle> actual, string message)
    {
        var sortedExpected = expected.OrderBy(r => r.TopLeft.X)
            .ThenBy(r => r.TopLeft.Y)
            .ThenBy(r => r.BottomRight.X)
            .ThenBy(r => r.BottomRight.Y)
            .ToList();
        var sortedActual = actual.OrderBy(r => r.TopLeft.X)
            .ThenBy(r => r.TopLeft.Y)
            .ThenBy(r => r.BottomRight.X)
            .ThenBy(r => r.BottomRight.Y)
            .ToList();

        EqualSorted(sortedExpected, sortedActual, message);
    }

    private static void EqualSorted<T>(IReadOnlyList<T> expected, IReadOnlyList<T> actual, string message)
    {
        Assert.True(expected.SequenceEqual(actual),
            "Expected:" + Environment.NewLine
                        + Format(expected)
                        + "Actual:" + Environment.NewLine
                        + Format(actual)
                        + message);
    }

    private static string Format<T>(IEnumerable<T> values)
    {
        var sb = new StringBuilder();
        foreach (var value in values)
        {
            sb.Append(value).AppendLine(", ");
        }

        return sb.ToString();
    }
    public static void AssertVisibility(RoomComponent level, string expectedVisibleMap, byte[] actualVisibility)
    {
        var expectedFragment = new TestNormalMapFragment { Map = expectedVisibleMap };
        expectedFragment.SetDimensions(level.Width, level.Height);
        expectedFragment.EnsureInitialized();

        var expectedVisibility = new byte[level.TileCount];

        Point? mismatchedPoint = null;
        expectedFragment.WriteMap(
            new Point(0, 0),
            level,
            (c, point, l, _) =>
            {
                var expectedVisible = c == ' ' ? (byte)0 : (byte)255;
                var i = l.PointToIndex![point.X, point.Y];
                expectedVisibility[i] = expectedVisible;
                var actualVisible = actualVisibility[i] == 0 ? 0 : 255;
                if (expectedVisible != actualVisible
                    && mismatchedPoint == null)
                {
                    mismatchedPoint = point;
                }
            },
            (object?)null);

        if (mismatchedPoint.HasValue)
        {
            Assert.Fail($"Mismatch at ({mismatchedPoint.Value.X}, {mismatchedPoint.Value.Y})" + @"
Expected map:
" + PrintMap(level, expectedVisibility) + @"
Actual map:
" + PrintMap(level, actualVisibility) + @"
Seed: " + level.Game.Options.InitialSeed);
        }
    }

    public static void AssertVisibility(RoomComponent level, byte[] expectedVisibility, byte[] actualVisibility)
    {
        for (var i = 0; i < actualVisibility.Length; i++)
        {
            if (actualVisibility[i] == expectedVisibility[i])
            {
                continue;
            }

            var point = level.IndexToPoint![i];
            Assert.Fail(
                $"Mismatch at ({point.X}, {point.Y}) expected {expectedVisibility[i]}, got {actualVisibility[i]}" +
                @"
Expected map:
" + PrintMap(level, expectedVisibility) + @"
Actual map:
" + PrintMap(level, actualVisibility) +
                PrintVisibility(level, actualVisibility) + @"
Seed: " + level.Game.Options.InitialSeed);
        }
    }

    public static void AssertTerrain(RoomComponent level, string expectedMap, MapFeature[] actualTerrain)
    {
        var expectedFragment = new TestNormalMapFragment { Map = expectedMap };
        expectedFragment.SetDimensions(level.Width, level.Height);
        expectedFragment.EnsureInitialized();

        var expected = new MapFeature[level.TileCount];
        var matched = true;

        expectedFragment.WriteMap(
            new Point(0, 0),
            level,
            (c, point, l, _) =>
            {
                var expectedFeature = MapFragment.GlyphToFeature(c);
                var i = l.PointToIndex![point.X, point.Y];
                expected[i] = expectedFeature;
                if (expectedFeature != actualTerrain[i]
                    && (expectedFeature == MapFeature.Default
                        || expectedFeature == MapFeature.Unexplored)
                    && !(actualTerrain[i] == MapFeature.Default
                         || actualTerrain[i] == MapFeature.Unexplored))
                {
                    matched = false;
                }
            },
            (object?)null);

        Assert.True(matched, @"Expected:
" + PrintMap(level, null, expected) + @"
Actual:
" + PrintMap(level, null, actualTerrain) + @"
Seed: " + level.Game.Options.InitialSeed);
    }

    public static string PrintMap(RoomComponent level, byte[]? visibleTerrain, MapFeature[]? terrain = null)
    {
        terrain ??= level.Terrain;
        var builder = new StringBuilder();
        var i = 0;
        for (var y = 0; y < level.Height; y++)
        {
            for (var x = 0; x < level.Width; x++)
            {
                if (visibleTerrain != null && visibleTerrain[i] == 0)
                {
                    builder.Append(' ');
                }
                else
                {
                    var feature = terrain[i];
                    var symbol = ' ';
                    switch (feature)
                    {
                        case MapFeature.Default:
                        case MapFeature.Unexplored:
                            break;
                        case MapFeature.StoneFloor:
                            symbol = '·';
                            break;
                        case MapFeature.RockFloor:
                            symbol = '█';
                            break;
                        case MapFeature.StoneWall:
                            var neighbors = level.Neighbors[i] & DirectionFlags.Cross;
                            switch (neighbors)
                            {
                                case DirectionFlags.None:
                                    symbol = '●';
                                    break;
                                case DirectionFlags.North:
                                    symbol = '╹';
                                    break;
                                case DirectionFlags.East:
                                    symbol = '╺';
                                    break;
                                case DirectionFlags.NorthAndEast:
                                    symbol = '┗';
                                    break;
                                case DirectionFlags.South:
                                    symbol = '╻';
                                    break;
                                case DirectionFlags.Longitudinal:
                                    symbol = '┃';
                                    break;
                                case DirectionFlags.SouthAndEast:
                                    symbol = '┏';
                                    break;
                                case DirectionFlags.NorthEastSouth:
                                    symbol = '┣';
                                    break;
                                case DirectionFlags.West:
                                    symbol = '╸';
                                    break;
                                case DirectionFlags.NorthAndWest:
                                    symbol = '┛';
                                    break;
                                case DirectionFlags.Latitudinal:
                                    symbol = '━';
                                    break;
                                case DirectionFlags.NorthEastWest:
                                    symbol = '┻';
                                    break;
                                case DirectionFlags.SouthAndWest:
                                    symbol = '┓';
                                    break;
                                case DirectionFlags.NorthWestSouth:
                                    symbol = '┫';
                                    break;
                                case DirectionFlags.SouthEastWest:
                                    symbol = '┳';
                                    break;
                                case DirectionFlags.Cross:
                                    symbol = '╋';
                                    break;
                                default:
                                    throw new InvalidOperationException("Invalid wall neighbors: " + neighbors);
                            }

                            break;
                        case MapFeature.StoneArchway:
                            symbol = '∩';
                            break;
                        case MapFeature.Pool:
                            symbol = '≈';
                            break;
                        default:
                            throw new NotSupportedException($"Map feature {feature} not supported.");
                    }

                    builder.Append(symbol);
                }

                i++;
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }

    public static string PrintVisibility(RoomComponent level, byte[] visibleTerrain)
    {
        var builder = new StringBuilder();
        var i = 0;
        for (var y = 0; y < level.Height; y++)
        {
            for (var x = 0; x < level.Width; x++)
            {
                builder.Append(visibleTerrain[i].ToString().PadLeft(4));
                builder.Append(',');

                i++;
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }

}
