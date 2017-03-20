using Xunit;

namespace UnicornHack.Utils
{
    public class RectangleTest
    {
        [Fact]
        public void Properties()
        {
            Assert.Equal(new Segment(0, 4), new Rectangle(new Point(0, 0), new Point(4, 4)).XProjection);
            Assert.Equal(new Segment(1, 7), new Rectangle(new Point(6, 1), new Point(8, 7)).YProjection);
            Assert.Equal(1, new Rectangle(new Point(6, 1), 1, 2).Width);
            Assert.Equal(2, new Rectangle(new Point(6, 1), 1, 2).Height);
            Assert.Equal(1, new Rectangle(new Point(6, 7), new Point(6, 7)).Width);
            Assert.Equal(1, new Rectangle(new Point(6, 7), new Point(6, 7)).Height);
            Assert.Equal(6, new Rectangle(new Point(0, 2), new Point(2, 3)).Area);
        }

        [Fact]
        public void Intersection()
        {
            Assert.Equal(new Rectangle(new Point(3, 3), new Point(4, 4)),
                new Rectangle(new Point(0, 0), new Point(4, 4))
                    .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
            Assert.Equal(new Rectangle(new Point(6, 3), new Point(7, 7)),
                new Rectangle(new Point(6, 1), new Point(8, 7))
                    .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
            Assert.Equal(new Rectangle(new Point(6, 7), new Point(7, 7)),
                new Rectangle(new Point(6, 7), new Point(10, 9))
                    .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
            Assert.Equal(new Rectangle(new Point(3, 3), new Point(3, 7)),
                new Rectangle(new Point(1, 2), new Point(3, 10))
                    .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
            Assert.Equal(new Rectangle(new Point(4, 4), new Point(6, 6)),
                new Rectangle(new Point(4, 4), new Point(6, 6))
                    .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
            Assert.Equal(new Rectangle(new Point(5, 5), new Point(5, 5)),
                new Rectangle(new Point(5, 5), new Point(5, 5))
                    .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
            Assert.Equal(new Rectangle(new Point(3, 3), new Point(7, 7)),
                new Rectangle(new Point(0, 0), new Point(10, 10))
                    .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
            Assert.Equal(new Rectangle(new Point(3, 5), new Point(7, 6)),
                new Rectangle(new Point(0, 5), new Point(9, 6))
                    .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
            Assert.Equal(new Rectangle(new Point(3, 7), new Point(7, 7)),
                new Rectangle(new Point(2, 7), new Point(10, 9))
                    .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
            Assert.Equal(new Rectangle(new Point(4, 3), new Point(5, 7)),
                new Rectangle(new Point(4, 1), new Point(5, 8))
                    .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
            Assert.Equal(new Rectangle(new Point(3, 3), new Point(7, 7)),
                new Rectangle(new Point(2, 3), new Point(8, 7))
                    .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
            Assert.Equal(new Rectangle(new Point(3, 3), new Point(7, 7)),
                new Rectangle(new Point(3, 0), new Point(7, 10))
                    .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
            Assert.Equal(null,
                new Rectangle(new Point(1, 1), new Point(8, 2))
                    .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
            Assert.Equal(null,
                new Rectangle(new Point(1, 4), new Point(2, 6))
                    .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
            Assert.Equal(null,
                new Rectangle(new Point(8, 2), new Point(10, 9))
                    .Intersection(new Rectangle(new Point(3, 3), new Point(7, 7))));
        }

        [Fact]
        public void Perimeter()
        {
            Assert.Equal(new[]
                {
                    new Point(1, 2), new Point(2, 2), new Point(3, 2), new Point(3, 3),
                    new Point(3, 4), new Point(2, 4), new Point(1, 4), new Point(1, 3)
                },
                new Rectangle(new Point(1, 2), new Point(3, 4)).GetPerimeter());

            Assert.Equal(new[] {new Point(0, 2), new Point(0, 3), new Point(0, 4)},
                new Rectangle(new Point(0, 2), new Point(0, 4)).GetPerimeter());

            Assert.Equal(new[] {new Point(1, 0), new Point(2, 0), new Point(3, 0)},
                new Rectangle(new Point(1, 0), new Point(3, 0)).GetPerimeter());

            Assert.Equal(new[] {new Point(1, 1)},
                new Rectangle(new Point(1, 1), new Point(1, 1)).GetPerimeter());
        }
    }
}