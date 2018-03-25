using UnicornHack.Generation.Map;
using UnicornHack.Primitives;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Senses;
using UnicornHack.Utils.DataStructures;
using Xunit;

namespace UnicornHack.Utils
{
    // These tests assume observer is at 0,0
    public class BeveledFOVTest
    {
        [Fact]
        public void Pattern1() => TestFOV(@"
..#
..#
###", @"
..#
..#
###");

        [Fact]
        public void Pattern2() => TestFOV(@"
.##
..#
...#", @"
.#
..#
...#");

        [Fact]
        public void Pattern3() => TestFOV(@"
.###...
#......
...#...
..#....", @"
.#
#...
 ..#
 .#..");

        [Fact]
        public void Pattern4() => TestFOV(@"
A###
#...
...#
..#.", @"
A#
#..
 ..#
  #.");

        [Fact]
        public void Pattern5() => TestFOV(@"
.........#
.#.......#
#....#...#
.........#", @"
.........#
.#.......#
#. ..#...#
 ..  .  .#");

        [Fact]
        public void Pattern6() => TestFOV(@"
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

        [Fact]
        public void Pattern7() => TestFOV(@"
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

        [Fact]
        public void Pattern8() => TestFOV(@"
.........##############
########..............#
       ################", @"
.........#
########..............#
                     ##");

        [Fact]
        public void Pattern9() => TestFOV(@"
......
......

......", @"
......
......
######
 ");

        [Fact]
        public void Pattern10() => TestFOV(@"
......#..#
.........#
.........#
.......#.#
.........#
##########
..........", @"
......#
.........#
.........#
.......#.#
.........#
##########
 ");

        [Fact]
        public void Pattern11() => TestFOV(@"
.........#
.....#...#
.........#
.........#
.........#
..#..#....
#.........", @"
.........#
.....#...#
.........
.........#
.........#
..#..#....
#..... ...");

        [Fact]
        public void Pattern12() => TestFOV(@"
.........#
.#.......#
..#......#
...#.....#
....#....#
.....#...#
.........#", @"
.........#
.#.......#
.. ......#
...  ....#
...    ..#
....     #
....");

        [Fact]
        public void Pattern13() => TestFOV(@"
.........#
.........#
..#......#
...#.....#
....#....#
.....#...#
.........#", @"
.........#
.........#
..#......#
... .....#
....  ...#
....   ..#
.....    #");

        [Fact]
        public void Pattern14() => TestFOV(@"
.........#
.........#
.........#
...#.....#
....#....#
.....#...#
.........#", @"
.........#
.........#
.........#
...#.....#
.... ....#
..... ...#
......  .#");

        [Fact]
        public void Pattern15() => TestFOV(@"
.........#
.........#
.........#
.........#
....#....#
.....#...#
.........#", @"
.........#
.........#
.........#
.........#
....#....#
..... ...#
...... ..#");

        [Fact]
        public void Pattern16() => TestFOV(@"
..........######
#A#A#A#A#......#
################", @"
..........#
#A#A#A#A#......#
 ##");

        [Fact]
        public void Benchmark()
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

            for (var i = 0; i < 10000; i++)
            {
                GetVisibleTerrain(level, new Point(11, 10), Direction.South);
                GetVisibleTerrain(level, new Point(11, 11), Direction.South);
                GetVisibleTerrain(level, new Point(11, 12), Direction.South);
                GetVisibleTerrain(level, new Point(12, 12), Direction.East);
            }
        }

        private void TestFOV(string map, string expectedFOV)
        {
            var level = TestHelper.BuildLevel(map);
            var origin = new Point(0, 0);
            var heading = Direction.Southeast;
            var visibleTerrain = GetVisibleTerrain(level, origin, heading);

            var expectedFragment =
                new NormalMapFragment
                {
                    Map = expectedFOV,
                    Width = level.Width,
                    Height = level.Height
                };
            expectedFragment.EnsureInitialized(level.Game);

            var expectedVisibility = new byte[level.Height * level.Width];
            var visibilityMatched = true;

            expectedFragment.WriteMap(
                new Point(0, 0),
                level,
                (c, point, l, _) =>
                {
                    var expectedVisible = c == ' ' ? (byte)0 : (byte)1;
                    var i = l.PointToIndex[point.X, point.Y];
                    expectedVisibility[i] = expectedVisible;
                    var actualVisibile = visibleTerrain[i] == 0 ? 0 : 1;
                    visibilityMatched &= expectedVisible == actualVisibile;
                },
                (object)null);

            Assert.True(visibilityMatched, @"Expected:
" + TestHelper.PrintMap(level, expectedVisibility) + @"
Actual:
" + TestHelper.PrintMap(level, visibleTerrain) + @"
Seed: " + level.Game.InitialSeed);
        }

        private byte[] GetVisibleTerrain(LevelComponent level, Point origin, Direction heading)
        {
            var visibleTerrain = new byte[level.Height * level.Width];

            level.VisibilityCalculator.Compute(origin, heading,
                primaryFOVQuadrants: 8, primaryRange: 24, totalFOVQuadrants: 8, secondaryRange: 24,
                SensorySystem.TileBlocksVisibility,
                (level, visibleTerrain), noFalloff: true);
            return visibleTerrain;
        }
    }
}
