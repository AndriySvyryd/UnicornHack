using UnicornHack.Systems.Effects;

namespace UnicornHack.TestUtilities;

public static class ActiveEffectsTestExtensions
{
    public static ActivePropertyEffect AddPropertyEffect(
        this ActiveEffectsComponent activeEffects, ActivePropertyEffect effect)
        => activeEffects.AddPropertyEffect(
            effect.EffectType,
            effect.TargetName,
            effect.CombinationFunction,
            effect.SourceAbilityEntityId,
            effect.SourceAbilityType,
            effect.SourceAbilityActivation,
            effect.MagnitudePrimary,
            effect.MagnitudeSecondary,
            effect.ExpirationTarget,
            effect.ExpirationEvent);

}