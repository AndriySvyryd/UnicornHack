﻿using System;
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
                    if (being.HitPoints > message.NewValue)
                    {
                        being.HitPoints = message.NewValue;
                    }

                    if (message.OldValue == 1)
                    {
                        // Being just created
                        being.HitPoints = message.NewValue;
                    }

                    break;
                case nameof(BeingComponent.EnergyPointMaximum):
                    if (being.EnergyPoints > message.NewValue)
                    {
                        being.EnergyPoints = message.NewValue;
                    }

                    if (message.OldValue == 0)
                    {
                        being.EnergyPoints = message.NewValue;
                    }

                    break;
                case nameof(BeingComponent.Might):
                    var hpEffect = manager.EffectApplicationSystem.GetOrAddPropertyEffect(
                        message.Entity, nameof(BeingComponent.HitPointMaximum), AttributedAbilityName);

                    hpEffect.Amount = message.NewValue * 10;

                    break;
                case nameof(BeingComponent.Focus):
                    var epEffect = manager.EffectApplicationSystem.GetOrAddPropertyEffect(
                        message.Entity, nameof(BeingComponent.EnergyPointMaximum), AttributedAbilityName);

                    epEffect.Amount = message.NewValue * 10;

                    break;
                case nameof(BeingComponent.Perception):
                {
                    var accuracyEffect = manager.EffectApplicationSystem.GetOrAddPropertyEffect(
                        message.Entity, nameof(BeingComponent.Accuracy), AttributedAbilityName);

                    accuracyEffect.Amount = message.NewValue * 10;

                    break;
                }
                case nameof(BeingComponent.Speed):
                {
                    var effectiveSpeed = message.NewValue - being.Hindrance;
                    CalculateMovementDelay(effectiveSpeed, message.Entity, manager);
                    CalculateTurningDelay(effectiveSpeed, message.Entity, manager);
                    CalculateEvasion(effectiveSpeed, message.Entity, manager);

                    break;
                }
                case nameof(BeingComponent.Hindrance):
                {
                    var effectiveSpeed = being.Speed - message.NewValue;
                    CalculateMovementDelay(effectiveSpeed, message.Entity, manager);
                    CalculateTurningDelay(effectiveSpeed, message.Entity, manager);
                    CalculateEvasion(effectiveSpeed, message.Entity, manager);

                    break;
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        private void CalculateEvasion(int effectiveSpeed, GameEntity beingEntity, GameManager manager)
        {
            var evasionEffect = manager.EffectApplicationSystem.GetOrAddPropertyEffect(
                beingEntity, nameof(BeingComponent.Evasion), AttributedAbilityName);

            evasionEffect.Amount = 50 + effectiveSpeed * 5;
        }

        private void CalculateMovementDelay(int effectiveSpeed, GameEntity beingEntity, GameManager manager)
        {
            var movementEffect = manager.EffectApplicationSystem.GetOrAddPropertyEffect(
                beingEntity, nameof(PositionComponent.MovementDelay), AttributedAbilityName);

            movementEffect.Amount = effectiveSpeed == 0
                ? 0
                : TimeSystem.DefaultActionDelay * 10 / effectiveSpeed;
        }

        private void CalculateTurningDelay(int effectiveSpeed, GameEntity beingEntity, GameManager manager)
        {
            var turningEffect = manager.EffectApplicationSystem.GetOrAddPropertyEffect(
                beingEntity, nameof(PositionComponent.TurningDelay), AttributedAbilityName);

            turningEffect.Amount = effectiveSpeed == 0
                ? 0
                : TimeSystem.DefaultActionDelay * 10 / effectiveSpeed;
        }
    }
}
