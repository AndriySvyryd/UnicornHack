namespace UnicornHack.Utils.DataStructures;

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
        Assert.Equal(new Vector(0, 0), new Point(2, 3).DifferenceTo(new Point(2, 3)));
        Assert.Equal(new Vector(2, 3), new Point(0, 0).DifferenceTo(new Point(2, 3)));
        Assert.Equal(new Vector(-2, -3), new Point(2, 3).DifferenceTo(new Point(0, 0)));
        Assert.Equal(new Vector(0, 1), new Point(2, 2).DifferenceTo(new Point(2, 3)));
        Assert.Equal(new Vector(-1, 0), new Point(3, 3).DifferenceTo(new Point(2, 3)));
    }

    [Fact]
    public void Pack()
    {
        Assert.Equal(new Point(2, 3), Point.Unpack(new Point(2, 3).ToInt32()));
        Assert.Equal(new Point(255, 254), Point.Unpack(new Point(255, 254).ToInt32()));
        Assert.Equal(new Point(2, 3), Point.Unpack(new Point(2, 3).ToUInt16()));
        Assert.Equal(new Point(255, 254), Point.Unpack(new Point(255, 254).ToUInt16()));
    }
}
