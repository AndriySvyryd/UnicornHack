using System.Diagnostics;
using System.Linq;
using UnicornHack.Primitives;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Time;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Actors
{
    public class AISystem :
        IGameSystem<PerformActionMessage>,
        IGameSystem<DiedMessage>,
        IGameSystem<EntityAddedMessage<GameEntity>>,
        IGameSystem<PropertyValueChangedMessage<GameEntity, bool>>,
        IGameSystem<DelayMessage>
    {

        public MessageProcessingResult Process(PerformActionMessage message, GameManager manager)
        {
            var actor = message.Actor;
            var ai = actor.AI;

            Debug.Assert(actor.Being.IsAlive);

            // TODO: If can't act - wait till player turn
            switch (TryAttackPlayerCharacter(actor, manager))
            {
                case true:
                    return MessageProcessingResult.ContinueProcessing;
                case false:
                    ai.NextActionTick += TimeSystem.DefaultActionDelay / 2;
                    return MessageProcessingResult.ContinueProcessing;
            }

            var position = actor.Position;
            if (position.MovementDelay == 0)
            {
                ai.NextActionTick += TimeSystem.DefaultActionDelay / 2;
                return MessageProcessingResult.ContinueProcessing;
            }

            var travelMessage = TravelMessage.Create(manager);
            travelMessage.Entity = actor;

            var directionToMove = TryGetDirectionToPlayer(actor, manager);
            if (directionToMove != null)
            {
                travelMessage.TargetCell = position.Heading != directionToMove
                    ? position.LevelCell
                    : position.LevelCell.Translate(directionToMove.Value.AsVector());
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
                : position.LevelCell.Translate(directionToMove.Value.AsVector());
            travelMessage.TargetHeading = directionToMove.Value;

            travelMessage.TargetCell = position.LevelCell.Translate(travelMessage.TargetHeading.AsVector());

            Debug.Assert(manager.TravelSystem.CanTravel(travelMessage, manager));

            manager.Enqueue(travelMessage);
            return MessageProcessingResult.ContinueProcessing;
        }

        private bool? TryAttackPlayerCharacter(GameEntity aiEntity, GameManager manager)
        {
            bool? ableToAttack = null;
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

                ableToAttack = false;
                if (Attack(aiEntity, playerEntity, manager))
                {
                    return true;
                }
            }

            return ableToAttack;
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

                var activationMessage = ActivateAbilityMessage.Create(manager);
                activationMessage.AbilityEntity = abilityEntity;
                activationMessage.ActivatorEntity = aiEntity;
                activationMessage.TargetEntity = targetEntity;

                if (!manager.AbilityActivationSystem.CanActivateAbility(activationMessage, shouldThrow: false))
                {
                    manager.Queue.ReturnMessage(activationMessage);
                    continue;
                }

                manager.Enqueue(activationMessage);
                return true;
            }

            return false;
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

        public MessageProcessingResult Process(DelayMessage message, GameManager state)
        {
            var ai = message.ActorEntity.AI;
            if (ai?.NextActionTick != null)
            {
                ai.NextActionTick += message.Delay;
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(DiedMessage message, GameManager manager)
        {
            var ai = message.BeingEntity.AI;
            if (ai != null)
            {
                ai.NextActionTick = null;
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(EntityAddedMessage<GameEntity> message, GameManager manager)
        {
            TrySlot(message.Entity.Ability, manager);

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(
            PropertyValueChangedMessage<GameEntity, bool> message, GameManager manager)
        {
            Debug.Assert(message.ChangedPropertyName == nameof(AbilityComponent.IsUsable));

            TrySlot((AbilityComponent)message.ChangedComponent, manager);

            return MessageProcessingResult.ContinueProcessing;
        }

        private static void TrySlot(AbilityComponent ability, GameManager manager)
        {
            var owner = ability.OwnerEntity;
            if (owner.AI == null
                || (ability.Activation & ActivationType.Slottable) == 0
                || ability.Slot != null
                || !ability.IsUsable
                || ability.Template?.Type == AbilityType.DefaultAttack)
            {
                return;
            }

            for (var i = 0; i < owner.Being.AbilitySlotCount; i++)
            {
                if (manager.SlottedAbilitiesIndex[(owner.Id, i)] != null)
                {
                    continue;
                }

                var setSlotMessage = SetAbilitySlotMessage.Create(manager);
                setSlotMessage.Slot = i;
                setSlotMessage.AbilityEntity = ability.Entity;

                manager.AbilitySlottingSystem.Process(setSlotMessage, manager);
                manager.Queue.ReturnMessage(setSlotMessage);

                break;
            }
        }
    }
}
