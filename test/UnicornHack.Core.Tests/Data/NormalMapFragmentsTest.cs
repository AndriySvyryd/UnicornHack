namespace UnicornHack.Data;

/// <summary>
///     Smoke tests for the hand-authored static fragments under
///     <c>Data/Fragments/Normal/</c>. Verifies the loader picks them up, applies the
///     declared transform flags, and that auto-derived tags (size + per-orientation
///     <c>door-*</c>) come out as authoring intended.
/// </summary>
public class NormalMapFragmentsTest
{
    [Fact]
    public void CrossJunction_loads_as_single_variant_with_all_four_door_tags()
    {
        var loader = NormalMapFragment.Loader;
        var variants = loader.GetAsList()
            .Where(f => string.Equals(f.SourceName, "crossJunction", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.Single(variants);
        var fragment = variants[0];
        Assert.Equal(1, fragment.VariantCount);
        Assert.Equal(MapFragmentTransform.Identity, fragment.Transform);
        Assert.NotNull(fragment.Tags);
        // 5x5 payload = 25 tiles → falls in the size-medium band (> 15, ≤ 49).
        Assert.Contains("size-medium", fragment.Tags!);
        Assert.Contains("door-n", fragment.Tags!);
        Assert.Contains("door-s", fragment.Tags!);
        Assert.Contains("door-e", fragment.Tags!);
        Assert.Contains("door-w", fragment.Tags!);
    }

    [Fact]
    public void RoomCorner_loads_as_four_rotations_each_carrying_two_door_tags()
    {
        var loader = NormalMapFragment.Loader;
        var variants = loader.GetAsList()
            .Where(f => string.Equals(f.SourceName, "roomCorner", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.Equal(4, variants.Count);
        Assert.All(variants, f => Assert.Equal(4, f.VariantCount));

        var transforms = variants.Select(f => f.Transform).ToHashSet();
        Assert.Contains(MapFragmentTransform.Identity, transforms);
        Assert.Contains(MapFragmentTransform.Rotate90, transforms);
        Assert.Contains(MapFragmentTransform.Rotate180, transforms);
        Assert.Contains(MapFragmentTransform.Rotate270, transforms);

        // The authored payload has `+` only on the S and E walls. After D4 rotations
        // the resulting door-direction tags cycle through the four (a, b) pairs:
        // {S,E}, {S,W}, {N,W}, {N,E}. Each variant therefore carries exactly two
        // door-* tags, and the four variants together cover all four pairs.
        var doorSets = variants
            .Select(f => f.Tags!
                .Where(t => t.StartsWith("door-", StringComparison.Ordinal))
                .OrderBy(t => t, StringComparer.Ordinal)
                .ToArray())
            .ToList();

        Assert.All(doorSets, doors => Assert.Equal(2, doors.Length));

        var doorSetStrings = doorSets.Select(d => string.Join(",", d)).ToHashSet();
        Assert.Contains("door-e,door-s", doorSetStrings);
        Assert.Contains("door-s,door-w", doorSetStrings);
        Assert.Contains("door-n,door-w", doorSetStrings);
        Assert.Contains("door-e,door-n", doorSetStrings);
    }

    [Fact]
    public void Corner_loads_as_four_rotations_with_no_door_tags()
    {
        var loader = NormalMapFragment.Loader;
        var variants = loader.GetAsList()
            .Where(f => string.Equals(f.SourceName, "corner", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.Equal(4, variants.Count);
        Assert.All(variants, f => Assert.Equal(4, f.VariantCount));

        var transforms = variants.Select(f => f.Transform).ToHashSet();
        Assert.Contains(MapFragmentTransform.Identity, transforms);
        Assert.Contains(MapFragmentTransform.Rotate90, transforms);
        Assert.Contains(MapFragmentTransform.Rotate180, transforms);
        Assert.Contains(MapFragmentTransform.Rotate270, transforms);

        // The inner variant has no `+` glyphs in its payload, so no door-* tags should
        // be derived for any of the four rotations.
        Assert.All(variants, f =>
        {
            Assert.NotNull(f.Tags);
            Assert.DoesNotContain(f.Tags!, t => t.StartsWith("door-", StringComparison.Ordinal));
        });
    }

    // Feature-wildcard fragments — each loads as a single identity variant
    // (NoTransform=true on a D4-symmetric payload), carries all four door-* tags
    // (four `+` doorways centered on each wall), and auto-derives the wildcard
    // feature tag from the central `$` glyph so the binding pass can substitute
    // any feature the room demands.
    [Theory]
    [InlineData("featureNiche", "size-medium")]
    [InlineData("featureChamber", "size-medium")]
    public void Wildcard_feature_fragment_loads_with_expected_tags(string sourceName, string sizeTag)
    {
        var loader = NormalMapFragment.Loader;
        var variants = loader.GetAsList()
            .Where(f => string.Equals(f.SourceName, sourceName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.Single(variants);
        var fragment = variants[0];
        Assert.Equal(1, fragment.VariantCount);
        Assert.Equal(MapFragmentTransform.Identity, fragment.Transform);
        Assert.NotNull(fragment.Tags);
        Assert.Contains(sizeTag, fragment.Tags!);
        Assert.Contains(MapFragment.WildcardFeatureTag, fragment.Tags!);
        Assert.Contains("door-n", fragment.Tags!);
        Assert.Contains("door-s", fragment.Tags!);
        Assert.Contains("door-e", fragment.Tags!);
        Assert.Contains("door-w", fragment.Tags!);
    }
}
