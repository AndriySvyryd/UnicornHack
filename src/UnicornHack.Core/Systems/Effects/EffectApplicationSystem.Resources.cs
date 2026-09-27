using UnicornHack.Services.LogEvents;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Time;

namespace UnicornHack.Systems.Effects;

public partial class EffectApplicationSystem
{
    public void ApplyResourceEffect(
        EffectComponent effect, GameEntity? targetEntity, GameEntity activatorEntity,
        int rawAmount, EffectResultMessage? result, GameManager manager, bool pretend)
    {
        if (targetEntity?.Being == null)
        {
            return;
        }

        var metadata = Effect.GetMetadata(effect.EffectType);
        var restoresActivator = metadata.RestoresActivator;
        var target = targetEntity!;
        var combinationFunction = effect.CombinationFunction;
        var resourceAmount = CalculateEffectiveAmount(
            effect.EffectType, effect, activatorEntity, target, rawAmount, combinationFunction);
        var secondaryAmount = effect.SecondaryAmount != null
            ? GetSecondaryAmount(effect, activatorEntity, target)
            : 0;
        if (resourceAmount == 0)
        {
            return;
        }

        if (effect.Duration == EffectDuration.Instant)
        {
            if (result != null)
            {
                AccumulateResourceChange(
                    result.ResourceChanges, effect.EffectType,
                    GetResourceChangeAmount(combinationFunction, resourceAmount));
            }
        }

        if (pretend)
        {
            return;
        }

        if (effect.Duration == EffectDuration.Instant)
        {
            var changedAmount = ApplyResourceChange(
                metadata, combinationFunction, target, resourceAmount,
                restoresActivator ? activatorEntity : null, manager);
            if (changedAmount != 0
                && result != null)
            {
                result.AnyEffectApplied = true;
            }

            if (changedAmount > 0
                && result == null)
            {
                LogResourceEffects(
                    target, new Dictionary<EffectType, int> { [effect.EffectType] = changedAmount }, manager);
            }
        }
        else
        {
            if (result != null)
            {
                result.AnyEffectApplied = true;
            }

            if (result is AttackResultMessage attackResult)
            {
                attackResult.LingeringEffects.TryGetValue(effect.EffectType, out var existing);
                attackResult.LingeringEffects[effect.EffectType] = checked(existing + resourceAmount);
            }

            var periodicEffects = target.ActiveEffects!;
            var expiration = GetExpiration(effect, activatorEntity, targetEntity, manager);
            var sourceAbilityActivation = effect.ContainingAbility?.Ability?.Activation ?? ActivationType.Default;
            var sourceAbilityId = restoresActivator
                                  || metadata.DurationPolicy != DurationPolicy.Independent
                                  || (sourceAbilityActivation & ActivationType.Continuous) != 0
                ? effect.ContainingAbilityId
                : null;
            if (metadata.DurationPolicy != DurationPolicy.Independent)
            {
                foreach (var existing in periodicEffects.PropertyEffects)
                {
                    if (existing.EffectType == effect.EffectType
                        && existing.SourceAbilityEntityId == sourceAbilityId
                        && existing.Metadata?.ResourceProperty != null)
                    {
                        existing.MagnitudePrimary = resourceAmount;
                        existing.MagnitudeSecondary = secondaryAmount;
                        existing.CombinationFunction = combinationFunction;
                        CombineExpiration(
                            existing, expiration, metadata.DurationPolicy, manager.Game.CurrentTick);
                        existing.SourceAbilityActivation = sourceAbilityActivation;
                        EnsureStatusAbility(effect.EffectType, target, manager);
                        RecomputeNextEffectTick(periodicEffects);
                        return;
                    }
                }
            }

            periodicEffects.AddPropertyEffect(
                effect.EffectType,
                combinationFunction: combinationFunction,
                sourceAbilityEntityId: sourceAbilityId,
                sourceAbilityActivation: sourceAbilityActivation,
                magnitudePrimary: resourceAmount,
                magnitudeSecondary: secondaryAmount,
                expirationTarget: expiration.Target,
                expirationEvent: expiration.Event);

            AdvanceNextEffectTick(periodicEffects, expiration);
            EnsureStatusAbility(effect.EffectType, target, manager);
        }
    }

