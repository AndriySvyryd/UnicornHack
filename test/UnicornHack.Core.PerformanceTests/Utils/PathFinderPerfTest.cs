
using BenchmarkDotNet.Attributes;
using UnicornHack.Primitives;
using UnicornHack.Utils;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.PerformanceTests.Utils
{
    public class PathFinderPerfTest
    {
        public int Iterations = 10000;

        //[Benchmark]
        public void Multiple_paths()
        {
            var (level, target) = PathFinderTest.TestPath(@"
*##..............#...#.#.*.#..#.#.#.#.#.#...
.*############.#.#.#.#.#*#*#.#.#.#.#.#.#....
##*..........#.#.#.#.#.#*#*##.############..
.*############.#.#.#.#.#*#*#.##.#...#...#...
*##............#.#.#.#.*.#.*....#.#.#.#.#*..
*...############.#.#.#*#.#.#*##...#.#.#.#*..
*####..............#.#*#.#.##*##..#...#.#*..
*.....##############.#*#.#.#.#*########.*...
*######..............#*#...#..#*#*#*#*#*....
.*********************.#...#...#*#*#*#*#####",
                Direction.North);

            var travelSystem = level.Entity.Manager.TravelSystem;
            for (var i = 0; i < Iterations; i++)
            {
                travelSystem.GetShortestPath(level, new Point(0, 0), target, Direction.North);
            }
        }
    }
}
