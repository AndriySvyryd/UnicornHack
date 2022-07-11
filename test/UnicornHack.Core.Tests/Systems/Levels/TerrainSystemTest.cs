namespace UnicornHack.Systems.Levels;

public class TerrainSystemTest
{
    [Fact]
    public void GetAdjacentPoints_returns_valid_points()
    {
        var level = TestHelper.BuildLevel(@"
.#.
...
...");

        Assert.Equal(
            new[]
            {
                new Point(2, 2), new Point(2, 1), new Point(2, 0), new Point(1, 0), new Point(0, 0),
                new Point(0, 1), new Point(0, 2), new Point(1, 2)
            },
            TerrainSystem.GetAdjacentPoints(level, new Point(1, 1), Direction.Southeast));

        Assert.Equal(
            new[] { new Point(0, 0), new Point(0, 1), new Point(1, 1), new Point(1, 0) },
            TerrainSystem.GetAdjacentPoints(level, new Point(0, 0), Direction.South, includeInitial: true));
    }
}
