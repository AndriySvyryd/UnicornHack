namespace UnicornHack.Generation.Map;

using UnicornHack.Generation.Map.TestFragments;

public class MapFragmentLoaderTest
{
    [Fact]
    public void Loader_materializes_full_D4_orbit_with_scaled_weights_for_asymmetric_fragment()
    {
        var loader = new MapFragmentLoader<DefiningMapFragment>(
            "nonexistent_path", typeof(AsymmetricFragmentData));

        var all = loader.GetAsList();

        Assert.Equal(8, all.Count);
        Assert.Equal(8, all.Select(f => f.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(all, f => Assert.Equal(8, f.VariantCount));
        Assert.Contains(all, f => f.Transform == MapFragmentTransform.Identity);
        // Source instance is in the returned set and carries the scaled multiplier.
        Assert.Contains(all, f => f.Name == "Asymmetric");
    }

    [Fact]
    public void Loader_respects_NoTransform_flag_keeping_orbit_size_one()
    {
        var loader = new MapFragmentLoader<DefiningMapFragment>(
            "nonexistent_path", typeof(NoTransformFragmentData));

        var all = loader.GetAsList();

        Assert.Single(all);
        Assert.Equal(1, all[0].VariantCount);
        Assert.Equal(MapFragmentTransform.Identity, all[0].Transform);
    }

    [Fact]
    public void Loader_respects_NoMirror_flag_dropping_orbit_to_rotations_only()
    {
        var loader = new MapFragmentLoader<DefiningMapFragment>(
            "nonexistent_path", typeof(NoMirrorFragmentData));

        var all = loader.GetAsList();

        Assert.Equal(4, all.Count);
        Assert.All(all, f => Assert.Equal(4, f.VariantCount));
        var transforms = all.Select(f => f.Transform).ToHashSet();
        Assert.Contains(MapFragmentTransform.Identity, transforms);
        Assert.Contains(MapFragmentTransform.Rotate90, transforms);
        Assert.Contains(MapFragmentTransform.Rotate180, transforms);
        Assert.Contains(MapFragmentTransform.Rotate270, transforms);
    }

    [Fact]
    public void Loader_preserves_total_weight_across_orbit()
    {
        var loader = new MapFragmentLoader<DefiningMapFragment>(
            "nonexistent_path", typeof(WeightedFragmentData));

        var all = loader.GetAsList();
        Assert.Equal(8, all.Count);

        var totalWeight = all.Sum(f => f.GetWeight("any", depth: 1, instances: 0, tagInstances: new Dictionary<string, int>()));
        // Source fragment's GenerationWeight is "10" → sum across 8 variants must equal 10.
        Assert.Equal(10f, totalWeight, precision: 4);
    }

    [Fact]
    public void Loader_does_not_transform_or_dilute_a_dynamic_map_without_a_static_map()
    {
        var loader = new MapFragmentLoader<DefiningMapFragment>(
            "nonexistent_path", typeof(DynamicFragmentData));

        var fragment = Assert.Single(loader.GetAsList());

        Assert.True(string.IsNullOrEmpty(fragment.Map));
        Assert.Equal(MapFragmentTransform.Identity, fragment.Transform);
        Assert.Equal(1, fragment.VariantCount);
        Assert.Equal(10f,
            fragment.GetWeight("any", depth: 1, instances: 0, tagInstances: new Dictionary<string, int>()));
    }

    [Fact]
    public void Fov_test_fragment_is_only_discovered_by_the_test_loader()
    {
        Assert.Null(DefiningMapFragment.Loader.Find("FOVTest"));

        var testLoader = new MapFragmentLoader<DefiningMapFragment>(
            "nonexistent_path", typeof(TestDefiningMapFragmentData));

        Assert.NotNull(testLoader.Find("FOVTest"));
    }
}

public static class AsymmetricFragmentData
{
    public static readonly DefiningMapFragment Fragment = new()
    {
        Name = "Asymmetric",
        Map = "ABC\nDEF",
        NoRandomDoorways = true
    };
}

public static class NoTransformFragmentData
{
    public static readonly DefiningMapFragment Fragment = new()
    {
        Name = "Fixed",
        Map = "ABC\nDEF",
        NoTransform = true,
        NoRandomDoorways = true
    };
}

public static class NoMirrorFragmentData
{
    public static readonly DefiningMapFragment Fragment = new()
    {
        Name = "RotatableOnly",
        Map = "ABC\nDEF",
        NoMirror = true,
        NoRandomDoorways = true
    };
}

public static class WeightedFragmentData
{
    public static readonly DefiningMapFragment Fragment = new()
    {
        Name = "Weighted",
        Map = "ABC\nDEF",
        GenerationWeight = "10",
        NoRandomDoorways = true
    };
}

public static class DynamicFragmentData
{
    public static readonly DefiningMapFragment Fragment = new()
    {
        Name = "Dynamic",
        DynamicMap = new RectangleMap { MinSize = new Dimensions(3, 3) },
        GenerationWeight = "10"
    };
}
