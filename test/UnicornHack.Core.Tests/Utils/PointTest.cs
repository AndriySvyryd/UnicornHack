using Xunit;

namespace UnicornHack.Utils
{
    public class PointTest
    {
        [Fact]
        public void Distance()
        {
            Assert.Equal(0, new Point(2, 3).DistanceTo(new Point(2, 3)));
            Assert.Equal(3, new Point(0, 0).DistanceTo(new Point(2, 3)));
            Assert.Equal(3, new Point(2, 3).DistanceTo(new Point(0, 0)));
            Assert.Equal(1, new Point(2, 2).DistanceTo(new Point(2, 3)));
            Assert.Equal(1, new Point(3, 3).DistanceTo(new Point(2, 3)));
        }

        [Fact]
        public void Direction()
        {
            Assert.Equal(new Vector(0, 0), new Point(2, 3).DirectionTo(new Point(2, 3)));
            Assert.Equal(new Vector(2, 3), new Point(0, 0).DirectionTo(new Point(2, 3)));
            Assert.Equal(new Vector(-2, -3), new Point(2, 3).DirectionTo(new Point(0, 0)));
            Assert.Equal(new Vector(0, 1), new Point(2, 2).DirectionTo(new Point(2, 3)));
            Assert.Equal(new Vector(-1, 0), new Point(3, 3).DirectionTo(new Point(2, 3)));
        }
    }
}