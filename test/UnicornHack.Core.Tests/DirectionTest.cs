using UnicornHack.Primitives;
using Xunit;

namespace UnicornHack
{
    public class DirectionTest
    {
        [Fact]
        public void OctantsTo()
        {
            Assert.Equal(0, Direction.Northeast.OctantsTo(Direction.Northeast));
            Assert.Equal(7, Direction.Northeast.OctantsTo(Direction.East));
            Assert.Equal(1, Direction.East.OctantsTo(Direction.Northeast));
            Assert.Equal(6, Direction.North.OctantsTo(Direction.East));
            Assert.Equal(2, Direction.West.OctantsTo(Direction.South));
            Assert.Equal(4, Direction.Northeast.OctantsTo(Direction.Southwest));
            Assert.Equal(4, Direction.Southwest.OctantsTo(Direction.Northeast));
        }

        [Fact]
        public void ClosestOctantsTo()
        {
            Assert.Equal(0, Direction.Northeast.ClosestOctantsTo(Direction.Northeast));
            Assert.Equal(1, Direction.Northeast.ClosestOctantsTo(Direction.East));
            Assert.Equal(1, Direction.East.ClosestOctantsTo(Direction.Northeast));
            Assert.Equal(2, Direction.North.ClosestOctantsTo(Direction.East));
            Assert.Equal(2, Direction.West.ClosestOctantsTo(Direction.South));
            Assert.Equal(4, Direction.Northeast.ClosestOctantsTo(Direction.Southwest));
            Assert.Equal(4, Direction.Southwest.ClosestOctantsTo(Direction.Northeast));
        }
    }
}
