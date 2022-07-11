namespace UnicornHack.Utils.DataStructures;

public class YBoundedIntervalTreeTest
{
    [Fact]
    public void Simple()
    {
        var boundingRectangle = new Rectangle(new Point(0, 0), new Point(10, 10));
        var initialRectangles = new[]
        {
            new Rectangle(new Point(0, 0), new Point(4, 4)), new Rectangle(new Point(6, 1), new Point(8, 7)),
            new Rectangle(new Point(6, 7), new Point(10, 9)), new Rectangle(new Point(1, 2), new Point(3, 10)),
            new Rectangle(new Point(4, 4), new Point(6, 6)), new Rectangle(new Point(5, 5), new Point(5, 5)),
            new Rectangle(new Point(0, 0), new Point(10, 10)), new Rectangle(new Point(0, 5), new Point(9, 6)),
            new Rectangle(new Point(2, 7), new Point(10, 9)), new Rectangle(new Point(4, 1), new Point(5, 8)),
            new Rectangle(new Point(2, 3), new Point(8, 7)), new Rectangle(new Point(3, 0), new Point(7, 10)),
            new Rectangle(new Point(1, 1), new Point(8, 2)), new Rectangle(new Point(1, 4), new Point(2, 6)),
            new Rectangle(new Point(8, 2), new Point(10, 9))
        };

        var targetRectangle = new Rectangle(new Point(3, 3), new Point(7, 7));
        Test(initialRectangles, boundingRectangle, targetRectangle);
    }

    [Fact]
    public void Random()
    {
        var boundingRectangle = new Rectangle(new Point(0, 0), new Point(10, 10));

        var randomCount = 20;
        var initialRectangles = new List<Rectangle>(randomCount);
        var seed = (uint)Environment.TickCount;
        var random = new SimpleRandom { Seed = seed };
        for (var i = 0; i < randomCount; i++)
        {
            initialRectangles.Add(Rectangle.CreateRandom(random, boundingRectangle));
        }

        var targetRectangle = Rectangle.CreateRandom(random, boundingRectangle);
        try
        {
            Test(initialRectangles, boundingRectangle, targetRectangle);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Seed: " + seed, e);
        }
    }

    private static void Test(
        IReadOnlyList<Rectangle> initialRectangles, Rectangle boundingRectangle, Rectangle targetRectangle)
    {
        var expectedRectangles = new List<Rectangle>();
        var otherRectangles = new List<Rectangle>();
        var tree = new YBoundedIntervalTree(boundingRectangle.XProjection);
        foreach (var initialRectangle in initialRectangles)
        {
            Assert.True(tree.Insert(initialRectangle));
            if (initialRectangle.XProjection.Encloses(targetRectangle.XProjection)
                && (targetRectangle.YProjection.Contains(initialRectangle.YProjection.Beginning)
                    || targetRectangle.YProjection.Contains(initialRectangle.YProjection.End)))
            {
                expectedRectangles.Add(initialRectangle);
            }
            else
            {
                otherRectangles.Add(initialRectangle);
            }
        }

        TestAssert.Equal(expectedRectangles, tree.GetEnclosing(targetRectangle), $"Target: {targetRectangle}");

        foreach (var rectangle in otherRectangles)
        {
            Assert.True(tree.Remove(rectangle));
        }

        TestAssert.Equal(expectedRectangles, tree.GetEnclosing(targetRectangle), $"Target: {targetRectangle}");

        foreach (var rectangle in expectedRectangles)
        {
            Assert.True(tree.Remove(rectangle));
        }

        Assert.Empty(tree.GetEnclosing(targetRectangle));

        tree.InsertRange(expectedRectangles);
        tree.InsertRange(otherRectangles);

        TestAssert.Equal(expectedRectangles, tree.GetEnclosing(targetRectangle), $"Target: {targetRectangle}");
    }
}
