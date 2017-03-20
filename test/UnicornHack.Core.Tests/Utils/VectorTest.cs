using Xunit;

namespace UnicornHack.Utils
{
    public class VectorTest
    {
        [Fact]
        public void GetInverse()
        {
            Assert.Equal(new Vector(-2, -3), new Vector(2, 3).GetInverse());
            Assert.Equal(new Vector(0, 0), new Vector(0, 0).GetInverse());
        }

        [Fact]
        public void GetOrthogonal()
        {
            Assert.Equal(new Vector(-3, 2), new Vector(2, 3).GetOrthogonal());
        }

        [Fact]
        public void GetUnit()
        {
            Assert.Equal(new Vector(1, 1), new Vector(2, 3).GetUnit());
            Assert.Equal(new Vector(0, 0), new Vector(0, 0).GetUnit());
        }
    }
}