    private int ApplyResourceChange(
        EffectMetadata metadata, ValueCombinationFunction combinationFunction,
        GameEntity targetEntity, int resourceAmount,
        GameEntity? restoringEntity, GameManager manager)
    {
        var targetAmount = GetResourceChangeAmount(combinationFunction, resourceAmount);
        var changedAmount = ChangeBeingProperty(
            metadata.ResourceProperty!, targetEntity, targetAmount, manager);
        if (changedAmount == 0)
        {
            return 0;
        }

        if (restoringEntity != null)
        {
            var restoration = CalculateEffectiveAmount(
                metadata.EffectType, effect: null, restoringEntity, restoringEntity,
                Math.Abs(changedAmount), ValueCombinationFunction.Sum);
            ChangeBeingProperty(metadata.ResourceProperty!, restoringEntity, restoration, manager);
        }

        ApplyRiders(metadata.EffectType, Math.Abs(changedAmount), targetEntity, manager);
        return changedAmount;
    }

    private static int ChangeBeingProperty(
        string property, GameEntity targetEntity, int amount, GameManager manager)
    {
        var being = targetEntity.Being!;
        var oldValue = property switch
        {
            nameof(BeingComponent.HitPoints) => being.HitPoints,
            nameof(BeingComponent.EnergyPoints) => being.EnergyPoints,
            nameof(BeingComponent.ExperiencePoints) => being.ExperiencePoints,
            _ => throw new InvalidOperationException($"Unsupported resource property '{property}'.")
        };

        switch (property)
        {
            case nameof(BeingComponent.HitPoints):
                being.HitPoints += amount;
                break;
            case nameof(BeingComponent.EnergyPoints):
                being.EnergyPoints += amount;
                break;
            case nameof(BeingComponent.ExperiencePoints):
                Debug.Assert(amount >= 0);
                manager.XPSystem.AddPlayerXP(amount, manager);
                break;
            default:
                throw new InvalidOperationException($"Unsupported resource property '{property}'.");
        }

        var newValue = property switch
        {
            nameof(BeingComponent.HitPoints) => being.HitPoints,
            nameof(BeingComponent.EnergyPoints) => being.EnergyPoints,
            nameof(BeingComponent.ExperiencePoints) => being.ExperiencePoints,
            _ => oldValue
        };
        return newValue - oldValue;
    }

    /// <summary>Extends every active timed bleeding effect when its target moves.</summary>
    public void ExtendEffectsOnMovement(GameEntity actorEntity, GameManager manager)
    {
        var activeEffects = actorEntity.ActiveEffects!;
        var extended = false;
        foreach (var instance in activeEffects.PropertyEffects)
        {
            if (instance is
                {
                    ExpirationEvent: EffectDuration.UntilTimeout,
                    ExpirationTarget: { } expiration
                }
                && instance.Metadata?.ExtendsOnMovement == true)
            {
                instance.ExpirationTarget = expiration + TimeSystem.DefaultActionDelay;
                extended = true;
            }
        }

        if (extended)
        {
            RecomputeNextEffectTick(activeEffects);
        }
    }

    private int ApplyLingeringResourceEffect(
        ActivePropertyEffect activeEffect, GameEntity targetEntity, int elapsed, GameManager manager)
    {
        if (elapsed <= 0)
        {
            return 0;
        }

        var resourceAmount = activeEffect.MagnitudePrimary * elapsed / TimeSystem.DefaultActionDelay;
        if (resourceAmount <= 0)
        {
            return 0;
        }

        var metadata = activeEffect.Metadata!;
        var restoringEntity = metadata.RestoresActivator
                              && manager.FindEntity(activeEffect.SourceAbilityEntityId)?.Ability?.OwnerEntity is
                                  { Being: not null } activatorEntity
            ? activatorEntity
            : null;
        return ApplyResourceChange(
            metadata, activeEffect.CombinationFunction, targetEntity, resourceAmount, restoringEntity, manager);
    }

