using System;
using System.Collections.Generic;
using System.Linq;
using UnicornHack.Primitives;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Time;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Beings
{
    public class LivingSystem :
        IGameSystem<XPGainedMessage>,
        IGameSystem<PropertyValueChangedMessage<GameEntity, int>>
    {
        public const string DiedMessageName = "Died";
        public const string InnateAbilityName = "innate";
        public const string AttributedAbilityName = "attributed";

        public MessageProcessingResult Process(XPGainedMessage message, GameManager manager)
        {
            var race = manager.KnowledgeSystem.GetLearningRace(message.Entity, manager);
            var being = message.Entity.Being;
            var regenerationRate = (float)race.NextLevelXP / (being.HitPointMaximum * 4);
            var regeneratingXp = message.ExperiencePoints + being.LeftoverRegenerationXP;
            var hpRegenerated = (int)Math.Floor(regeneratingXp / regenerationRate);
            being.LeftoverRegenerationXP = regeneratingXp % regenerationRate;
            ChangeCurrentHP(being, hpRegenerated);
            return MessageProcessingResult.ContinueProcessing;
        }

        public bool ChangeCurrentHP(BeingComponent being, int hp)
        {
            if (being.HitPoints <= 0)
            {
                return false;
            }

            var newHP = being.HitPoints + hp;
            if (newHP > being.HitPointMaximum)
            {
                newHP = being.HitPointMaximum;
            }

            being.HitPoints = newHP;

            if (being.HitPoints <= 0)
            {
                var entity = being.Entity;
                var ai = entity.AI;
                if (ai != null)
                {
                    ai.NextActionTick = null;
                }

                // TODO: Reset NextActionTick for player

                EnqueueDiedMessage(entity, entity.Manager);
                return false;
            }

            return true;
        }

        public void ChangeCurrentEP(BeingComponent being, int ep)
        {
            var newEP = being.EnergyPoints + ep;
            if (newEP < 0)
            {
                newEP = 0;
            }

            if (being.EnergyPoints > being.EnergyPointMaximum)
            {
                newEP = being.EnergyPointMaximum;
            }

            being.EnergyPoints = newEP;
        }

        private void EnqueueDiedMessage(GameEntity entity, GameManager manager)
        {
            var died = manager.Queue.CreateMessage<DiedMessage>(DiedMessageName);
            died.BeingEntity = entity;
            manager.Enqueue(died);
        }

        public MessageProcessingResult Process(
            PropertyValueChangedMessage<GameEntity, int> message, GameManager manager)
        {
            var being = message.Entity.Being;
            switch (message.ChangedPropertyName)
            {
                case nameof(BeingComponent.HitPointMaximum):
                    being.HitPoints = message.OldValue == 0
                        ? message.NewValue
                        : being.HitPoints * message.NewValue / message.OldValue;

                    if (message.OldValue == 0
                        && being.IsAlive)
                    {
                        // Initialize NextActionTick here so that actors don't try to act before they have HP
                        var ai = message.Entity.AI;
                        if (ai != null)
                        {
                            if (ai.NextActionTick == null)
                            {
                                ai.NextActionTick = manager.Game.CurrentTick;
                            }
                        }
                        else
                        {
                            var player = message.Entity.Player;
                            if (player != null)
                            {
                                player.NextActionTick = manager.Game.CurrentTick;
                            }
                        }
                    }

                    break;
                case nameof(BeingComponent.EnergyPointMaximum):
                    being.EnergyPoints = message.OldValue == 0
                        ? message.NewValue
                        : being.EnergyPoints * message.NewValue / message.OldValue;

                    break;
                case nameof(BeingComponent.Constitution):
                    var hpEffect = GetAttributeEffects(being, manager)
                        .Select(e => e.Effect)
                        .First(e => e.PropertyName == nameof(BeingComponent.HitPointMaximum));

                    hpEffect.Amount = message.NewValue * 10;

                    break;
                case nameof(BeingComponent.Willpower):
                    var epEffect = GetAttributeEffects(being, manager)
                        .Select(e => e.Effect)
                        .First(e => e.PropertyName == nameof(BeingComponent.EnergyPointMaximum));
                    epEffect.Amount = message.NewValue * 10;

                    break;
                case nameof(BeingComponent.Quickness):
                    var movementEffect = GetAttributeEffects(being, manager)
                        .Select(e => e.Effect)
                        .First(e => e.PropertyName == nameof(PositionComponent.MovementDelay));

                    movementEffect.Amount = message.NewValue == 0
                        ? 0
                        : TimeSystem.DefaultActionDelay * 10 / message.NewValue;

                    break;
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        private IEnumerable<GameEntity> GetAttributeEffects(BeingComponent being, GameManager manager)
        {
            var abilityId = GetAttributedAbility(being.EntityId, manager).Id;
            var effects = manager.AppliedEffectsToSourceAbilityRelationship[abilityId];
            if (!effects.Any())
            {
                // Ability hasn't been applied yet, so return the definition
                return manager.EffectsToContainingAbilityRelationship[abilityId];
            }

            return effects;
        }

        private GameEntity GetAttributedAbility(int beingId, GameManager manager)
        {
            var attributedAbility = manager.AbilitiesToAffectableRelationship[beingId]
                .FirstOrDefault(a => a.Ability.Name == AttributedAbilityName);
            if (attributedAbility == null)
            {
                using (var abilityReference = manager.CreateEntity())
                {
                    attributedAbility = abilityReference.Referenced;

                    var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
                    ability.Name = AttributedAbilityName;
                    ability.OwnerId = beingId;
                    ability.Activation = ActivationType.Always;

                    attributedAbility.Ability = ability;
                }

                AddPropertyEffect(attributedAbility.Id, nameof(BeingComponent.HitPointMaximum), manager);
                AddPropertyEffect(attributedAbility.Id, nameof(BeingComponent.EnergyPointMaximum), manager);
                AddPropertyEffect(attributedAbility.Id, nameof(PositionComponent.MovementDelay), manager);
            }

            return attributedAbility;
        }

        private EffectComponent AddPropertyEffect(int abilityId, string propertyName, GameManager manager)
        {
            using (var effectReference = manager.CreateEntity())
            {
                var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);

                effect.ContainingAbilityId = abilityId;
                effect.EffectType = EffectType.ChangeProperty;
                effect.DurationTicks = (int)EffectDuration.Infinite;
                effect.Function = ValueCombinationFunction.MeanRoundDown;
                effect.PropertyName = propertyName;

                effectReference.Referenced.Effect = effect;

                return effect;
            }
        }
    }
}
