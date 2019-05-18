using System;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Time
{
    public class TimeSystem : IGameSystem<AdvanceTurnMessage>
    {
        public const string AdvanceTurnMessageName = "AdvanceTurn";
        public const int DefaultActionDelay = 100;

        public void AdvanceToNextPlayerTurn(GameManager manager)
        {
            while (manager.Game.ActingPlayer == null
                   && manager.Players.Any(p => p.Being.IsAlive))
            {
                EnqueueAdvanceTurn(manager);
                manager.Queue.ProcessQueue(manager);
            }
        }

        public void EnqueueAdvanceTurn(GameManager manager)
            => manager.Enqueue(manager.Queue.CreateMessage<AdvanceTurnMessage>(AdvanceTurnMessageName),
                lowPriority: true);

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
            var tick = GetTick(entity);
            Debug.Assert(tick != null, $"Entity {entity.Id} is not a supported temporal entity!");

            manager.Game.CurrentTick = tick.Value;

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

            var ability = entity.Ability;
            if (ability != null
                && ability.CooldownTick == tick)
            {
                ability.CooldownTick = null;
                return MessageProcessingResult.ContinueProcessing;
            }

            var appliedEffect = entity.Effect;
            if (appliedEffect != null)
            {
                var removeComponentMessage = manager.CreateRemoveComponentMessage();
                removeComponentMessage.Entity = entity;
                removeComponentMessage.Component = EntityComponent.Effect;
                manager.Enqueue(removeComponentMessage, lowPriority: true);
                return MessageProcessingResult.ContinueProcessing;
            }

            return MessageProcessingResult.StopProcessing;
        }

        public static int? GetTick(GameEntity entity)
        {
            var ai = entity.AI;
            if (ai != null)
            {
                return ai.NextActionTick;
            }

            var player = entity.Player;
            if (player != null)
            {
                return player.NextActionTick;
            }

            var ability = entity.Ability;
            if (ability != null)
            {
                return ability.CooldownTick;
            }

            var appliedEffect = entity.Effect;
            return appliedEffect?.ExpirationTick;
        }
    }
}
