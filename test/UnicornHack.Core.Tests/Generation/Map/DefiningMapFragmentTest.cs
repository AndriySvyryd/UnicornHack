namespace UnicornHack.Generation.Map;

public class DefiningMapFragmentTest
{
    private static readonly IReadOnlyDictionary<string, int> EmptyTags = new Dictionary<string, int>();

    private static readonly IReadOnlyDictionary<RoomFeatures, int> EmptyFeatures = new Dictionary<RoomFeatures, int>();

    [Fact]
    public void ConstantWeight()
    {
        var fragment = new DefiningMapFragment { Name = "test", GenerationWeight = "0.5", NoTransform = true };

        Assert.Equal(0.5, fragment.GetWeight("foo", 2, 0, EmptyTags));
    }

    [Fact]
    public void InfiniteWeight()
    {
        var fragment = new DefiningMapFragment { Name = "test", GenerationWeight = "Infinity", NoTransform = true };

        Assert.True(float.IsPositiveInfinity(fragment.GetWeight("foo", 2, 0, EmptyTags)));
    }

    [Fact]
    public void BranchWeight()
    {
        var fragment = new DefiningMapFragment { Name = "test", GenerationWeight = "$branch == 'foo' ? 1 : 0", NoTransform = true };

        Assert.Equal(0, fragment.GetWeight("bar", 2, 0, EmptyTags));
        Assert.Equal(1, fragment.GetWeight("foo", 1, 0, EmptyTags));

        fragment.GenerationWeight = "$depth >= 2 && $depth <= 3 ? 1 : 0";

        Assert.Equal(0, fragment.GetWeight("foo", 1, 0, EmptyTags));
        Assert.Equal(0, fragment.GetWeight("bar", 4, 0, EmptyTags));
        Assert.Equal(1, fragment.GetWeight("bar", 2, 0, EmptyTags));
        Assert.Equal(1, fragment.GetWeight("foo", 3, 0, EmptyTags));
    }

    [Fact]
    public void InstancesWeight()
    {
        var fragment = new DefiningMapFragment { Name = "test", GenerationWeight = "Max(3 - $instances, 0)", NoTransform = true };

        Assert.Equal(3, fragment.GetWeight("foo", 2, 0, EmptyTags));
        Assert.Equal(1, fragment.GetWeight("foo", 2, 2, EmptyTags));
        Assert.Equal(0, fragment.GetWeight("foo", 2, 4, EmptyTags));
    }

    [Fact]
    public void TagInstancesWeight()
    {
        var fragment = new DefiningMapFragment
        {
            Name = "test",
            GenerationWeight = "Min($tagInstances['shrine'] / 2.0, 3)",
            NoTransform = true
        };

        Assert.Equal(0, fragment.GetWeight("foo", 2, 0, EmptyTags));
        Assert.Equal(0.5, fragment.GetWeight("foo", 2, 0, new Dictionary<string, int> { ["shrine"] = 1 }));
        Assert.Equal(3, fragment.GetWeight("foo", 2, 0, new Dictionary<string, int> { ["shrine"] = 10 }));
    }

    [Fact]
    public void TagInstancesWeight_missing_key_returns_zero()
    {
        var fragment = new DefiningMapFragment
        {
            Name = "test",
            GenerationWeight = "$tagInstances['missing'] + 1",
            NoTransform = true
        };

        Assert.Equal(1, fragment.GetWeight("foo", 2, 0, new Dictionary<string, int> { ["other"] = 5 }));
    }

    [Fact]
    public void GlobalInstancesWeight()
    {
        var fragment = new DefiningMapFragment { Name = "test", GenerationWeight = "Max(3 - $globalInstances, 0)", NoTransform = true };

        Assert.Equal(3, fragment.GetWeight("foo", 2, RoomFeatures.Normal, instances: 0, EmptyTags, globalInstances: 0, EmptyTags, EmptyFeatures));
        Assert.Equal(1, fragment.GetWeight("foo", 2, RoomFeatures.Normal, instances: 0, EmptyTags, globalInstances: 2, EmptyTags, EmptyFeatures));
        Assert.Equal(0, fragment.GetWeight("foo", 2, RoomFeatures.Normal, instances: 0, EmptyTags, globalInstances: 4, EmptyTags, EmptyFeatures));
    }

    [Fact]
    public void GlobalTagInstancesWeight()
    {
        var fragment = new DefiningMapFragment
        {
            Name = "test",
            GenerationWeight = "Min($globalTagInstances['shrine'] / 2.0, 3)",
            NoTransform = true
        };

        Assert.Equal(0, fragment.GetWeight("foo", 2, RoomFeatures.Normal, 0, EmptyTags, 0, EmptyTags, EmptyFeatures));
        Assert.Equal(
            0.5,
            fragment.GetWeight("foo", 2, RoomFeatures.Normal, 0, EmptyTags, 0, new Dictionary<string, int> { ["shrine"] = 1 }, EmptyFeatures));
        Assert.Equal(
            3,
            fragment.GetWeight("foo", 2, RoomFeatures.Normal, 0, EmptyTags, 0, new Dictionary<string, int> { ["shrine"] = 10 }, EmptyFeatures));
    }

    [Fact]
    public void GlobalFeatureInstancesWeight()
    {
        // $globalFeatureInstances is keyed by RoomFeatures bits. Authors use it to damp
        // themed fragments once the same feature has appeared on previous levels of the
        // run (e.g. throttle deity altars after the first couple).
        var fragment = new DefiningMapFragment
        {
            Name = "test",
            // 256 = (int)RoomFeatures.DeityAltar (1 << 8). The DSL parser only accepts
            // numeric literals as index keys, so we use the raw enum value.
            GenerationWeight = "Max(3 - $globalFeatureInstances[256], 0)",
            NoTransform = true
        };

        Assert.Equal(3, fragment.GetWeight("foo", 2, RoomFeatures.DeityAltar, 0, EmptyTags, 0, EmptyTags, EmptyFeatures));
        Assert.Equal(
            1,
            fragment.GetWeight("foo", 2, RoomFeatures.DeityAltar, 0, EmptyTags, 0, EmptyTags,
                new Dictionary<RoomFeatures, int> { [RoomFeatures.DeityAltar] = 2 }));
        Assert.Equal(
            0,
            fragment.GetWeight("foo", 2, RoomFeatures.DeityAltar, 0, EmptyTags, 0, EmptyTags,
                new Dictionary<RoomFeatures, int> { [RoomFeatures.DeityAltar] = 5 }));
    }

    [Fact]
    public void GlobalFeatureInstancesWeight_missing_key_returns_zero()
    {
        var fragment = new DefiningMapFragment
        {
            Name = "test",
            // 4096 = (int)RoomFeatures.SmallFountain (1 << 12). Missing-key lookup falls
            // back to 0 via the dictionary indexer wired by UnicornExpressionVisitor.
            GenerationWeight = "$globalFeatureInstances[4096] + 1",
            NoTransform = true
        };

        Assert.Equal(
            1,
            fragment.GetWeight("foo", 2, RoomFeatures.Normal, 0, EmptyTags, 0, EmptyTags,
                new Dictionary<RoomFeatures, int> { [RoomFeatures.DeityAltar] = 4 }));
    }
}
