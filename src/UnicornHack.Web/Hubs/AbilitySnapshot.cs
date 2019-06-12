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
                    properties.Add(abilityEntity.Ability.TargetingType);
                    properties.Add(abilityEntity.Ability.TargetingShape);

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
                    var targetingType = abilityEntry.Property(nameof(AbilityComponent.TargetingType));
                    if (targetingType.IsModified)
                    {
                        properties.Add(i);
                        properties.Add(abilityEntity.Ability.TargetingType);
                    }

                    i++;
                    var targetingShape = abilityEntry.Property(nameof(AbilityComponent.TargetingShape));
                    if (targetingShape.IsModified)
                    {
                        properties.Add(i);
                        properties.Add(abilityEntity.Ability.TargetingShape);
                    }

                    return properties.Count > 2 ? properties : null;
                }
            }
        }

        public static List<object> SerializeAttributes(
            GameEntity abilityEntity, GameEntity activator, SerializationContext context)
        {
            var ability = abilityEntity.Ability;
            var effects = context.Manager.EffectsToContainingAbilityRelationship[abilityEntity.Id]
                .SelectMany(e => GetActivatedEffects(e, ability, context.Manager));
            var result = new List<object>(17)
            {
                ability.EntityId,
                context.Services.Language.GetString(ability),
                ability.Level,
                ability.Activation,
                ability.ActivationCondition,
                ability.TargetingType,
                ability.TargetingShape,
                ability.Range,
                ability.HeadingDeviation,
                ability.EnergyCost,
                context.Manager.AbilityActivationSystem.GetActualDelay(ability, activator),
                ability.Cooldown,
                ability.CooldownTick == null ? 0 : ability.CooldownTick.Value - context.Manager.Game.CurrentTick,
                ability.XPCooldown,
                ability.CooldownXpLeft ?? 0,
                ability.SuccessCondition,
                effects.Select(e => SerializeAttributes(e, context)).ToList()
            };

            if (ability.Template != null)
            {
                result.Add(context.Services.Language.GetDescription(ability.Name, DescriptionCategory.Ability));
                //result.Add(ability.Template.Type);
                //result.Add(ability.Template.Cost);
                //result.Add(ability.Template.Accuracy);
            }

            return result;
        }

        private static IEnumerable<GameEntity> GetActivatedEffects(
            GameEntity effectEntity, AbilityComponent ability, GameManager manager)
        {
            var effect = effectEntity.Effect;
            if (effect.EffectType != EffectType.Activate)
            {
                return new[] {effectEntity};
            }

            var activatableEntity = manager.FindEntity(effect.TargetEntityId);
            if (activatableEntity.HasComponent(EntityComponent.Ability))
            {
                return manager.EffectsToContainingAbilityRelationship[activatableEntity.Id]
                    .SelectMany(e => GetActivatedEffects(e, ability, manager));
            }

            var trigger = ability.Trigger == ActivationType.Default
                ? AbilityActivationSystem.GetTrigger(ability)
                : ability.Trigger;
            var triggeredAbility = AbilityActivationSystem
                .GetTriggeredAbilities(activatableEntity.Id, trigger, manager)
                .FirstOrDefault();

            if (triggeredAbility == null)
            {
                return Enumerable.Empty<GameEntity>();
            }

            return manager.EffectsToContainingAbilityRelationship[triggeredAbility.Id]
                .SelectMany(e => GetActivatedEffects(e, ability, manager));
        }

        public static List<object> SerializeAttributes(GameEntity effectEntity, SerializationContext context)
        {
            var effect = effectEntity.Effect;
            return new List<object>(9)
            {
                effect.EntityId,
                effect.EffectType,
                effect.ShouldTargetActivator,
                effect.Amount?.ToString() ?? effect.AmountExpression,
                effect.Function,
                effect.TargetName,
                effect.SecondaryAmount,
                effect.Duration,
                effect.DurationAmount
            };
        }
    }
}
