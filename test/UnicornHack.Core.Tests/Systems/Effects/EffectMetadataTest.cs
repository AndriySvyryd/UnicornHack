using UnicornHack.Generation.Effects;
using UnicornHack.Systems.Beings;

namespace UnicornHack.Systems.Effects;

public class EffectMetadataTest
{
    [Fact]
    public void Every_effect_type_except_default_has_registered_metadata()
    {
        var affectedProperties = new HashSet<string>
        {
            nameof(BeingComponent.EnergyPoints),
            nameof(BeingComponent.HitPoints),
            nameof(BeingComponent.ExperiencePoints)
        };

        foreach (var effectType in Enum.GetValues<EffectType>())
        {
            if (effectType == EffectType.Default)
            {
                // Default is intentionally unregistered - GetMetadata throws for it.
                Assert.Throws<InvalidOperationException>(() => Effect.GetMetadata(EffectType.Default));
                continue;
            }

            var metadata = Effect.GetMetadata(effectType);
            Assert.Equal(effectType, metadata.EffectType);
            Assert.True(metadata.ResourceProperty is null || affectedProperties.Contains(metadata.ResourceProperty));
            Assert.NotNull(metadata.ApplicationHandler);
            Assert.True(
                metadata.ResourceProperty is null
                ||
                metadata.CombinationFunction != ValueCombinationFunction.Subtract
                || metadata.ResistanceProperty is not null);
        }
    }

    [Fact]
    public void BuildUpPerSource_statuses_require_active_source_markers()
    {
        var requiringMarker = Enum.GetValues<EffectType>()
            .Where(effectType => effectType != EffectType.Default)
            .Select(Effect.GetMetadata)
            .Where(metadata => metadata.CombinationFunction == ValueCombinationFunction.BuildUpPerSource)
            .Select(metadata => metadata.EffectType)
            .Order()
            .ToList();

        Assert.NotEmpty(requiringMarker);
        Assert.All(requiringMarker, effectType => Assert.NotNull(Effect.GetMetadata(effectType).StatusAbilityName));
    }

}
