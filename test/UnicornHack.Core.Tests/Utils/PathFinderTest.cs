using System;
using System.Collections.Generic;
using System.Linq;
using UnicornHack.Generation.Map;
using Xunit;

namespace UnicornHack.Utils
{
    // * - expected path
    public class PathFinderTest
    {
        [Fact]
        public void Pattern1()
        {
            TestPath(@"
**.
##*
**.",
                Direction.North);
        }

        [Fact]
        public void Pattern2()
        {
            TestPath(@"
**.
..*",
                Direction.North);
        }

        [Fact]
        public void Pattern3()
        {
            TestPath(@"
*..
.**",
                Direction.South);
        }

        [Fact]
        public void Pattern4()
        {
            TestPath(@"
*.#
#*##
.#*#
.#*#
#*##.
#.*.#*
...**",
                Direction.North);
        }

        [Fact]
        public void Benchmark()
        {
            var (level, target) = TestPath(@"
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

            for (int i = 0; i < 10000; i++)
            {
                level.GetShortestPath(new Point(0, 0), target, Direction.North);
            }
        }

        private (Level, Point) TestPath(string expectedPathMap, Direction initialDirection)
        {
            var seed = Environment.TickCount;
            var map = expectedPathMap.Replace('*', '.');
            var level = TestHelper.BuildLevel(map, seed);

            var expectedFragment = new MapFragment {Map = expectedPathMap};
            expectedFragment.EnsureInitialized(level.Game);
            var expectedPathArray = new byte[level.Height * level.Width];
            expectedFragment.WriteMap(
                new Point(0, 0),
                level,
                (c, point, l, v) =>
                {
                    expectedPathArray[l.PointToIndex[point.X, point.Y]] = c == '*' ? (byte)1 : (byte)0;
                },
                (object)null);

            var expectedPath = ExtractPath(level, expectedPathArray, 1);
            var actualPath = level.GetShortestPath(new Point(0, 0), expectedPath.Last(), initialDirection);
            actualPath.Reverse();

            var actualPathArray = new byte[level.Height * level.Width];
            actualPathArray[0] = 1;
            var pathMatches = true;
            for (var i = 0; i < actualPath.Count; i++)
            {
                var point = actualPath[i];
                if (i < expectedPath.Count)
                {
                    pathMatches = pathMatches && point.Equals(expectedPath[i]);
                }
                actualPathArray[level.PointToIndex[point.X, point.Y]] = 1;
            }

            Assert.True(pathMatches, @"Expected:
" + TestHelper.PrintMap(level, expectedPathArray) + @"
Actual:
" + TestHelper.PrintMap(level, actualPathArray) + @"
Seed: " + seed);

            return (level, expectedPath.Last());
        }

        private List<Point> ExtractPath(Level level, byte[] expectedPathArray, byte pathValue)
        {
            var expectedPath = new List<Point>();
            var currentPathPoint = new Point(0, 0);
            var previousPointDirectionIndex = (byte)Direction.North;
            while (previousPointDirectionIndex != byte.MaxValue)
            {
                for (var directionIndex = 0; directionIndex < 8; directionIndex++)
                {
                    if (directionIndex == previousPointDirectionIndex)
                    {
                        continue;
                    }

                    var direction = Level.MovementDirections[directionIndex];
                    var newLocationX = (byte)(currentPathPoint.X + direction.X);
                    var newLocationY = (byte)(currentPathPoint.Y + direction.Y);

                    if (newLocationX >= level.Width || newLocationY >= level.Height)
                    {
                        continue;
                    }

                    var newLocationIndex = level.PointToIndex[newLocationX, newLocationY];
                    if (expectedPathArray[newLocationIndex] != pathValue)
                    {
                        continue;
                    }

                    currentPathPoint = new Point(newLocationX, newLocationY);
                    expectedPath.Add(currentPathPoint);
                    previousPointDirectionIndex = Level.OppositeDirectionIndexes[directionIndex];
                    goto NextPointFound;
                }

                previousPointDirectionIndex = byte.MaxValue;
                NextPointFound:
                ;
            }
            return expectedPath;
        }
    }
}