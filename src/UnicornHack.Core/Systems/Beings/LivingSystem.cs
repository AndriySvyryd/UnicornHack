using System;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Time;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Beings
{
    public class LivingSystem :
        IGameSystem<XPGainedMessage>,
        IGameSystem<PropertyValueChangedMessage<GameEntity, int>>,
        IGameSystem<EntityAddedMessage<GameEntity>>
    {
        public const string AttributedAbilityName = "attributed";

        public MessageProcessingResult Process(XPGainedMessage message, GameManager manager)
        {
            var player = message.Entity.Player;
            var being = message.Entity.Being;

            var hpRegenerationRate = (float)player.NextLevelXP / (being.HitPointMaximum * 4);
            var hpRegeneratingXp = message.ExperiencePoints + being.LeftoverHPRegenerationXP;
            var hpRegenerated = (int)Math.Floor(hpRegeneratingXp / hpRegenerationRate);
            being.LeftoverHPRegenerationXP = hpRegeneratingXp % hpRegenerationRate;
            being.HitPoints += hpRegenerated;

            var epRegenerationRate = (float)player.NextLevelXP / (being.EnergyPointMaximum * 4);
            var epRegeneratingXp = message.ExperiencePoints + being.LeftoverEPRegenerationXP;
            var epRegenerated = (int)Math.Floor(epRegeneratingXp / epRegenerationRate);
            being.LeftoverEPRegenerationXP = epRegeneratingXp % epRegenerationRate;
            being.EnergyPoints += epRegenerated;

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(
            PropertyValueChangedMessage<GameEntity, int> message, GameManager manager)
        {
            var being = message.Entity.Being;
            switch (message.ChangedPropertyName)
            {
                case nameof(BeingComponent.HitPoints):
                    if (message.OldValue > 0
                        && message.NewValue <= 0)
                    {
                        being.HitPoints = 0;
                        DiedMessage.Enqueue(message.Entity, manager);
                    }

                    break;
                case nameof(BeingComponent.HitPointMaximum):
                    if (message.OldValue == 0)
                    {
                        being.HitPoints = message.NewValue;
                    }
                    else if (message.OldValue != message.NewValue)
                    {
                        being.HitPoints = (being.HitPoints * message.NewValue) / message.OldValue;
                    }

                    if (being.HitPoints > message.NewValue)
                    {
                        being.HitPoints = message.NewValue;
                    }

                    break;
                case nameof(BeingComponent.EnergyPointMaximum):
                    if (message.OldValue == 0)
                    {
                        being.EnergyPoints = message.NewValue;
                    }
                    else if (message.OldValue != message.NewValue)
                    {
                        being.EnergyPoints = (being.EnergyPoints * message.NewValue) / message.OldValue;
                    }

                    if (being.EnergyPoints > message.NewValue)
                    {
                        being.EnergyPoints = message.NewValue;
                    }

                    break;
                case nameof(BeingComponent.Might):
                    CalculateHp(message.NewValue, message.Entity, manager);

                    break;
                case nameof(BeingComponent.Focus):
                    CalculateEp(message.NewValue, message.Entity, manager);

                    break;
                case nameof(BeingComponent.Perception):
                    CalculateAccuracy(message.NewValue, message.Entity, manager);

                    break;
                case nameof(BeingComponent.Speed):
                {
                    var effectiveSpeed = message.NewValue - being.Hindrance / 10;
                    CalculateMovementDelay(effectiveSpeed, message.Entity, manager);
                    CalculateTurningDelay(effectiveSpeed, message.Entity, manager);
                    CalculateEvasion(effectiveSpeed, message.Entity, manager);

                    break;
                }
                case nameof(BeingComponent.Hindrance):
                {
                    var effectiveSpeed = being.Speed - message.NewValue / 10;
                    CalculateMovementDelay(effectiveSpeed, message.Entity, manager);
                    CalculateTurningDelay(effectiveSpeed, message.Entity, manager);
                    CalculateEvasion(effectiveSpeed, message.Entity, manager);

                    break;
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(EntityAddedMessage<GameEntity> message, GameManager manager)
        {
            var being = message.Entity.Being;

            being.HitPointMaximum = 1;
            being.HitPoints = 1;

            CalculateHp(being.Might, message.Entity, manager);
            CalculateEp(being.Focus, message.Entity, manager);
            CalculateAccuracy(being.Perception, message.Entity, manager);

            var effectiveSpeed = being.Speed - being.Hindrance / 10;
            CalculateMovementDelay(effectiveSpeed, message.Entity, manager);
            CalculateTurningDelay(effectiveSpeed, message.Entity, manager);
            CalculateEvasion(effectiveSpeed, message.Entity, manager);
            return MessageProcessingResult.ContinueProcessing;
        }

        private void CalculateHp(int might, GameEntity beingEntity, GameManager manager)
        {
            var hpEffect = manager.EffectApplicationSystem.GetOrAddPropertyEffect(
                beingEntity, nameof(BeingComponent.HitPointMaximum), AttributedAbilityName);

            hpEffect.AppliedAmount = might * 10;
        }

        private void CalculateEp(int focus, GameEntity beingEntity, GameManager manager)
        {
            var epEffect = manager.EffectApplicationSystem.GetOrAddPropertyEffect(
                beingEntity, nameof(BeingComponent.EnergyPointMaximum), AttributedAbilityName);

            epEffect.AppliedAmount = focus * 10;
        }

        private void CalculateAccuracy(int perception, GameEntity beingEntity, GameManager manager)
        {
            var accuracyEffect = manager.EffectApplicationSystem.GetOrAddPropertyEffect(
                beingEntity, nameof(BeingComponent.Accuracy), AttributedAbilityName);

            accuracyEffect.AppliedAmount = perception * 10;
        }

        private void CalculateEvasion(int effectiveSpeed, GameEntity beingEntity, GameManager manager)
        {
            var evasionEffect = manager.EffectApplicationSystem.GetOrAddPropertyEffect(
                beingEntity, nameof(BeingComponent.Evasion), AttributedAbilityName);

            evasionEffect.AppliedAmount = 50 + effectiveSpeed * 5;
        }

        private void CalculateMovementDelay(int effectiveSpeed, GameEntity beingEntity, GameManager manager)
        {
            var movementEffect = manager.EffectApplicationSystem.GetOrAddPropertyEffect(
                beingEntity, nameof(PositionComponent.MovementDelay), AttributedAbilityName);

            movementEffect.AppliedAmount = effectiveSpeed == 0
                ? 0
                : TimeSystem.DefaultActionDelay * 10 / effectiveSpeed;
        }

        private void CalculateTurningDelay(int effectiveSpeed, GameEntity beingEntity, GameManager manager)
        {
            var turningEffect = manager.EffectApplicationSystem.GetOrAddPropertyEffect(
                beingEntity, nameof(PositionComponent.TurningDelay), AttributedAbilityName);

            turningEffect.AppliedAmount = effectiveSpeed == 0
                ? 0
                : TimeSystem.DefaultActionDelay * 10 / effectiveSpeed;
        }
    }
}
