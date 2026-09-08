using UnicornHack.Systems.Effects;

namespace UnicornHack.TestUtilities;

public static class ActiveEffectsTestExtensions
{
    public static ActivePropertyEffect CreatePropertyEffect(
        ActiveEffectsComponent activeEffects,
        int gameId, int entityId, int instanceId, EffectType effectType, string? targetName,
        ValueCombinationFunction combinationFunction = ValueCombinationFunction.Default,
        int? sourceAbilityEntityId = null,
        AbilityType sourceAbilityType = AbilityType.Default,
        ActivationType sourceAbilityActivation = ActivationType.Default,
        int magnitudePrimary = 0,
        int magnitudeSecondary = 0,
        int? expirationTarget = null,
        EffectDuration? expirationEvent = null)
    {
        return activeEffects.Entity.Manager.RentActivePropertyEffect(
            gameId, entityId, instanceId, effectType, targetName, combinationFunction,
            sourceAbilityEntityId, sourceAbilityType, sourceAbilityActivation,
            magnitudePrimary, magnitudeSecondary, expirationTarget, expirationEvent);
    }

    public static ActiveComponentEffect CreateComponentEffect(
        ActiveEffectsComponent activeEffects,
        int gameId, int entityId, int instanceId, EffectType effectType, string? targetName)
        => activeEffects.Entity.Manager.RentActiveComponentEffect(
            gameId, entityId, instanceId, effectType, targetName);

    public static EffectAccumulator CreateAccumulator(
        ActiveEffectsComponent activeEffects, EffectType effectType, int value = 0)
        => activeEffects.Entity.Manager.RentEffectAccumulator(
            activeEffects.GameId, activeEffects.EntityId, effectType, value);

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