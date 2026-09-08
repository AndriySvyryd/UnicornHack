using System.Globalization;
using UnicornHack.Generation.Effects;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;

namespace UnicornHack.Systems.Effects;

public static class EffectTestHelper
{
    public static void UpdateOrAddPropertyEffect(
        this EffectApplicationSystem system,
        int amount, string propertyName, string abilityName, GameEntity entity,
        ValueCombinationFunction? function = null)
    {
        var manager = entity.Manager;
        var abilityEntity = manager.AffectableAbilitiesIndex[(entity.Id, abilityName)];
        var attachOwnerAfterEffect = false;
        using var abilityReference = abilityEntity == null
            ? manager.CreateEntity()
            : default;
        if (abilityEntity == null)
        {
            abilityEntity = abilityReference.Referenced;
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Name = abilityName;
            ability.Activation = ActivationType.Always;
            abilityEntity.Ability = ability;
            attachOwnerAfterEffect = true;
        }

        var reactivate = abilityEntity.Ability!.IsActive;
        if (reactivate)
        {
            manager.Enqueue(DeactivateAbilityMessage.Create(abilityEntity.Ability!, entity, manager));
            manager.Queue.ProcessQueue(manager);
        }

        GameEntity? replacedEffectEntity = null;
        foreach (var effectEntity in abilityEntity.Ability!.Effects)
        {
            if (effectEntity.Effect is { EffectType: EffectType.ChangeProperty } existingEffect
                && existingEffect.TargetName == propertyName)
            {
                replacedEffectEntity = effectEntity;
                break;
            }
        }

        if (replacedEffectEntity != null)
        {
            replacedEffectEntity.Effect = null;
        }

        AddPropertyEffect(propertyName, amount, abilityEntity.Id, manager, function);

        if (attachOwnerAfterEffect)
        {
            abilityEntity.Ability!.OwnerId = entity.Id;
        }
        else if (reactivate)
        {
            TestHelper.ActivateAbility(abilityEntity, entity, manager);
        }
    }

    private static void AddPropertyEffect(
        string propertyName, int value, int abilityId, GameManager manager,
        ValueCombinationFunction? function = null)
    {
        using var effectEntityReference = manager.CreateEntity();
        var newEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
        newEffect.EffectType = EffectType.ChangeProperty;
        newEffect.Duration = EffectDuration.Infinite;
        newEffect.TargetName = propertyName;
        newEffect.Amount = value.ToString(CultureInfo.InvariantCulture);
        newEffect.CombinationFunction = function ?? Effect.GetMetadata(EffectType.ChangeProperty).CombinationFunction;
        newEffect.ContainingAbilityId = abilityId;
        effectEntityReference.Referenced.Effect = newEffect;
    }

    public static int GetStatusEffectThreshold(GameEntity entity)
    {
        var actor = (ActorComponent?)entity.AI ?? entity.Player;
        return actor is { PreviousActionTick: { } previous, NextActionTick: { } next }
               && next > previous
            ? next - previous
            : EffectApplicationSystem.BuildUpThreshold;
    }

    public static bool HasEffectOrAccumulator(this BeingComponent being, EffectType effectType)
        => being.Entity.ActiveEffects is { } activeEffects
           && (Effect.GetMetadata(effectType).CombinationFunction.IsBuildUp()
                             ? activeEffects.Accumulators.ContainsKey(effectType)
                                 || activeEffects.PropertyEffects.Any(effect => effect.EffectType == effectType)
               : activeEffects.PropertyEffects.Any(effect => effect.EffectType == effectType));

    public static void ApplyStatus(
        GameEntity target, EffectType effectType, int duration, GameManager manager, int amount = 0)
    {
        var metadata = Effect.GetMetadata(effectType);
        if (metadata.CombinationFunction.IsBuildUp()
            && metadata.CombinationFunction != ValueCombinationFunction.BuildUpPerSource)
        {
            manager.EffectApplicationSystem.ApplyStatus(
                EffectApplicationContext.CreateStatus(
                    effectType, target, (amount == 0 ? duration : amount) * 2));
            manager.Queue.ProcessQueue(manager);
            return;
        }

        TestHelper.ApplyEffect(effectType, target, target, manager, effect =>
        {
            if (metadata.CombinationFunction == ValueCombinationFunction.BuildUpPerSource)
            {
                effect.Duration = EffectDuration.Instant;
                effect.Amount = ((amount == 0 ? duration : amount) * 2)
                    .ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                effect.Duration = EffectDuration.UntilTimeout;
                effect.DurationAmount = duration.ToString(CultureInfo.InvariantCulture);
                effect.Amount = amount.ToString(CultureInfo.InvariantCulture);
            }
        });
        manager.Queue.ProcessQueue(manager);
    }

    public static void ApplyInstantDamageEffect(
        GameEntity target, EffectType effectType, int amount, GameManager manager)
    {
        var metadata = Effect.GetMetadata(effectType);
        Neutralize(metadata.ResistanceProperty);
        if (metadata.IncludesMagicResistance)
        {
            Neutralize(nameof(BeingComponent.MagicResistance));
        }

        Neutralize(metadata.AbsorptionProperty);
        manager.Queue.ProcessQueue(manager);

        TestHelper.ApplyEffect(effectType, target, target, manager, effect =>
        {
            effect.Duration = EffectDuration.Instant;
            effect.Amount = amount.ToString(CultureInfo.InvariantCulture);
        });
        manager.Queue.ProcessQueue(manager);

        void Neutralize(string? property)
        {
            if (property is null)
            {
                return;
            }

            var current = target.ActiveEffects!.ResolveInt(property);
            if (current != 0)
            {
                manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
                    -current, property, target, manager);
            }
        }
    }

    /// <summary>
    ///     Banks a timed resource effect on the target the way an authored ability does in production: an
    ///     <see cref="EffectDuration.UntilTimeout" /> resource effect carried by a throwaway manual ability.
    /// </summary>
    public static void ApplyDamageOverTimeEffect(
        GameEntity target, EffectType effectType, int ratePerAction, int duration, GameManager manager,
        ValueCombinationFunction? combinationFunction = null)
    {
        TestHelper.ApplyEffect(effectType, target, target, manager, effect =>
        {
            effect.Duration = EffectDuration.UntilTimeout;
            effect.Amount = ratePerAction.ToString(CultureInfo.InvariantCulture);
            effect.DurationAmount = duration.ToString(CultureInfo.InvariantCulture);
            if (combinationFunction is { } function)
            {
                effect.CombinationFunction = function;
            }
        });
        manager.Queue.ProcessQueue(manager);
    }
}
