using System.Collections;
using MessagePack;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Services;
using UnicornHack.Systems.Abilities;

namespace UnicornHack.Hubs;

public static class AbilitySnapshot
{
    public static List<object?>? Serialize(GameEntity abilityEntity, EntityState? state, SerializationContext context)
    {
        List<object?> properties;
        switch (state)
        {
            case null:
            case EntityState.Added:
            {
                var ability = abilityEntity.Ability!;
                properties =
                [
                    null,
                    context.Services.Language.GetString(ability),
                    (int)ability.Activation,
                    ability.Slot,
                    ability.CooldownTick,
                    ability.CooldownXpLeft,
                    (int)ability.TargetingShape,
                    ability.TargetingShapeSize
                ];
                return properties;
            }
            case EntityState.Deleted:
                return SerializationContext.DeletedBitArray;
            default:
            {
                var ability = abilityEntity.Ability!;
                var abilityEntry = context.DbContext.Entry(ability);
                if (abilityEntry.State == EntityState.Unchanged)
                {
                    return null;
                }

                var i = 0;
                var setValues = new bool[8];
                setValues[i++] = true;
                properties = [null];

                var name = abilityEntry.Property(nameof(AbilityComponent.Name));
                if (name.IsModified)
                {
                    setValues[i++] = true;
                    properties.Add(context.Services.Language.GetString(ability));
                }
                else
                {
                    setValues[i++] = false;
                }

                var activation = abilityEntry.Property(nameof(AbilityComponent.Activation));
                if (activation.IsModified)
                {
                    setValues[i++] = true;
                    properties.Add(ability.Activation);
                }
                else
                {
                    setValues[i++] = false;
                }

                var slot = abilityEntry.Property(nameof(AbilityComponent.Slot));
                if (slot.IsModified)
                {
                    setValues[i++] = true;
                    properties.Add(ability.Slot);
                }
                else
                {
                    setValues[i++] = false;
                }

                var cooldownTick = abilityEntry.Property(nameof(AbilityComponent.CooldownTick));
                if (cooldownTick.IsModified)
                {
                    setValues[i++] = true;
                    properties.Add(ability.CooldownTick);
                }
                else
                {
                    setValues[i++] = false;
                }

                var cooldownXpLeft = abilityEntry.Property(nameof(AbilityComponent.CooldownXpLeft));
                if (cooldownXpLeft.IsModified)
                {
                    setValues[i++] = true;
                    properties.Add(ability.CooldownXpLeft);
                }
                else
                {
                    setValues[i++] = false;
                }

                var targetingShape = abilityEntry.Property(nameof(AbilityComponent.TargetingShape));
                if (targetingShape.IsModified)
                {
                    setValues[i++] = true;
                    properties.Add((int)ability.TargetingShape);
                }
                else
                {
                    setValues[i++] = false;
                }

                var targetingType = abilityEntry.Property(nameof(AbilityComponent.TargetingShapeSize));
                if (targetingType.IsModified)
                {
                    setValues[i++] = true;
                    properties.Add(ability.TargetingShapeSize);
                }
                else
                {
                    setValues[i++] = false;
                }

                if (properties.Count == 1)
                {
                    return null;
                }

                Debug.Assert(i == 8);
                properties[0] = new BitArray(setValues);
                return properties;
            }
        }
    }

    public static List<object?> SerializeAttributes(
        GameEntity abilityEntity, GameEntity activator, SerializationContext context)
    {
        var manager = context.Manager;
        var ability = abilityEntity.Ability!;

        var activateMessage = ActivateAbilityMessage.Create(manager);
        activateMessage.AbilityEntity = abilityEntity;
        activateMessage.ActivatorEntity = activator;
        activateMessage.TargetEntity = activator;

        var stats = manager.AbilityActivationSystem.GetAttackStats(activateMessage);
        manager.ReturnMessage(activateMessage);

        var result = new List<object?>(ability.Template == null ? 20 : 21)
        {
            null,
            ability.EntityId,
            context.Services.Language.GetString(ability),
            ability.Level,
            ability.Activation,
            ability.ActivationCondition,
            ability.TargetingShape,
            ability.TargetingShapeSize,
            ability.Range,
            ability.MinHeadingDeviation,
            ability.MaxHeadingDeviation,
            ability.EnergyCost,
            stats.Delay,
            ability.Cooldown,
            ability.CooldownTick == null ? 0 : ability.CooldownTick.Value - manager.Game.CurrentTick,
            ability.XPCooldown,
            ability.CooldownXpLeft ?? 0,
            stats.SelfEffects.Select(e => SerializeEffectAttributes(e, activator, context)).ToList(),
            stats.SubAttacks.Select(s => Serialize(s, activator, context)).ToList()
        };

        if (ability.Template != null)
        {
            result.Add(context.Services.Language.GetDescription(ability.Name!, DescriptionCategory.Ability));
            //result.Add(ability.Template.Type);
            //result.Add(ability.Template.Cost);
        }

        return result;
    }

    private static List<object?> SerializeEffectAttributes(
        GameEntity effectEntity, GameEntity activator, SerializationContext context)
    {
        var effect = effectEntity.Effect!;
        return new List<object?>(9)
        {
            effect.EntityId,
            effect.EffectType,
            effect.ShouldTargetActivator,
            effect.AppliedAmount != null
                ? effect.AppliedAmount.ToString()
                : effect.Amount == null
                    ? null
                    // TODO: output a reader-friendly version of AmountExpression
                    : context.Manager.EffectApplicationSystem.GetAmount(effect, activator)
                      + $" ({effect.Amount})",
            effect.CombinationFunction,
            effect.TargetName,
            effect.SecondaryAmount,
            effect.Duration,
            effect.DurationAmount
        };
    }

    private static List<object> Serialize(SubAttackStats stats, GameEntity activator, SerializationContext context)
        => new(4)
        {
            stats.AbilityId,
            stats.SuccessCondition,
            stats.Accuracy,
            stats.Effects.Where(e => e.Effect!.EffectType != EffectType.Activate)
                .Select(e => SerializeEffectAttributes(e, activator, context)).ToList()
        };
}
