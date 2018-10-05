using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using UnicornHack.Primitives;
using UnicornHack.Utils;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.PerformanceTests.Utils
{
    public class BeveledVisibilityCalculatorPerfTest
    {
        public int Iterations = 10000;

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

            var visibleTerrain = new byte[level.Height * level.Width];
            for (var i = 0; i < Iterations; i++)
            {
                Array.Clear(visibleTerrain, 0, visibleTerrain.Length);
                BeveledVisibilityCalculatorTest.GetVisibleTerrain(level, new Point(11, 10), visibleTerrain);

                Array.Clear(visibleTerrain, 0, visibleTerrain.Length);
                BeveledVisibilityCalculatorTest.GetVisibleTerrain(level, new Point(11, 11), visibleTerrain);

                Array.Clear(visibleTerrain, 0, visibleTerrain.Length);
                BeveledVisibilityCalculatorTest.GetVisibleTerrain(level, new Point(11, 12), visibleTerrain);

                Array.Clear(visibleTerrain, 0, visibleTerrain.Length);
                BeveledVisibilityCalculatorTest.GetVisibleTerrain(level, new Point(12, 12), visibleTerrain);
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

            var visibleTerrain = new byte[level.Height * level.Width];
            for (var i = 0; i < Iterations; i++)
            {
                Array.Clear(visibleTerrain, 0, visibleTerrain.Length);
                BeveledVisibilityCalculatorTest.GetVisibleTerrain(level, new Point(11, 10), Direction.South, visibleTerrain);

                Array.Clear(visibleTerrain, 0, visibleTerrain.Length);
                BeveledVisibilityCalculatorTest.GetVisibleTerrain(level, new Point(11, 11), Direction.South, visibleTerrain);

                Array.Clear(visibleTerrain, 0, visibleTerrain.Length);
                BeveledVisibilityCalculatorTest.GetVisibleTerrain(level, new Point(11, 12), Direction.South, visibleTerrain);

                Array.Clear(visibleTerrain, 0, visibleTerrain.Length);
                BeveledVisibilityCalculatorTest.GetVisibleTerrain(level, new Point(12, 12), Direction.East, visibleTerrain);
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

            var visibleTerrain = new List<(int, byte)>();
            for (var i = 0; i < Iterations << 3; i++)
            {
                visibleTerrain.Clear();
                BeveledVisibilityCalculatorTest.GetLOS(level, new Point(19, 19), new Point(11, 11), visibleTerrain);

                visibleTerrain.Clear();
                BeveledVisibilityCalculatorTest.GetLOS(level, new Point(11, 19), new Point(11, 11), visibleTerrain);

                visibleTerrain.Clear();
                BeveledVisibilityCalculatorTest.GetLOS(level, new Point(3, 14), new Point(11, 11), visibleTerrain);

                visibleTerrain.Clear();
                BeveledVisibilityCalculatorTest.GetLOS(level, new Point(10, 8), new Point(11, 11), visibleTerrain);
            }
        }
    }
}
