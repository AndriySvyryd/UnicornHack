using System;
using BenchmarkDotNet.Attributes;
using UnicornHack.Primitives;
using UnicornHack.Utils;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.PerformanceTests.Utils
{
    public class BeveledFOVPerfTest
    {
        public int Iterations = 10000;

        [Benchmark]
        public void Mixed_terrain()
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
                BeveledFOVTest.GetVisibleTerrain(level, new Point(11, 10), Direction.South, visibleTerrain);

                Array.Clear(visibleTerrain, 0, visibleTerrain.Length);
                BeveledFOVTest.GetVisibleTerrain(level, new Point(11, 11), Direction.South, visibleTerrain);

                Array.Clear(visibleTerrain, 0, visibleTerrain.Length);
                BeveledFOVTest.GetVisibleTerrain(level, new Point(11, 12), Direction.South, visibleTerrain);

                Array.Clear(visibleTerrain, 0, visibleTerrain.Length);
                BeveledFOVTest.GetVisibleTerrain(level, new Point(12, 12), Direction.East, visibleTerrain);
            }
        }
    }
}
