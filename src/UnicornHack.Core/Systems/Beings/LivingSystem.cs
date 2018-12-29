using System;
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
                        && being.HitPoints <= 0)
                    {
                        EnqueueDiedMessage(message.Entity, manager);
                    }

                    break;
                case nameof(BeingComponent.HitPointMaximum):
                    if (being.HitPoints > message.NewValue)
                    {
                        being.HitPoints = message.NewValue;
                    }

                    if (message.OldValue == 0)
                    {
                        being.HitPoints = message.NewValue;

                        // TODO: Move to AI/Player system
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
                    if (being.EnergyPoints > message.NewValue)
                    {
                        being.EnergyPoints = message.NewValue;
                    }

                    if (message.OldValue == 0)
                    {
                        being.EnergyPoints = message.NewValue;
                    }

                    break;
                case nameof(BeingComponent.Constitution):
                    var hpEffect = GetPropertyEffect(message.Entity, nameof(BeingComponent.HitPointMaximum), manager);

                    hpEffect.Amount = message.NewValue * 10;

                    break;
                case nameof(BeingComponent.Willpower):
                    var epEffect = GetPropertyEffect(message.Entity, nameof(BeingComponent.EnergyPointMaximum), manager);

                    epEffect.Amount = message.NewValue * 10;

                    break;
                case nameof(BeingComponent.Quickness):
                    var movementEffect = GetPropertyEffect(message.Entity, nameof(PositionComponent.MovementDelay), manager);

                    movementEffect.Amount = message.NewValue == 0
                        ? 0
                        : TimeSystem.DefaultActionDelay * 10 / message.NewValue;

                    break;
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        private static EffectComponent GetPropertyEffect(GameEntity entity, string propertyName, GameManager manager)
            => manager.EffectApplicationSystem.GetPropertyEffect(entity, propertyName, AttributedAbilityName);

        private void EnqueueDiedMessage(GameEntity entity, GameManager manager)
        {
            var died = manager.Queue.CreateMessage<DiedMessage>(DiedMessageName);
            died.BeingEntity = entity;
            manager.Enqueue(died);
        }
    }
}
