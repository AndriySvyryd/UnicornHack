using System.Diagnostics;
using System.Linq;
using UnicornHack.Primitives;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Time;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Actors
{
    public class AISystem :
        IGameSystem<PerformActionMessage>,
        IGameSystem<AbilityActivatedMessage>,
        IGameSystem<TraveledMessage>,
        IGameSystem<ItemEquippedMessage>,
        IGameSystem<ItemActivatedMessage>,
        IGameSystem<ItemMovedMessage>
    {
        public const string PerformAIActionMessageName = "PerformAIAction";

        public void EnqueueAIAction(GameEntity entity, GameManager manager)
        {
            var message = manager.Queue.CreateMessage<PerformActionMessage>(PerformAIActionMessageName);
            message.Actor = entity;
            manager.Enqueue(message);
        }

        public MessageProcessingResult Process(PerformActionMessage message, GameManager manager)
        {
            var actor = message.Actor;
            var ai = actor.AI;

            Debug.Assert(actor.Being.IsAlive);

            if (TryAttackPlayerCharacter(actor, manager))
            {
                return MessageProcessingResult.ContinueProcessing;
            }

            var position = actor.Position;
            if (position.MovementDelay == 0)
            {
                ai.NextActionTick += TimeSystem.DefaultActionDelay / 2;
                return MessageProcessingResult.ContinueProcessing;
            }

            var travelMessage = manager.TravelSystem.CreateTravelMessage(manager);
            travelMessage.Entity = actor;

            var directionToMove = TryGetDirectionToPlayer(actor, manager);
            if (directionToMove != null)
            {
                travelMessage.TargetCell = position.Heading != directionToMove
                    ? position.LevelCell
                    : position.LevelCell.Translate(Vector.Convert(directionToMove.Value));
                travelMessage.TargetHeading = directionToMove.Value;

                if (manager.TravelSystem.CanTravel(travelMessage, manager))
                {
                    manager.Enqueue(travelMessage);
                    return MessageProcessingResult.ContinueProcessing;
                }
            }

            var possibleDirectionsToMove =
                manager.TravelSystem.GetPossibleMovementDirections(position, safe: true, manager);
            if (possibleDirectionsToMove.Count == 0)
            {
                ai.NextActionTick += TimeSystem.DefaultActionDelay / 2;
                return MessageProcessingResult.ContinueProcessing;
            }

            if (possibleDirectionsToMove.Contains(position.Heading.Value)
                && manager.Game.Random.NextBool())
            {
                directionToMove = position.Heading.Value;
            }
            else
            {
                var directionIndex = manager.Game.Random.Next(0, possibleDirectionsToMove.Count);
                directionToMove = possibleDirectionsToMove[directionIndex];
            }

            travelMessage.TargetCell = position.Heading != directionToMove
                ? position.LevelCell
                : position.LevelCell.Translate(Vector.Convert(directionToMove.Value));
            travelMessage.TargetHeading = directionToMove.Value;

            travelMessage.TargetCell = position.LevelCell.Translate(Vector.Convert(travelMessage.TargetHeading));

            Debug.Assert(manager.TravelSystem.CanTravel(travelMessage, manager));

            manager.Enqueue(travelMessage);
            return MessageProcessingResult.ContinueProcessing;
        }

        private bool TryAttackPlayerCharacter(GameEntity aiEntity, GameManager manager)
        {
            var aiPosition = aiEntity.Position;
            foreach (var playerEntity in manager.Players)
            {
                var playerPosition = playerEntity.Position;
                if (playerPosition.LevelId != aiPosition.LevelId
                    || playerPosition.LevelCell.DistanceTo(aiPosition.LevelCell) > 1
                    || !playerEntity.Being.IsAlive)
                {
                    continue;
                }

                return Attack(aiEntity, playerEntity, manager);
            }

            return false;
        }

        private bool Attack(GameEntity aiEntity, GameEntity targetEntity, GameManager manager)
        {
            foreach (var abilityEntity in manager.AbilitiesToAffectableRelationship[aiEntity.Id])
            {
                var ability = abilityEntity.Ability;
                if (ability.Activation != ActivationType.Targeted
                    || !ability.IsUsable)
                {
                    continue;
                }

                var activationMessage = manager.AbilityActivationSystem.CreateActivateAbilityMessage(manager);
                activationMessage.AbilityEntity = abilityEntity;
                activationMessage.ActivatorEntity = aiEntity;
                activationMessage.TargetEntity = targetEntity;

                if (!manager.AbilityActivationSystem.CanActivateAbility(activationMessage))
                {
                    manager.Queue.ReturnMessage(activationMessage);
                    return false;
                }

                manager.Enqueue(activationMessage);
                return true;
            }

            return false;
        }

        public MessageProcessingResult Process(AbilityActivatedMessage message, GameManager manager)
        {
            if (message.SuccessfulActivation
                && message.Delay != 0)
            {
                var ai = message.ActivatorEntity.AI;
                if (ai != null)
                {
                    ai.NextActionTick += message.Delay;
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        private Direction? TryGetDirectionToPlayer(GameEntity aiEntity, GameManager manager)
        {
            var aiPosition = aiEntity.Position;
            foreach (var playerEntity in manager.Players)
            {
                var playerPosition = playerEntity.Position;
                if (playerPosition.LevelId != aiPosition.LevelId
                    || playerPosition.LevelCell.DistanceTo(aiPosition.LevelCell) > 8
                    || !playerEntity.Being.IsAlive)
                {
                    continue;
                }

                var level = aiPosition.LevelEntity.Level;

                // TODO: Check memory and senses
                //if (manager.SensorySystem.GetVisibility(aiEntity, playerPosition.LevelCell, level) == 0)
                //{
                //    continue;
                //}

                // TODO: Avoid connections and creatures
                return manager.TravelSystem.GetFirstStepFromShortestPath(
                    level, aiPosition.LevelCell, playerPosition.LevelCell, aiPosition.Heading.Value);
            }

            return null;
        }

        public MessageProcessingResult Process(TraveledMessage message, GameManager manager)
        {
            if (message.Successful
                && message.Delay != 0)
            {
                var ai = message.Entity.AI;
                if (ai != null)
                {
                    ai.NextActionTick += message.Delay;
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(ItemEquippedMessage message, GameManager manager)
        {
            if (message.Successful
                && message.Delay != 0)
            {
                var ai = message.ActorEntity.AI;
                if (ai != null)
                {
                    ai.NextActionTick += message.Delay;
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(ItemActivatedMessage message, GameManager state)
        {
            if (message.Successful
                && message.Delay != 0)
            {
                var ai = message.ActivatorEntity.AI;
                if (ai != null)
                {
                    ai.NextActionTick += message.Delay;
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(ItemMovedMessage message, GameManager manager)
        {
            if (message.Successful
                && message.Delay != 0)
            {
                var ai = message.InitialContainer?.AI;
                if (ai == null)
                {
                    var finalContainderId = message.ItemEntity.Item.ContainerId;
                    if (finalContainderId != null)
                    {
                        ai = manager.FindEntity(finalContainderId.Value).AI;
                    }
                }

                if (ai != null)
                {
                    ai.NextActionTick += message.Delay;
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }
    }
}
