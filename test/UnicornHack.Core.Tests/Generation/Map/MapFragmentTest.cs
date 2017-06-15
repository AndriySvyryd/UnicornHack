using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using UnicornHack.Services;
using UnicornHack.Services.English;
using UnicornHack.Utils;
using Xunit;

namespace UnicornHack.Generation.Map
{
    public class MapFragmentTest
    {
        [Fact]
        public void BuildRoom_Point()
        {
            BuildRoomTest(@"
O");
        }

        [Fact]
        public void BuildRoom_Hairy_Point()
        {
            BuildRoomTest(@"
 O
OOO
 O");
        }

        [Fact]
        public void BuildRoom_Small_Square()
        {
            BuildRoomTest(@"
PP
PP");
        }

        [Fact]
        public void BuildRoom_Hairy_Small_Square()
        {
            BuildRoomTest(@"
OO O
 PPO
OPP
O OO");
        }

        [Fact]
        public void BuildRoom_Rectangle()
        {
            BuildRoomTest(@"
PPP
PIP
PPP");
        }

        [Fact]
        public void BuildRoom_Hairy_Rectangle()
        {
            BuildRoomTest(@"
 O O
OPPPO
 PIP
OPPPO
 O O");
        }

        [Fact]
        public void BuildRoom_Hollow_Rectangle()
        {
            BuildRoomTest(@"
PPP OOO
PIPOO O
PPP OOO");
        }

        [Fact]
        public void BuildRoom_I()
        {
            BuildRoomTest(@"
OOOOO
O O O
  O  X
O O O
OOOOO");
        }

        [Fact]
        public void BuildRoom_Ring()
        {
            BuildRoomTest(@"
OOO
O O
OOPP
  PP");
        }

        [Fact]
        public void BuildRoom_Thick_Rectangle()
        {
            BuildRoomTest(@"
PPPPPP
PXXXXP
PX  XP
PX  XP
PXXXXP
PPPPPP");
        }

        [Fact]
        public void BuildRoom_Touching_Squares()
        {
            BuildRoomTest(@"
PP X
PP
 PP
 PP");
        }

        private void BuildRoomTest(string map)
        {
            var level = CreateLevel();

            var fragment = new MapFragment
            {
                Map = map
            };
            fragment.EnsureInitialized(level.Game);

            var input = new List<Point>();
            var expectedInside = new List<Point>();
            var expectedPerimeter = new List<Point>();
            var expectedOutside = new List<Point>();

            var seed = Environment.TickCount;
            fragment.WriteMap(
                level.BoundingRectangle.PlaceInside(fragment.PayloadArea, new SimpleRandom {Seed = seed}).Value,
                level,
                (c, point, l, s) =>
                {
                    if (c != ' ')
                    {
                        input.Add(point);
                    }
                    switch (c)
                    {
                        case 'I':
                            s.Item1.Add(point);
                            break;
                        case 'P':
                            s.Item2.Add(point);
                            break;
                        case 'O':
                            s.Item3.Add(point);
                            break;
                    }
                },
                (expectedInside, expectedPerimeter, expectedOutside));

            var inside = new List<Point>();
            var perimeter = new List<Point>();
            var outside = new List<Point>();
            var room = fragment.BuildRoom(level, input, inside.Add, perimeter.Add, outside.Add);

            try
            {
                Assert.Equal(fragment.Width, room.BoundingRectangle.Width);
                Assert.Equal(fragment.Height, room.BoundingRectangle.Height);
                TestAssert.Equal(expectedInside, inside, "");
                TestAssert.Equal(expectedPerimeter, perimeter.Distinct(), "");
                TestAssert.Equal(expectedOutside, outside, "");

                Point? firstPoint = null;
                Point? lastPoint = null;
                foreach (var point in perimeter)
                {
                    if (firstPoint == null)
                    {
                        firstPoint = point;
                        lastPoint = point;
                    }
                    else
                    {
                        Assert.Equal(1, lastPoint.Value.OrthogonalDistanceTo(point));
                        lastPoint = point;
                    }
                }
                if (perimeter.Count > 1)
                {
                    Assert.Equal(1, firstPoint.Value.OrthogonalDistanceTo(lastPoint.Value));
                }

            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Seed: " + seed, e);
            }
        }

        private static Level CreateLevel()
        {
            var level = new Level
            {
                Height = 10,
                Width = 10,
                Game = new Game {Services = new GameServices(new EnglishLanguageService(), new MemoryCache(new MemoryCacheOptions()))}
            };
            level.Terrain = new byte[level.Height * level.Width];
            level.WallNeighbours = new byte[level.Height * level.Width];
            level.EnsureInitialized();

            return level;
        }
    }
}