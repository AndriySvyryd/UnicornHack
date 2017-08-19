using System;
using UnicornHack.Generation.Map;
using Xunit;

namespace UnicornHack.Utils
{
    // These tests assume observer is at 0,0
    public class BeveledFOVTest
    {
        [Fact]
        public void Pattern1()
        {
            TestFOV(@"
..#
..#
###", @"
..#
..#
###");
        }

        [Fact]
        public void Pattern2()
        {
            TestFOV(@"
.##
..#
...#", @"
.#
..#
...#");
        }

        [Fact]
        public void Pattern3()
        {
            TestFOV(@"
.###...
#......
...#...
..#....", @"
.#
#...
 ..#
 .#..");
        }

        [Fact]
        public void Pattern4()
        {
            TestFOV(@"
A###
#...
...#
..#.", @"
A#
#..
 ..#
  #.");
        }

        [Fact]
        public void Pattern5()
        {
            TestFOV(@"
.........#
.#.......#
#....#...#
.........#", @"
.........#
.#.......#
#. ..#...#
 ..  .  .#");
        }

        [Fact]
        public void Pattern6()
        {
            TestFOV(@"
........#
.#......#
......#.#
.#......#
........#", @"
........#
.#......#
.. ...#.#
.#.  ...#
..     .#");
        }

        [Fact]
        public void Pattern7()
        {
            TestFOV(@"
......#..#
.........#
.#.......#
..#....#.#
.#.......#
..........", @"
......#
.........#
.#.......#
..#....#.#
.# ......#
..  ......");
        }

        [Fact]
        public void Pattern8()
        {
            TestFOV(@"
.........##############
########..............#
       ################", @"
.........#
########..............#
                     ##");
        }

        [Fact]
        public void Pattern9()
        {
            TestFOV(@"
......
......

......", @"
......
......
......
");
        }

        [Fact]
        public void Benchmark()
        {
            var map = @"
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

            var seed = Environment.TickCount;
            var level = TestHelper.BuildLevel(map, seed);

            for (var i = 0; i < 10000; i++)
            {
                level.RecomputeVisibility(new Point(11, 10), visibilityFalloff: 1);
                level.RecomputeVisibility(new Point(11, 11), visibilityFalloff: 1);
                level.RecomputeVisibility(new Point(11, 12), visibilityFalloff: 1);
                level.RecomputeVisibility(new Point(12, 12), visibilityFalloff: 1);
            }
        }

        private void TestFOV(string map, string expectedFOV)
        {
            var seed = Environment.TickCount;
            var level = TestHelper.BuildLevel(map, seed);

            level.RecomputeVisibility(new Point(0, 0), visibilityFalloff: 0);

            var expectedFragment = new NormalMapFragment {Map = expectedFOV};
            expectedFragment.EnsureInitialized(level.Game);

            var expectedVisibility = new byte[level.Height * level.Width];
            var visibilityMatched = true;

            expectedFragment.WriteMap(
                new Point(0, 0),
                level,
                (c, point, l, v) =>
                {
                    var expectedVisible = c == ' ' ? (byte)0 : (byte)1;
                    var i = l.PointToIndex[point.X, point.Y];
                    expectedVisibility[i] = expectedVisible;
                    var actualVisibile = l.VisibleTerrain[i] == 0 ? 0 : 1;
                    visibilityMatched &= expectedVisible == actualVisibile;
                },
                (object)null);

            Assert.True(visibilityMatched, @"Expected:
" + TestHelper.PrintMap(level, expectedVisibility) + @"
Actual:
" + TestHelper.PrintMap(level) + @"
Seed: " + seed);
        }
    }
}