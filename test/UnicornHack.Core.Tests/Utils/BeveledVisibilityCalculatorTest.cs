using System.Collections.Generic;
using System.Linq;
using UnicornHack.Primitives;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Senses;
using UnicornHack.Utils.DataStructures;
using Xunit;

namespace UnicornHack.Utils;

// Most of these tests assume observer is at 0,0
public class BeveledVisibilityCalculatorTest
{
    [Fact]
    public void FOV_Pattern1() => TestFOV(@"
..#
..#
##.", new byte[] { 254, 254, 254, 254, 254, 254, 254, 254, 158, });

    [Fact]
    public void FOV_Pattern2() => TestFOV(@"
.##
..#.
...#", new byte[] { 254, 254, 0, 0, 254, 222, 254, 0, 254, 254, 206, 254, });

    [Fact]
    public void FOV_Pattern3() => TestFOV(@"
.###...
#......
...#...
..#....",
        new byte[]
        {
            254, 254, 0, 0, 0, 0, 0, 254, 190, 127, 15, 0, 0, 0, 0, 127, 254, 254, 0, 0, 0, 0, 15, 254, 148, 51, 0, 0,
        });

    [Fact]
    public void FOV_Pattern4() => TestFOV(@"
A###
#...
...#
..#.", new byte[] { 254, 254, 0, 0, 254, 84, 47, 0, 0, 47, 242, 254, 0, 0, 254, 148, });

    [Fact]
    public void FOV_Pattern5() => TestFOV(@"
.........#
.#.......#
#....#...#
.........#",
        new byte[]
        {
            254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 127, 238, 254, 254, 254, 254, 254, 254, 254,
            111, 0, 15, 127, 254, 76, 152, 227, 254, 0, 174, 15, 0, 0, 0, 0, 0, 1, 254,
        });

    [Fact]
    public void FOV_Pattern6() => TestFOV(@"
........#
.#......#
......#.#
.#......#
........#",
        new byte[]
        {
            254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 127, 238, 254, 254, 254, 254, 254, 254, 127, 0, 15,
            127, 238, 254, 63, 254, 254, 254, 0, 0, 0, 15, 100, 132, 254, 254, 42, 0, 0, 0, 0, 0, 15, 254,
        });

    [Fact]
    public void FOV_Pattern7() => TestFOV(@"
......#..#
.........#
.#.......#
..#....#.#
.#.......#
..........",
        new byte[]
        {
            254, 254, 254, 254, 254, 254, 254, 0, 0, 0, 254, 254, 254, 254, 254, 254, 248, 232, 211, 254, 254, 254, 206,
            254, 254, 254, 254, 254, 254, 254, 254, 63, 254, 201, 254, 254, 254, 254, 90, 254, 254, 254, 0, 51, 244,
            254, 254, 222, 127, 254, 254, 31, 0, 0, 85, 252, 254, 254, 254, 238,
        });

    [Fact]
    public void FOV_Pattern8() => TestFOV(@"
.........##############
########..............#
       ################",
        new byte[]
        {
            254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 254, 254, 254, 254,
            254, 254, 254, 254, 18, 32, 39, 43, 47, 51, 56, 60, 64, 68, 72, 75, 79, 81, 254, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 254, 254,
        });

    [Fact]
    public void FOV_Pattern9() => TestFOV(@"
......
......

......",
        new byte[]
        {
            254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 0, 0, 0, 0, 0, 0,
        });

    [Fact]
    public void FOV_Pattern10() => TestFOV(@"
......#..#
.........#
.........#
.......#.#
.........#
##########
.....###..",
        new byte[]
        {
            254, 254, 254, 254, 254, 254, 254, 0, 0, 0, 254, 254, 254, 254, 254, 254, 248, 232, 211, 254, 254, 254, 254,
            254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 90, 254, 254, 254, 254, 254, 254,
            254, 254, 222, 127, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        });

    [Fact]
    public void FOV_Pattern11() => TestFOV(@"
.........#
.....#...#
.........#
.........#
.........#
..#..#....
#.........",
        new byte[]
        {
            254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 25, 50, 76, 254, 254, 254,
            254, 254, 254, 234, 177, 101, 26, 0, 254, 254, 254, 254, 254, 254, 254, 254, 252, 254, 254, 254, 254, 254,
            254, 254, 254, 254, 254, 254, 254, 254, 254, 222, 254, 254, 211, 254, 254, 222, 254, 248, 76, 127, 254, 211,
            34, 194, 254, 254,
        });

    [Fact]
    public void FOV_Pattern12() => TestFOV(@"
.........#
.#.......#
..#......#
...#.....#
....#....#
.....#...#
.........#",
        new byte[]
        {
            254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 127, 238, 254, 254, 254, 254, 254, 254, 254,
            127, 0, 15, 127, 238, 254, 254, 254, 254, 254, 238, 15, 0, 0, 15, 127, 238, 254, 254, 254, 254, 127, 0, 0,
            0, 0, 15, 127, 254, 254, 254, 238, 15, 0, 0, 0, 0, 0, 0, 254, 254, 254, 127, 0, 0, 0, 0, 0, 0,
        });

    [Fact]
    public void FOV_Pattern13() => TestFOV(@"
.........#
.........#
..#......#
...#.....#
....#....#
.....#...#
.........#",
        new byte[]
        {
            254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254,
            254, 254, 187, 254, 254, 254, 254, 254, 254, 254, 254, 187, 254, 127, 251, 254, 254, 254, 254, 254, 254,
            254, 127, 0, 66, 230, 254, 254, 254, 254, 254, 254, 251, 66, 0, 23, 187, 254, 254, 254, 254, 254, 254, 230,
            23, 0, 2, 127, 254,
        });

    [Fact]
    public void FOV_Pattern14() => TestFOV(@"
.........#
.........#
.........#
...#.....#
....#....#
.....#...#
.........#",
        new byte[]
        {
            254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254,
            254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 202, 254, 254, 254, 254, 254, 254, 254,
            254, 202, 254, 168, 254, 254, 254, 254, 254, 254, 254, 254, 168, 254, 127, 252, 254, 254, 254, 254, 254,
            254, 254, 127, 0, 85, 244, 254,
        });

    [Fact]
    public void FOV_Pattern15() => TestFOV(@"
.........#
.........#
.........#
.........#
....#....#
.....#...#
.........#",
        new byte[]
        {
            254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254,
            254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254,
            254, 254, 254, 208, 254, 254, 254, 254, 254, 254, 254, 254, 208, 254, 185, 254, 254, 254, 254, 254, 254,
            254, 254, 185, 10, 158, 254, 254,
        });

    [Fact]
    public void FOV_Pattern16() => TestFOV(@"
.#.......#
#.#......#
.#.#.....#
..#.#....#
...#.#...#
....#.#..#
.....#.#.#",
        new byte[]
        {
            254, 254, 0, 0, 0, 0, 0, 0, 0, 0, 254, 190, 254, 0, 0, 0, 0, 0, 0, 0, 0, 254, 158, 254, 0, 0, 0, 0, 0, 0, 0,
            0, 254, 148, 254, 0, 0, 0, 0, 0, 0, 0, 0, 254, 142, 254, 0, 0, 0, 0, 0, 0, 0, 0, 254, 138, 254, 0, 0, 0, 0,
            0, 0, 0, 0, 254, 136, 254, 0, 0,
        });

    [Fact]
    public void FOV_Pattern17() => TestFOV(@"
.#.......#
#.#......#
.#.#.....#
....#....#
.........#
....#....#
.....#...#",
        new byte[]
        {
            254, 254, 0, 0, 0, 0, 0, 0, 0, 0, 254, 190, 254, 0, 0, 0, 0, 0, 0, 0, 0, 254, 158, 254, 0, 0, 0, 0, 0, 0, 0,
            0, 66, 198, 254, 0, 0, 0, 0, 0, 0, 0, 0, 127, 198, 45, 0, 0, 0, 0, 0, 0, 0, 2, 254, 181, 68, 0, 0, 0, 0, 0,
            0, 0, 0, 254, 189, 95, 0, 0,
        });

    [Fact]
    public void FOV_Pattern18() => TestFOV(@"
..........######
#A#A#A#A#......#
################",
        new byte[]
        {
            254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 0, 0, 0, 0, 0, 254, 169, 254, 42, 254, 22, 254, 15,
            254, 15, 27, 34, 37, 40, 44, 254, 0, 254, 254, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        });

    [Fact]
    public void FOV_Pattern19() => TestFOV(@"
...#
.###
#A#", new byte[] { 254, 254, 254, 254, 254, 254, 254, 254, 254, 39, 0, 0, });

    [Fact]
    public void FOV_Pattern20() => TestFOV(@"
.......#...
.....#.#...
...........
.#.#.......
#........#.
..#..#.....
#..........",
        new byte[]
        {
            254, 254, 254, 254, 254, 254, 254, 254, 0, 0, 0, 254, 254, 254, 254, 254, 254, 25, 254, 0, 0, 0, 254, 254,
            254, 254, 254, 234, 177, 101, 26, 0, 0, 254, 254, 222, 254, 202, 254, 254, 254, 252, 203, 127, 254, 34, 127,
            202, 18, 168, 254, 254, 254, 254, 98, 0, 52, 254, 238, 168, 254, 127, 252, 254, 222, 127, 0, 63, 0, 127,
            254, 127, 0, 85, 244, 254, 254,
        });

    [Fact]
    public void FOV_Falloff() => TestFOV(@"
.......#
.......#
.......#
.......#
.......#
.......#
.......#",
        new byte[]
        {
            254, 238, 223, 207, 192, 176, 161, 145, 238, 244, 234, 224, 214, 204, 194, 184, 223, 234, 234, 224, 214,
            204, 194, 184, 207, 224, 224, 224, 214, 204, 194, 184, 192, 214, 214, 214, 214, 204, 194, 184, 176, 204,
            204, 204, 204, 204, 194, 184, 161, 194, 194, 194, 194, 194, 194, 184,
        }, noFalloff: false);

    [Fact]
    public void LOS_Pattern1_right() => TestLOS(@"
..?
..#
###", new byte[] { 0, 168, 254, 0, 0, 254, 0, 0, 0, });

    [Fact]
    public void LOS_Pattern1_bottom() => TestLOS(@"
..#
..#
?##", new byte[] { 0, 0, 0, 168, 0, 0, 254, 254, 0, });

    [Fact]
    public void LOS_Pattern1_diagonal() => TestLOS(@"
..#
..#
##?", new byte[] { 0, 8, 0, 8, 186, 254, 0, 254, 158, });

    [Fact]
    public void LOS_Pattern2_diagonal() => TestLOS(@"
.##
..#.
..?#", new byte[] { 0, 254, 0, 0, 8, 186, 254, 0, 0, 76, 206, 0, });

    [Fact]
    public void LOS_Pattern3_other() => TestLOS(@"
.###...
#..?...
...#...
..#....", new byte[] { 0, 254, 0, 0, 0, 0, 0, 0, 1, 50, 15, 0, 0, 0, 0, 0, 0, 254, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, });

    [Fact]
    public void LOS_Pattern4_diagonal() => TestLOS(@"
A###
#...
...#
..#?", new byte[] { 0, 254, 0, 0, 254, 136, 32, 0, 0, 32, 224, 254, 0, 0, 254, 148, });

    [Fact]
    public void LOS_Pattern5_bottom() => TestLOS(@"
.........#
.#.......#
#....#...#
?........#",
        new byte[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 254, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0,
        });

    [Fact]
    public void LOS_Pattern5_left() => TestLOS(@"
?........#
.#.......#
#....#...#
.........#",
        new byte[]
        {
            254, 236, 202, 168, 134, 100, 66, 32, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        },
        new Point(8, 0));

    [Fact]
    public void LOS_Pattern5_other() => TestLOS(@"
.........#
.#.......#
#....#...#
.....?...#",
        new byte[]
        {
            0, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 254, 23, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 12, 46, 254, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0,
        });

    [Fact]
    public void LOS_Pattern6_other() => TestLOS(@"
........#
.#......#
......#.#
.#......#
.?......#",
        new byte[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 73, 254, 0, 0, 0, 0, 0, 0, 0, 70, 90, 0, 0, 0, 0, 0, 0, 0, 42, 254, 0, 0, 0, 0,
            0, 0, 0, 14, 42, 0, 0, 0, 0, 0, 0, 0,
        });

    [Fact]
    public void LOS_Pattern6_top() => TestLOS(@"
........#
?#......#
......#.#
.#......#
........#",
        new byte[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 254, 254, 0, 0, 0, 0, 0, 0, 0, 202, 0, 0, 0, 0, 0, 0, 0, 0, 100, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        },
        new Point(0, 4));

    [Fact]
    public void LOS_Pattern6_top_right() => TestLOS(@"
........#
.#.?....#
......#.#
.#......#
........#",
        new byte[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 254, 0, 0, 0, 0, 0,
            0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0,
        },
        new Point(0, 4));

    [Fact]
    public void LOS_Pattern16_diagonal() => TestLOS(@"
.#.......#
#.#......#
.#.#.....#
..#.#....#
...#.#...#
....#.#..#
.....#?#.#",
        new byte[]
        {
            0, 254, 0, 0, 0, 0, 0, 0, 0, 0, 254, 76, 254, 0, 0, 0, 0, 0, 0, 0, 0, 254, 140, 254, 0, 0, 0, 0, 0, 0, 0, 0,
            254, 190, 254, 0, 0, 0, 0, 0, 0, 0, 0, 254, 142, 254, 0, 0, 0, 0, 0, 0, 0, 0, 254, 138, 254, 0, 0, 0, 0, 0,
            0, 0, 0, 254, 136, 0, 0, 0,
        });

    [Fact]
    public void LOS_Pattern17_diagonal() => TestLOS(@"
.#.......#
#.#......#
.#.#.....#
....#....#
.........#
....#....#
.....#?..#",
        new byte[]
        {
            0, 254, 0, 0, 0, 0, 0, 0, 0, 0, 254, 76, 254, 0, 0, 0, 0, 0, 0, 0, 0, 254, 140, 254, 0, 0, 0, 0, 0, 0, 0, 0,
            22, 190, 254, 0, 0, 0, 0, 0, 0, 0, 0, 43, 183, 45, 0, 0, 0, 0, 0, 0, 0, 0, 254, 181, 68, 0, 0, 0, 0, 0, 0,
            0, 0, 254, 189, 0, 0, 0,
        });

    private static void TestFOV(string map, byte[] expectedVisibility, bool noFalloff = true)
    {
        var level = TestHelper.BuildLevel(map);
        var visibleTerrain = GetVisibleTerrain(
            level, origin: new Point(0, 0), heading: Direction.Southeast, new byte[level.TileCount], noFalloff);

        TestHelper.AssertVisibility(level, expectedVisibility, visibleTerrain);
    }

    public static byte[] GetVisibleTerrain(
        LevelComponent level, Point origin, Direction heading, byte[] visibleTerrain, bool noFalloff = true)
    {
        level.VisibilityCalculator.ComputeDirected(
            origin, heading, primaryFOVQuadrants: 1, primaryRange: 24, totalFOVQuadrants: 2, secondaryRange: 12,
            noFalloff, SensorySystem.TileBlocksVisibility, visibleTerrain);
        return visibleTerrain;
    }

    public static byte[] GetVisibleTerrain(LevelComponent level, Point origin, byte[] visibleTerrain,
        bool noFalloff = true)
    {
        level.VisibilityCalculator.ComputeOmnidirectional(
            origin, range: 24, noFalloff, SensorySystem.TileBlocksVisibility, visibleTerrain);
        return visibleTerrain;
    }

    private static void TestLOS(string map, byte[] expectedVisibility, Point? origin = null)
    {
        var (level, rooms) = TestHelper.BuildLevelWithRooms(map);
        var target = rooms.Single().DoorwayPoints.Single();
        origin ??= new Point(0, 0);
        var visibleTerrainList = GetLOS(level, target, origin.Value);

        var visibleTerrain = new byte[level.TileCount];
        var originDistance = 0;
        foreach (var (point, visibility) in visibleTerrainList)
        {
            visibleTerrain[level.PointToIndex[point.X, point.Y]] += visibility;
            var distance = origin.Value.DistanceTo(point);
            Assert.False(distance < originDistance, $"Point {point} is out of sequence");

            originDistance = distance;
        }

        TestHelper.AssertVisibility(level, expectedVisibility, visibleTerrain);
    }

    public static List<(Point, byte)> GetLOS(LevelComponent level, Point target, Point origin)
        => level.VisibilityCalculator.ComputeLOS(origin, target, SensorySystem.TileBlocksVisibility,
            level.Entity.Manager);
}
