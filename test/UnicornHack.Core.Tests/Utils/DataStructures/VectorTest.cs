namespace UnicornHack.Utils.DataStructures;

public class VectorTest
{
    [Fact]
    public void GetInverse()
    {
        Assert.Equal(new Vector(-2, -3), new Vector(2, 3).GetInverse());
        Assert.Equal(new Vector(0, 0), new Vector(0, 0).GetInverse());
    }

    [Fact]
    public void GetOrthogonal() => Assert.Equal(new Vector(-3, 2), new Vector(2, 3).GetOrthogonal());

    [Fact]
    public void GetUnit()
    {
        Assert.Equal(new Vector(1, 1), new Vector(2, 3).GetUnit());
        Assert.Equal(new Vector(0, 0), new Vector(0, 0).GetUnit());
        Assert.Equal(new Vector(-1, 1), new Vector(-2, 3).GetUnit());
        Assert.Equal(new Vector(-1, -1), new Vector(-1, -1).GetUnit());
    }

    [Fact]
    public void GetOctant()
    {
        Assert.Equal(0, new Vector(1, 0).GetOctant());
        Assert.Equal(4, new Vector(-1, 0).GetOctant());
        Assert.Equal(2, new Vector(0, -1).GetOctant());
        Assert.Equal(6, new Vector(0, 1).GetOctant());
        Assert.Equal(1, new Vector(1, -1).GetOctant());
        Assert.Equal(3, new Vector(-1, -1).GetOctant());
        Assert.Equal(4, new Vector(-10, 1).GetOctant());
        Assert.Equal(3, new Vector(-10, -1).GetOctant());
        Assert.Equal(7, new Vector(9, 1).GetOctant());
        Assert.Equal(3, new Vector(-8, -7).GetOctant());
        Assert.Equal(1, new Vector(5, -6).GetOctant());
        Assert.Equal(6, new Vector(3, 4).GetOctant());
        Assert.Equal(1, new Vector(2, -3).GetOctant());
        Assert.Equal(3, new Vector(-2, -1).GetOctant());
    }

    [Fact]
    public void OctantsTo()
    {
        Assert.Equal(0, new Vector(1, 0).OctantsTo(Direction.East));
        Assert.Equal(0, new Vector(1, 1).OctantsTo(Direction.Southeast));
        Assert.Equal(0, new Vector(0, 1).OctantsTo(Direction.South));
        Assert.Equal(0, new Vector(-1, 1).OctantsTo(Direction.Southwest));
        Assert.Equal(0, new Vector(-1, 0).OctantsTo(Direction.West));
        Assert.Equal(0, new Vector(-1, -1).OctantsTo(Direction.Northwest));
        Assert.Equal(0, new Vector(0, -1).OctantsTo(Direction.North));
        Assert.Equal(0, new Vector(1, -1).OctantsTo(Direction.Northeast));

        Assert.Equal(1, new Vector(1, 0).OctantsTo(Direction.Northeast));
        Assert.Equal(2, new Vector(-1, 0).OctantsTo(Direction.South));
        Assert.Equal(3, new Vector(0, -1).OctantsTo(Direction.Southwest));
        Assert.Equal(4, new Vector(1, 0).OctantsTo(Direction.West));
        Assert.Equal(-3, new Vector(0, 1).OctantsTo(Direction.Northwest));
        Assert.Equal(-2, new Vector(1, -1).OctantsTo(Direction.Southeast));
        Assert.Equal(-1, new Vector(-1, -1).OctantsTo(Direction.North));

        Assert.Equal(-1, new Vector(-2, -1).OctantsTo(Direction.North));
        Assert.Equal(-2, new Vector(-2, -1).OctantsTo(Direction.Northeast));
        Assert.Equal(-3, new Vector(-2, -1).OctantsTo(Direction.East));
        Assert.Equal(3, new Vector(-2, -1).OctantsTo(Direction.Southeast));
        Assert.Equal(2, new Vector(-2, -1).OctantsTo(Direction.South));
        Assert.Equal(1, new Vector(-2, -1).OctantsTo(Direction.Southwest));
        Assert.Equal(0, new Vector(-2, -1).OctantsTo(Direction.West));
        Assert.Equal(0, new Vector(-2, -1).OctantsTo(Direction.Northwest));

        Assert.Equal(0, new Vector(-10, 1).OctantsTo(Direction.West));
        Assert.Equal(0, new Vector(-10, -1).OctantsTo(Direction.West));
        Assert.Equal(0, new Vector(-2, -1).OctantsTo(Direction.West));
        Assert.Equal(0, new Vector(-2, -1).OctantsTo(Direction.Northwest));
        Assert.Equal(1, new Vector(9, 1).OctantsTo(Direction.Northeast));
        Assert.Equal(-1, new Vector(-1, -9).OctantsTo(Direction.Northeast));
        Assert.Equal(3, new Vector(-7, -8).OctantsTo(Direction.South));
        Assert.Equal(3, new Vector(-8, -7).OctantsTo(Direction.South));
        Assert.Equal(4, new Vector(5, -6).OctantsTo(Direction.Southwest));
        Assert.Equal(4, new Vector(4, -6).OctantsTo(Direction.Southwest));
        Assert.Equal(-4, new Vector(3, 4).OctantsTo(Direction.Northwest));
        Assert.Equal(-2, new Vector(2, -3).OctantsTo(Direction.Southeast));
    }
}
