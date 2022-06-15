using System;
using BenchmarkDotNet.Attributes;
using UnicornHack.Primitives;
using UnicornHack.Utils;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.PerformanceTests.Utils;

public class BeveledVisibilityCalculatorPerfTest
{
    private const int Iterations = 10000;

    [Benchmark]
    public void Mixed_terrain_omnidirectional_FOV()
    {
        const string map = @"
     #############
     #...#...#...#
     #..#.....#..#
     #.#...#...#.#
   ####.........####
  ##.A.....#.....A.##
 ##.###.........##..##
##.###.#...#...#.##..##
#.## #..#.....#..###..#
.##  #...#.#.#...# ##..
##   #....#.#....#  ##.
###########A###########
.......................
........#.#.#.#.#.#.#..
.......................
........#.#.#.#.#.#.#..
..#####................
........#.#.#.#.#.#.#..
.#.....#...............
.#.....##.#.#.#.#.#.#..
.#.....#...............
........#.#.#.#.#.#.#..
..#####................";

        var level = TestHelper.BuildLevel(map);

        var visibleTerrain = new byte[level.TileCount];
        for (var i = 0; i < Iterations; i++)
        {
            Array.Clear(visibleTerrain, 0, visibleTerrain.Length);
            BeveledVisibilityCalculatorTest.GetVisibleTerrain(level, new Point(11, 10), visibleTerrain,
                noFalloff: false);

            Array.Clear(visibleTerrain, 0, visibleTerrain.Length);
            BeveledVisibilityCalculatorTest.GetVisibleTerrain(level, new Point(11, 11), visibleTerrain,
                noFalloff: false);

            Array.Clear(visibleTerrain, 0, visibleTerrain.Length);
            BeveledVisibilityCalculatorTest.GetVisibleTerrain(level, new Point(11, 12), visibleTerrain,
                noFalloff: false);

            Array.Clear(visibleTerrain, 0, visibleTerrain.Length);
            BeveledVisibilityCalculatorTest.GetVisibleTerrain(level, new Point(12, 12), visibleTerrain,
                noFalloff: false);
        }
    }

    [Benchmark]
    public void Mixed_terrain_directional_FOV()
    {
        const string map = @"
     #############
     #...#...#...#
     #..#.....#..#
     #.#...#...#.#
   ####.........####
  ##.A.....#.....A.##
 ##.###.........##..##
##.###.#...#...#.##..##
#.## #..#.....#..###..#
.##  #...#.#.#...# ##..
##   #....#.#....#  ##.
###########A###########
.......................
........#.#.#.#.#.#.#..
.......................
........#.#.#.#.#.#.#..
..#####................
........#.#.#.#.#.#.#..
.#.....#...............
.#.....##.#.#.#.#.#.#..
.#.....#...............
........#.#.#.#.#.#.#..
..#####................";

        var level = TestHelper.BuildLevel(map);

        var visibleTerrain = new byte[level.TileCount];
        for (var i = 0; i < Iterations * 2; i++)
        {
            Array.Clear(visibleTerrain, 0, visibleTerrain.Length);
            BeveledVisibilityCalculatorTest.GetVisibleTerrain(level, new Point(11, 10), Direction.South, visibleTerrain,
                noFalloff: false);

            Array.Clear(visibleTerrain, 0, visibleTerrain.Length);
            BeveledVisibilityCalculatorTest.GetVisibleTerrain(level, new Point(11, 11), Direction.South, visibleTerrain,
                noFalloff: false);

            Array.Clear(visibleTerrain, 0, visibleTerrain.Length);
            BeveledVisibilityCalculatorTest.GetVisibleTerrain(level, new Point(11, 12), Direction.South, visibleTerrain,
                noFalloff: false);

            Array.Clear(visibleTerrain, 0, visibleTerrain.Length);
            BeveledVisibilityCalculatorTest.GetVisibleTerrain(level, new Point(12, 12), Direction.East, visibleTerrain,
                noFalloff: false);
        }
    }

    [Benchmark]
    public void Mixed_terrain_LOS()
    {
        const string map = @"
     #############
     #...#...#...#
     #..#.....#..#
     #.#...#...#.#
   ####.........####
  ##.A.....#.....A.##
 ##.###.........##..##
##.###.#...#...#.##..##
#.## #..#.....#..###..#
.##  #...#.#.#...# ##..
##   #....#.#....#  ##.
###########A###########
.......................
........#.#.#.#.#.#.#..
.......................
........#.#.#.#.#.#.#..
..#####................
........#.#.#.#.#.#.#..
.#.....#...............
.#.....##.#.#.#.#.#.#..
.#.....#...............
........#.#.#.#.#.#.#..
..#####................";

        var level = TestHelper.BuildLevel(map);
        var pool = level.Entity.Manager.PointByteListArrayPool;

        for (var i = 0; i < Iterations << 3; i++)
        {
            var visibleTerrain = BeveledVisibilityCalculatorTest.GetLOS(level, new Point(19, 19), new Point(11, 11));
            visibleTerrain.Clear();
            pool.Return(visibleTerrain);

            visibleTerrain = BeveledVisibilityCalculatorTest.GetLOS(level, new Point(11, 19), new Point(11, 11));
            visibleTerrain.Clear();
            pool.Return(visibleTerrain);

            visibleTerrain = BeveledVisibilityCalculatorTest.GetLOS(level, new Point(3, 14), new Point(11, 11));
            visibleTerrain.Clear();
            pool.Return(visibleTerrain);

            visibleTerrain = BeveledVisibilityCalculatorTest.GetLOS(level, new Point(10, 8), new Point(11, 11));
            visibleTerrain.Clear();
            pool.Return(visibleTerrain);
        }
    }
}