    public void ApplyLingeringResourceEffects(GameEntity targetEntity, int elapsed)
    {
        if (elapsed <= 0
            || targetEntity.ActiveEffects is not { } activeEffects)
        {
            return;
        }

        var resourceChanges = new Dictionary<EffectType, int>(activeEffects.PropertyEffects.Count);
        ActiveEffectsComponent.ForEachEffect(
            activeEffects.PropertyEffects,
            (System: this, TargetEntity: targetEntity, Elapsed: elapsed, Manager: targetEntity.Manager,
                ResourceChanges: resourceChanges),
            static (activeEffect, state) =>
            {
                if (activeEffect.Metadata?.ResourceProperty != null)
                {
                    var changedAmount = state.System.ApplyLingeringResourceEffect(
                        activeEffect, state.TargetEntity, state.Elapsed, state.Manager);
                    if (changedAmount != 0)
                    {
                        AccumulateResourceChange(
                            state.ResourceChanges, activeEffect.EffectType, changedAmount);
                    }
                }
            });

        if (resourceChanges.Count != 0)
        {
            LogResourceEffects(targetEntity, resourceChanges, targetEntity.Manager);
        }
    }

    private static void LogResourceEffects(
        GameEntity targetEntity, IReadOnlyDictionary<EffectType, int> resourceChanges, GameManager manager)
    {
        foreach (var playerEntity in manager.Players)
        {
            var targetSensed = manager.SensorySystem.CanSense(playerEntity, targetEntity) ?? SenseType.None;
            if (targetSensed == SenseType.None)
            {
                continue;
            }

            var logMessage = manager.Game.Services.Language.GetString(
                new ResourceEffectEvent(playerEntity, targetEntity, resourceChanges, targetSensed));
            manager.LoggingSystem.WriteLog(logMessage, playerEntity, manager);
        }
    }

    private static int GetResourceChangeAmount(
        ValueCombinationFunction combinationFunction, int resourceAmount)
        => combinationFunction == ValueCombinationFunction.Subtract
            ? -resourceAmount
            : resourceAmount;

    private static void AccumulateResourceChange(
        Dictionary<EffectType, int> resourceChanges, EffectType effectType, int amount)
    {
        resourceChanges.TryGetValue(effectType, out var existing);
        resourceChanges[effectType] = existing + amount;
    }

