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
                    UpdateMaxHp(message.NewValue, message.Entity, manager);

                    break;
                case nameof(BeingComponent.Focus):
                    UpdateMaxEp(message.NewValue, message.Entity, manager);

                    break;
                case nameof(BeingComponent.Perception):
                    UpdateAccuracy(message.NewValue, message.Entity, manager);

                    break;
                case nameof(BeingComponent.Speed):
                {
                    var effectiveSpeed = message.NewValue - being.Hindrance / 10;
                    UpdateMovementDelay(effectiveSpeed, message.Entity, manager);
                    UpdateTurningDelay(effectiveSpeed, message.Entity, manager);
                    UpdateEvasion(effectiveSpeed, message.Entity, manager);

                    break;
                }
                case nameof(BeingComponent.Hindrance):
                {
                    var effectiveSpeed = being.Speed - message.NewValue / 10;
                    UpdateMovementDelay(effectiveSpeed, message.Entity, manager);
                    UpdateTurningDelay(effectiveSpeed, message.Entity, manager);
                    UpdateEvasion(effectiveSpeed, message.Entity, manager);

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

            UpdateMaxHp(being.Might, message.Entity, manager);
            UpdateMaxEp(being.Focus, message.Entity, manager);
            UpdateAccuracy(being.Perception, message.Entity, manager);

            var effectiveSpeed = being.Speed - being.Hindrance / 10;
            UpdateMovementDelay(effectiveSpeed, message.Entity, manager);
            UpdateTurningDelay(effectiveSpeed, message.Entity, manager);
            UpdateEvasion(effectiveSpeed, message.Entity, manager);
            return MessageProcessingResult.ContinueProcessing;
        }

        private void UpdateMaxHp(int might, GameEntity beingEntity, GameManager manager)
            => manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(might * 10,
                nameof(BeingComponent.HitPointMaximum), AttributedAbilityName, beingEntity);

        private void UpdateMaxEp(int focus, GameEntity beingEntity, GameManager manager)
            => manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(focus * 10,
                nameof(BeingComponent.EnergyPointMaximum), AttributedAbilityName, beingEntity);

        private void UpdateAccuracy(int perception, GameEntity beingEntity, GameManager manager)
            => manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(perception * 10,
                nameof(BeingComponent.Accuracy), AttributedAbilityName, beingEntity);

        private void UpdateEvasion(int effectiveSpeed, GameEntity beingEntity, GameManager manager)
            => manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(50 + effectiveSpeed * 5,
                nameof(BeingComponent.Evasion), AttributedAbilityName, beingEntity);

        private void UpdateMovementDelay(int effectiveSpeed, GameEntity beingEntity, GameManager manager)
            => manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(effectiveSpeed == 0
            ? 0
            : TimeSystem.DefaultActionDelay * 10 / effectiveSpeed,
                nameof(PositionComponent.MovementDelay), AttributedAbilityName, beingEntity);

        private void UpdateTurningDelay(int effectiveSpeed, GameEntity beingEntity, GameManager manager)
            => manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(effectiveSpeed == 0
            ? 0
            : TimeSystem.DefaultActionDelay * 10 / effectiveSpeed,
                nameof(PositionComponent.TurningDelay), AttributedAbilityName, beingEntity);
    }
}
