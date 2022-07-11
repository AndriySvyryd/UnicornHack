namespace UnicornHack.Generation;

public class DefiningMapFragmentTest
{
    [Fact]
    public void ConstantWeight()
    {
        var fragment = new DefiningMapFragment { GenerationWeight = "0.5" };

        Assert.Equal(0.5, fragment.GetWeight("foo", 2, 0, 0));
    }

    [Fact]
    public void InfiniteWeight()
    {
        var fragment = new DefiningMapFragment { GenerationWeight = "Infinity" };

        Assert.True(float.IsPositiveInfinity(fragment.GetWeight("foo", 2, 0, 0)));
    }

    [Fact]
    public void BranchWeight()
    {
        var fragment = new DefiningMapFragment { GenerationWeight = "$branch == 'foo' ? 1 : 0" };

        Assert.Equal(0, fragment.GetWeight("bar", 2, 0, 0));
        Assert.Equal(1, fragment.GetWeight("foo", 1, 0, 0));

        fragment.GenerationWeight = "$depth >= 2 && $depth <= 3 ? 1 : 0";

        Assert.Equal(0, fragment.GetWeight("foo", 1, 0, 0));
        Assert.Equal(0, fragment.GetWeight("bar", 4, 0, 0));
        Assert.Equal(1, fragment.GetWeight("bar", 2, 0, 0));
        Assert.Equal(1, fragment.GetWeight("foo", 3, 0, 0));
    }

    [Fact]
    public void InstancesWeight()
    {
        var fragment = new DefiningMapFragment { GenerationWeight = "Max(3 - $instances, 0)" };

        Assert.Equal(3, fragment.GetWeight("foo", 2, 0, 0));
        Assert.Equal(1, fragment.GetWeight("foo", 2, 2, 0));
        Assert.Equal(0, fragment.GetWeight("foo", 2, 4, 0));
    }

    [Fact]
    public void TagInstancesWeight()
    {
        var fragment = new DefiningMapFragment { GenerationWeight = "Min($tagInstances / 2.0, 3)" };

        Assert.Equal(0, fragment.GetWeight("foo", 2, 0, 0));
        Assert.Equal(0.5, fragment.GetWeight("foo", 2, 0, 1));
        Assert.Equal(3, fragment.GetWeight("foo", 2, 0, 10));
    }
}