    private int CalculateEffectiveAmount(
        EffectType effectType,
        EffectComponent? effect,
        GameEntity activatorEntity,
        GameEntity targetEntity,
        int rawAmount,
        ValueCombinationFunction combinationFunction)
    {
        var metadata = Effect.GetMetadata(effectType);
        var activeEffects = targetEntity.ActiveEffects!;
        if (metadata.ResourceProperty != null
            && combinationFunction != ValueCombinationFunction.Subtract)
        {
            if (rawAmount <= 0)
            {
                return rawAmount;
            }

            var restorationResistance = metadata.ResistanceProperty is { } restorationResistanceProperty
                ? activeEffects.ResolveInt(restorationResistanceProperty)
                : 0;
            return (int)Math.Clamp(
                (long)rawAmount * (100 - restorationResistance) / 100,
                int.MinValue, int.MaxValue);
        }

        var absorption = 0;
        if (effect != null
            && metadata.AbsorptionProperty is { } absorptionProperty)
        {
            absorption = activeEffects.ResolveInt(absorptionProperty)
                         - GetSecondaryAmount(effect, activatorEntity, targetEntity);
        }

        var damage = rawAmount;
        if (absorption > 0
            && damage > 0)
        {
            damage = Math.Max(1, damage - absorption * damage / (absorption + damage));
        }

        if (damage < 0)
        {
            damage = 0;
        }

        var resistance = metadata.ResistanceProperty is { } resistanceProperty
            ? activeEffects.ResolveInt(resistanceProperty)
            : 0;
        if (metadata.CombinationFunction.IsBuildUp())
        {
            resistance += metadata.StatusCategory switch
            {
                StatusCategory.Physical => targetEntity.Being!.PhysicalStatusResistance,
                StatusCategory.Mental => targetEntity.Being!.MentalStatusResistance,
                _ => 0
            };
        }
        if (metadata.IncludesMagicResistance)
        {
            resistance += activeEffects.ResolveInt(nameof(BeingComponent.MagicResistance));
        }

        if (effect != null
            && metadata.ResistanceProperty != null
            && effectType != EffectType.PhysicalDamage
            && resistance >= 0)
        {
            var penetration = Math.Max(0, GetSecondaryAmount(effect, activatorEntity, targetEntity));
            resistance = Math.Max(0, resistance - penetration);
        }

        return Math.Max(0, damage * (100 - resistance) / 100);
    }

    // A folded instant resource-change total for one effect type: the combined amount plus the running sum
    // and summand count the combination function needs, and one representative effect entity to apply it.
    private readonly struct ResourceChangeAccumulation
    {
        public readonly int Amount;
        public readonly int RunningSum;
        public readonly int SummandCount;
        public readonly GameEntity? EffectEntity;

        public ResourceChangeAccumulation(int amount, int runningSum, int summandCount, GameEntity? effectEntity)
        {
            Amount = amount;
            RunningSum = runningSum;
            SummandCount = summandCount;
            EffectEntity = effectEntity;
        }
    }

    // Folds a same-type instant resource effect into its total. Subtract describes the eventual direction
    // (damage), so its magnitudes sum just like additive healing; the representative effect carries the
    // metadata and target used when the total is applied.
    private static void AccumulateRawResourceEffect(
        Dictionary<EffectType, ResourceChangeAccumulation> resourceChanges, GameEntity effectEntity, int amount)
    {
        var effect = effectEntity.Effect!;
        var key = effect.EffectType;
        resourceChanges.TryGetValue(key, out var existing);
        var value = existing.Amount;
        var runningSum = existing.RunningSum;
        var summandCount = existing.SummandCount;
        var combinationFunction = effect.CombinationFunction;
        var aggregationFunction = combinationFunction == ValueCombinationFunction.Subtract
            ? ValueCombinationFunction.Sum
            : combinationFunction;
        PropertyValueCombiner.Apply(amount, aggregationFunction, ref value, ref runningSum, ref summandCount);
        resourceChanges[key] = new ResourceChangeAccumulation(value, runningSum, summandCount, effectEntity);
    }

    public int GetExpectedDamage(IEnumerable<GameEntity> effects, GameEntity activatorEntity,
        GameEntity targetEntity)
    {
        var totalDamage = 0;
        var damageEffects = new Dictionary<EffectType, ResourceChangeAccumulation>();
        foreach (var effectEntity in effects)
        {
            var effect = effectEntity.Effect!;
            if (Effect.GetMetadata(effect.EffectType).CombinationFunction != ValueCombinationFunction.Subtract)
            {
                continue;
            }

            AccumulateRawResourceEffect(
                damageEffects, effectEntity, GetAmount(effect, activatorEntity, targetEntity));
        }

        foreach (var damageEffect in damageEffects)
        {
            totalDamage += CalculateEffectiveAmount(
                damageEffect.Key, damageEffect.Value.EffectEntity!.Effect!, activatorEntity, targetEntity,
                damageEffect.Value.Amount, ValueCombinationFunction.Subtract);
        }

        return totalDamage;
    }
}
