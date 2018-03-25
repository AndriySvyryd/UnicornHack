using System;
using System.Diagnostics;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Time
{
    public class TimeSystem : IGameSystem<AdvanceTurnMessage>
    {
        public const string AdvanceTurnMessageName = "AdvanceTurn";
        public const int DefaultActionDelay = 100;

        public static void AdvanceToNextPlayerTurn(GameManager manager)
        {
            while (manager.Game.ActingPlayer == null)
            {
                EnqueueAdvanceTurn(manager);
                manager.Queue.ProcessQueue(manager);
            }
        }

        public static void EnqueueAdvanceTurn(GameManager manager)
            => manager.Enqueue(manager.Queue.CreateMessage<AdvanceTurnMessage>(AdvanceTurnMessageName));

        // TODO: Handle ability timeout
        public MessageProcessingResult Process(AdvanceTurnMessage message, GameManager manager)
        {
            if (manager.TemporalEntitiesIndex.Count == 0)
            {
                if (manager.Queue.QueuedCount == 0)
                {
                    throw new InvalidOperationException("No actors found!");
                }

                return MessageProcessingResult.StopProcessing;
            }

            var entity = manager.TemporalEntitiesIndex.First();
            manager.Game.CurrentTick = GetTick(entity).Value;

            var ai = entity.AI;
            if (ai != null)
            {
                manager.AISystem.EnqueueAIAction(entity, manager);
                return MessageProcessingResult.ContinueProcessing;
            }

            var player = entity.Player;
            if (player != null)
            {
                manager.PlayerSystem.EnqueuePlayerAction(entity, manager);
                return MessageProcessingResult.ContinueProcessing;
            }

            var appliedEffect = entity.Effect;
            if (appliedEffect != null)
            {
                entity.RemoveComponent(EntityComponent.Effect, appliedEffect);
                return MessageProcessingResult.ContinueProcessing;
            }

            Debug.Fail($"Entity {entity.Id} is not a supported temporal entity!");

            return MessageProcessingResult.StopProcessing;
        }

        public static int? GetTick(GameEntity entity)
        {
            int? tick = null;
            var ai = entity.AI;
            if (ai != null)
            {
                tick = ai.NextActionTick;
                return tick;
            }

            var appliedEffect = entity.Effect;
            if (appliedEffect != null)
            {
                tick = appliedEffect.ExpirationTick;
                return tick;
            }

            var player = entity.Player;
            if (player != null)
            {
                tick = player.NextActionTick;
            }

            return tick;
        }
    }
}
