using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Primitives;
using UnicornHack.Services;
using UnicornHack.Systems.Abilities;

namespace UnicornHack.Hubs
{
    public static class AbilitySnapshot
    {
        public static List<object> Serialize(GameEntity abilityEntity, EntityState? state, SerializationContext context)
        {
            List<object> properties;
            switch (state)
            {
                case null:
                case EntityState.Added:
                {
                    var ability = abilityEntity.Ability;

                    properties = state == null
                        ? new List<object>(6)
                        : new List<object>(7) {(int)state};

                    properties.Add(abilityEntity.Id);
                    properties.Add(context.Services.Language.GetString(ability));
                    properties.Add(abilityEntity.Ability.Activation);
                    properties.Add(abilityEntity.Ability.Slot);
                    properties.Add(abilityEntity.Ability.CooldownTick);
                    properties.Add(abilityEntity.Ability.CooldownXpLeft);
                    properties.Add(abilityEntity.Ability.TargetingShape);
                    properties.Add(abilityEntity.Ability.TargetingShapeSize);

                    return properties;
                }
                case EntityState.Deleted:
                    return new List<object>
                    {
                        (int)state,
                        abilityEntity.Id
                    };
                default:
                {
                    var ability = abilityEntity.Ability;
                    properties = new List<object>
                    {
                        (int)state,
                        abilityEntity.Id
                    };
                    var abilityEntry = context.DbContext.Entry(ability);
                    var i = 1;
                    var name = abilityEntry.Property(nameof(AbilityComponent.Name));
                    if (name.IsModified)
                    {
                        properties.Add(i);
                        properties.Add(context.Services.Language.GetString(ability));
                    }

                    i++;
                    var activation = abilityEntry.Property(nameof(AbilityComponent.Activation));
                    if (activation.IsModified)
                    {
                        properties.Add(i);
                        properties.Add(abilityEntity.Ability.Activation);
                    }

                    i++;
                    var slot = abilityEntry.Property(nameof(AbilityComponent.Slot));
                    if (slot.IsModified)
                    {
                        properties.Add(i);
                        properties.Add(abilityEntity.Ability.Slot);
                    }

                    i++;
                    var cooldownTick = abilityEntry.Property(nameof(AbilityComponent.CooldownTick));
                    if (cooldownTick.IsModified)
                    {
                        properties.Add(i);
                        properties.Add(abilityEntity.Ability.CooldownTick);
                    }

                    i++;
                    var cooldownXpLeft = abilityEntry.Property(nameof(AbilityComponent.CooldownXpLeft));
                    if (cooldownXpLeft.IsModified)
                    {
                        properties.Add(i);
                        properties.Add(abilityEntity.Ability.CooldownXpLeft);
                    }

                    i++;
                    var targetingShape = abilityEntry.Property(nameof(AbilityComponent.TargetingShape));
                    if (targetingShape.IsModified)
                    {
                        properties.Add(i);
                        properties.Add(abilityEntity.Ability.TargetingShape);
                    }

                    i++;
                    var targetingType = abilityEntry.Property(nameof(AbilityComponent.TargetingShapeSize));
                    if (targetingType.IsModified)
                    {
                        properties.Add(i);
                        properties.Add(abilityEntity.Ability.TargetingShapeSize);
                    }

                    return properties.Count > 2 ? properties : null;
                }
            }
        }

        public static List<object> SerializeAttributes(
            GameEntity abilityEntity, GameEntity activator, SerializationContext context)
        {
            var manager = context.Manager;
            var ability = abilityEntity.Ability;

            var activateMessage = ActivateAbilityMessage.Create(manager);
            activateMessage.AbilityEntity = abilityEntity;
            activateMessage.ActivatorEntity = activator;
            activateMessage.TargetEntity = activator;

            var stats = manager.AbilityActivationSystem.GetAttackStats(activateMessage);
            manager.ReturnMessage(activateMessage);

            var result = new List<object>(ability.Template == null ? 21 : 22)
            {
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
                result.Add(context.Services.Language.GetDescription(ability.Name, DescriptionCategory.Ability));
                //result.Add(ability.Template.Type);
                //result.Add(ability.Template.Cost);
            }

            return result;
        }

        private static List<object> SerializeEffectAttributes(
            GameEntity effectEntity, GameEntity activator, SerializationContext context)
        {
            var effect = effectEntity.Effect;
            return new List<object>(9)
            {
                effect.EntityId,
                effect.EffectType,
                effect.ShouldTargetActivator,
                effect.Amount != null
                    ? effect.Amount.ToString()
                    : effect.AmountExpression == null
                        ? null
                        : context.Manager.EffectApplicationSystem.GetActualAmount(effect, activator).ToString(),
                effect.Function,
                effect.TargetName,
                effect.SecondaryAmount,
                effect.Duration,
                effect.DurationAmount
            };
        }

        private static List<object> Serialize(SubAttackStats stats, GameEntity activator, SerializationContext context)
            => new List<object>(3)
            {
                stats.SuccessCondition,
                stats.Accuracy,
                stats.Effects.Where(e => e.Effect.EffectType != EffectType.Activate)
                    .Select(e => SerializeEffectAttributes(e, activator, context)).ToList()
            };
    }
}
