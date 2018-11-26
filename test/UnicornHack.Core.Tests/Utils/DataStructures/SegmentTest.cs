using Xunit;

namespace UnicornHack.Utils.DataStructures
{
    public class SegmentTest
    {
        [Fact]
        public void Length()
        {
            Assert.Equal(1, new Segment(0, 0).Length);
            Assert.Equal(2, new Segment(2, 3).Length);
        }

        [Fact]
        public void MidPoint()
        {
            Assert.Equal(0, new Segment(0, 0).MidPoint);
            Assert.Equal(2, new Segment(2, 3).MidPoint);
            Assert.Equal(3, new Segment(2, 4).MidPoint);
        }

        [Fact]
        public void Contains()
        {
            Assert.True(new Segment(0, 0).Contains(new Segment(0, 0)));
            Assert.False(new Segment(2, 3).Contains(new Segment(0, 1)));
            Assert.False(new Segment(2, 4).Contains(new Segment(5, 6)));
            Assert.False(new Segment(2, 3).Contains(new Segment(0, 2)));
            Assert.False(new Segment(2, 4).Contains(new Segment(4, 6)));
            Assert.True(new Segment(1, 4).Contains(new Segment(2, 3)));
            Assert.True(new Segment(2, 6).Contains(new Segment(3, 3)));
        }

        [Fact]
        public void Intersection()
        {
            Assert.Equal(new Segment(0, 0), new Segment(0, 0).Intersection(new Segment(0, 0)));
            Assert.Null(new Segment(2, 3).Intersection(new Segment(0, 1)));
            Assert.Null(new Segment(2, 4).Intersection(new Segment(5, 6)));
            Assert.Equal(new Segment(2, 2), new Segment(2, 3).Intersection(new Segment(0, 2)));
            Assert.Equal(new Segment(4, 4), new Segment(2, 4).Intersection(new Segment(4, 6)));
            Assert.Equal(new Segment(2, 3), new Segment(1, 4).Intersection(new Segment(2, 3)));
            Assert.Equal(new Segment(3, 3), new Segment(2, 6).Intersection(new Segment(3, 3)));
        }
    }
}
