namespace UnicornHack.Utils.DataStructures;

public class RectangleIntervalTreeTest
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
        var seed = 1u;
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

    private static void Test(IReadOnlyList<Rectangle> initialRectangles, Rectangle boundingRectangle,
        Rectangle targetRectangle)
    {
        var expectedRectangles = new List<Rectangle>();
        var otherRectangles = new List<Rectangle>();
        var tree = new RectangleIntervalTree(boundingRectangle);
        foreach (var initialRectangle in initialRectangles)
        {
            Assert.True(tree.Insert(initialRectangle));
            if (initialRectangle.Overlaps(targetRectangle))
            {
                expectedRectangles.Add(initialRectangle);
            }
            else
            {
                otherRectangles.Add(initialRectangle);
            }
        }

        TestAssert.Equal(expectedRectangles, tree.GetOverlapping(targetRectangle), $"Target: {targetRectangle}");

        foreach (var rectangle in otherRectangles)
        {
            Assert.True(tree.Remove(rectangle));
        }

        TestAssert.Equal(expectedRectangles, tree.GetOverlapping(targetRectangle), $"Target: {targetRectangle}");

        foreach (var rectangle in expectedRectangles)
        {
            Assert.True(tree.Remove(rectangle));
        }

        Assert.Empty(tree.GetOverlapping(targetRectangle));

        tree.InsertRange(expectedRectangles);
        tree.InsertRange(otherRectangles);

        TestAssert.Equal(expectedRectangles, tree.GetOverlapping(targetRectangle), $"Target: {targetRectangle}");
    }

    /// <summary>
    ///     The dense room-layout shift-to-merge step inserts a chunk whose bounding rectangle
    ///     overlaps a previously placed chunk's by a single shared wall column. Insertion of
    ///     such a rectangle must succeed and both rectangles must remain queryable.
    /// </summary>
    [Fact]
    public void Inserts_rectangles_that_share_a_single_column_with_a_placed_neighbor()
    {
        var bounds = new Rectangle(new Point(0, 0), new Point(10, 10));
        var tree = new RectangleIntervalTree(bounds);

        var anchor = new Rectangle(new Point(0, 0), new Point(4, 4));
        var shifted = new Rectangle(new Point(4, 0), new Point(8, 4));

        Assert.True(tree.Insert(anchor));
        Assert.True(tree.Insert(shifted));

        var queryColumn = new Rectangle(new Point(4, 0), new Point(4, 4));
        var overlapping = tree.GetOverlapping(queryColumn).ToList();
        Assert.Contains(anchor, overlapping);
        Assert.Contains(shifted, overlapping);

        Assert.True(tree.Remove(anchor));
        Assert.True(tree.Remove(shifted));
        Assert.Empty(tree.GetOverlapping(queryColumn));
    }

    /// <summary>
    ///     A rectangle fully enclosed by a previously inserted one still inserts and is
    ///     reported alongside the enclosing rectangle by <see cref="RectangleIntervalTree.GetOverlapping" />.
    /// </summary>
    [Fact]
    public void Inserts_a_rectangle_fully_enclosed_by_a_placed_neighbor()
    {
        var bounds = new Rectangle(new Point(0, 0), new Point(10, 10));
        var tree = new RectangleIntervalTree(bounds);

        var outer = new Rectangle(new Point(0, 0), new Point(8, 8));
        var inner = new Rectangle(new Point(2, 2), new Point(6, 6));

        Assert.True(tree.Insert(outer));
        Assert.True(tree.Insert(inner));

        var probe = new Rectangle(new Point(3, 3), new Point(4, 4));
        var overlapping = tree.GetOverlapping(probe).ToList();
        Assert.Contains(outer, overlapping);
        Assert.Contains(inner, overlapping);
    }
}
