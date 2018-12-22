using System;
using System.Diagnostics;
using UnicornHack.Primitives;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Time;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Actors
{
    public class PlayerSystem :
        IGameSystem<PerformActionMessage>,
        IGameSystem<AbilityActivatedMessage>,
        IGameSystem<TraveledMessage>,
        IGameSystem<ItemEquippedMessage>,
        IGameSystem<ItemMovedMessage>,
        IGameSystem<DiedMessage>
    {
        public const string PerformPlayerActionMessageName = "PerformPlayerAction";

        public void EnqueuePlayerAction(GameEntity entity, GameManager manager)
        {
            var message = manager.Queue.CreateMessage<PerformActionMessage>(PerformPlayerActionMessageName);
            message.Actor = entity;
            manager.Enqueue(message);
        }

        public MessageProcessingResult Process(PerformActionMessage message, GameManager manager)
        {
            if (!ProcessTurn(message, manager))
            {
                manager.Game.ActingPlayer = message.Actor;
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        private bool ProcessTurn(PerformActionMessage message, GameManager manager)
        {
            var playerEntity = message.Actor;
            var player = playerEntity.Player;

            if (!playerEntity.Being.IsAlive)
            {
                player.NextActionTick += TimeSystem.DefaultActionDelay;
                return false;
            }

            // TODO: add an option to stop here and display current state while performing multi-action commands (every x actions)

            var action = player.NextAction;
            var target = player.NextActionTarget;
            var target2 = player.NextActionTarget2;
            if (action == null)
            {
                return false;
            }

            if (!player.QueuedAction)
            {
                player.CommandHistory.Add(new PlayerCommand
                {
                    GameId = playerEntity.GameId,
                    Id = ++playerEntity.Game.NextCommandId,
                    PlayerId = playerEntity.Id,
                    Tick = manager.Game.CurrentTick,
                    Action = action.Value,
                    Target = target,
                    Target2 = target2
                });
            }

            player.NextAction = null;
            player.NextActionTarget = null;
            player.NextActionTarget2 = null;
            player.QueuedAction = false;

            var position = playerEntity.Position;
            switch (action)
            {
                case PlayerAction.Wait:
                    player.NextActionTick += TimeSystem.DefaultActionDelay;
                    break;
                case PlayerAction.ChangeHeading:
                case PlayerAction.MoveOneCell:
                    var moveDirection = (Direction)target.Value;
                    switch (moveDirection)
                    {
                        case Direction.Down:
                        case Direction.Up:
                            manager.LoggingSystem.WriteLog(
                                manager.Game.Services.Language.UnableToMove(moveDirection), playerEntity, manager);
                            break;
                        default:
                            Move(moveDirection, playerEntity, manager,
                                onlyChangeHeading: action == PlayerAction.ChangeHeading);
                            break;
                    }

                    break;
                case PlayerAction.MoveToCell:
                    var targetCell = Point.Unpack(target).Value;

                    if (position.LevelCell.DistanceTo(targetCell) == 1)
                    {
                        return Move(position.LevelCell.DifferenceTo(targetCell).AsDirection(),
                            playerEntity, manager);
                    }
                    else
                    {
                        if (!Move(targetCell, playerEntity, manager))
                        {
                            return false;
                        }

                        if (position.LevelCell != targetCell)
                        {
                            player.NextAction = PlayerAction.MoveToCell;
                            player.NextActionTarget = target;
                            player.QueuedAction = true;
                        }
                    }

                    break;
                case PlayerAction.DropItem:
                    var itemToDrop = GetItem(target.Value, playerEntity, manager);
                    if (itemToDrop == null)
                    {
                        return false;
                    }

                    var dropMessage = manager.ItemMovingSystem.CreateMoveItemMessage(manager);
                    dropMessage.ItemEntity = itemToDrop;
                    dropMessage.TargetLevelEntity = position.LevelEntity;
                    dropMessage.TargetCell = position.LevelCell;

                    manager.Enqueue(dropMessage);
                    break;
                case PlayerAction.EquipItem:
                    var itemToEquip = GetItem(target.Value, playerEntity, manager);
                    var slot = (EquipmentSlot)target2.Value;
                    if (itemToEquip == null)
                    {
                        return false;
                    }

                    var equipMessage = manager.ItemUsageSystem.CreateEquipItemMessage(manager);
                    equipMessage.ActorEntity = playerEntity;
                    equipMessage.ItemEntity = itemToEquip;
                    equipMessage.Slot = slot;

                    manager.Enqueue(equipMessage);
                    break;
                case PlayerAction.UnequipItem:
                    var itemToUnequip = GetItem(target.Value, playerEntity, manager);
                    if (itemToUnequip == null)
                    {
                        return false;
                    }

                    var unequipMessage = manager.ItemUsageSystem.CreateEquipItemMessage(manager);
                    unequipMessage.ActorEntity = playerEntity;
                    unequipMessage.ItemEntity = itemToUnequip;
                    unequipMessage.Slot = EquipmentSlot.None;

                    manager.Enqueue(unequipMessage);
                    break;
                case PlayerAction.SetAbilitySlot:
                    var setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
                    setSlotMessage.AbilityEntity = manager.FindEntity(target);
                    setSlotMessage.OwnerEntity = playerEntity;
                    setSlotMessage.Slot = target2;

                    manager.Enqueue(setSlotMessage);
                    break;
                case PlayerAction.UseAbilitySlot:
                    if (target == null)
                    {
                        throw new InvalidOperationException("Must specify ability slot number");
                    }

                    var abilityEntity = manager.AbilitySlottingSystem.GetAbility(playerEntity.Id, target.Value, manager);
                    if (abilityEntity == null)
                    {
                        throw new InvalidOperationException("No ability in slot" + target);
                    }

                    ActivateAbility(abilityEntity, playerEntity, target2, manager);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Action {action} on character {playerEntity.Player.ProperName} is invalid.");
            }

            return true;
        }

        private bool Move(Point targetCell, GameEntity playerEntity, GameManager manager)
        {
            // TODO: Only consider known terrain, avoid actors and connections
            var position = playerEntity.Position;
            var direction = manager.TravelSystem.GetFirstStepFromShortestPath(
                position.LevelEntity.Level, position.LevelCell, targetCell, position.Heading.Value);
            if (direction == null)
            {
                manager.LoggingSystem.WriteLog(
                    manager.Game.Services.Language.NoPath(), playerEntity, manager);
                return false;
            }

            return Move(direction.Value, playerEntity, manager);
        }

        private bool Move(Direction direction, GameEntity playerEntity, GameManager manager,
            bool onlyChangeHeading = false)
        {
            // TODO: only attack on move if hostile
            var position = playerEntity.Position;
            var targetCell = position.LevelCell.Translate(direction.AsVector());
            var conflictingActor = manager.LevelActorToLevelCellIndex[(position.LevelId, targetCell.X, targetCell.Y)];
            if (conflictingActor != null)
            {
                var abilityEntity = manager.AbilitySlottingSystem.GetAbility(
                    playerEntity.Id, AbilitySlottingSystem.DefaultAttackSlot, manager);
                return ActivateAbility(
                    abilityEntity, playerEntity, targetCell, conflictingActor, manager);
            }

            var travelMessage = manager.TravelSystem.CreateTravelMessage(manager);
            travelMessage.Entity = playerEntity;
            travelMessage.TargetHeading = direction;
            travelMessage.TargetCell = onlyChangeHeading
                ? position.LevelCell
                : position.LevelCell.Translate(travelMessage.TargetHeading.AsVector());
            travelMessage.MoveOffConflicting = true;

            if (!manager.TravelSystem.CanTravel(travelMessage, manager))
            {
                manager.LoggingSystem.WriteLog(
                    manager.Game.Services.Language.UnableToMove(direction), playerEntity, manager);
                manager.Queue.ReturnMessage(travelMessage);
                return false;
            }

            manager.Enqueue(travelMessage);
            return true;
        }

        public MessageProcessingResult Process(TraveledMessage message, GameManager state)
        {
            var player = message.Entity.Player;
            if (player != null)
            {
                Debug.Assert(message.Successful);
                player.NextActionTick += message.Delay;
                return MessageProcessingResult.ContinueProcessing;
            }

            if (!message.Successful)
            {
                return MessageProcessingResult.ContinueProcessing;
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        private GameEntity GetItem(int itemId, GameEntity playerEntity, GameManager manager)
        {
            var itemEntity = manager.FindEntity(itemId);
            if (itemEntity == null
                || itemEntity.Item.ContainerId != playerEntity.Id)
            {
                throw new InvalidOperationException("Invalid item " + itemId);
            }

            return itemEntity;
        }

        private bool ActivateAbility(
            GameEntity abilityEntity, GameEntity playerEntity, int? target, GameManager manager)
        {
            if (target == null)
            {
                return ActivateAbility(abilityEntity, playerEntity, playerEntity.Position.LevelCell, playerEntity, manager);
            }

            Point targetCell;
            GameEntity targetActor;
            if (target.Value < 0)
            {
                targetActor = manager.FindEntity(-target.Value);
                if (targetActor == null
                    || !targetActor.Being.IsAlive)
                {
                    return false;
                }

                targetCell = targetActor.Position.LevelCell;
            }
            else
            {
                targetCell = Point.Unpack(target).Value;
                targetActor = manager.LevelActorToLevelCellIndex[(playerEntity.Position.LevelId, targetCell.X, targetCell.Y)];
            }

            return ActivateAbility(abilityEntity, playerEntity, targetCell, targetActor, manager);
        }

        private bool ActivateAbility(GameEntity abilityEntity, GameEntity playerEntity,
            Point targetCell, GameEntity targetEntity, GameManager manager)
        {
            var ability = abilityEntity.Ability;
            if ((ability.Activation & (ActivationType.Targeted | ActivationType.ManualActivation)) == 0
                || !ability.IsUsable)
            {
                throw new InvalidOperationException("Ability " + abilityEntity.Id + " cannot be used.");
            }

            var position = playerEntity.Position;
            var shouldMoveCloser = false;
            if ((ability.Activation & ActivationType.ManualActivation) == 0)
            {
                switch (ability.TargetingType)
                {
                    case TargetingType.AdjacentSingle:
                    case TargetingType.AdjacentArc:
                        shouldMoveCloser = position.LevelCell.DistanceTo(targetCell) > 1;
                        break;
                    case TargetingType.Projectile:
                    case TargetingType.GuidedProjectile:
                    case TargetingType.LineOfSight:
                    case TargetingType.Beam:
                        // TODO: Check LOS and move closer if none
                        break;
                    default:
                        throw new InvalidOperationException($"Targeting type ${ability.TargetingType.ToString()} not supported");
                }
            }

            if (shouldMoveCloser)
            {
                if (!Move(targetCell, playerEntity, manager))
                {
                    return false;
                }

                var player = playerEntity.Player;
                player.NextAction = PlayerAction.UseAbilitySlot;
                player.NextActionTarget = ability.Slot;
                player.NextActionTarget2 = (-targetEntity?.Id) ?? targetCell.ToInt32();
                player.QueuedAction = true;

                return true;
            }

            var activationMessage = manager.AbilityActivationSystem.CreateActivateAbilityMessage(manager);
            activationMessage.AbilityEntity = abilityEntity;
            activationMessage.ActivatorEntity = playerEntity;
            activationMessage.TargetEntity = targetEntity;

            if (!manager.AbilityActivationSystem.CanActivateAbility(activationMessage))
            {
                manager.Queue.ReturnMessage(activationMessage);
                return false;
            }

            manager.Enqueue(activationMessage);
            return true;
        }

        public MessageProcessingResult Process(AbilityActivatedMessage message, GameManager manager)
        {
            // TODO: show a message on fail
            if (message.SuccessfulActivation
                && message.Delay != 0)
            {
                var player = message.ActivatorEntity.Player;
                if (player != null)
                {
                    player.NextActionTick += message.Delay;
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(ItemEquippedMessage message, GameManager manager)
        {
            // TODO: show a message on fail
            if (message.Successful
                && message.Delay != 0)
            {
                var player = message.ActorEntity.Player;
                if (player != null)
                {
                    player.NextActionTick += message.Delay;
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(ItemMovedMessage message, GameManager manager)
        {
            // TODO: show a message on fail
            if (message.Successful
                && message.Delay != 0)
            {
                var player = message.InitialContainer?.Player;
                if (player == null)
                {
                    var finalContainderId = message.ItemEntity.Item.ContainerId;
                    if (finalContainderId != null)
                    {
                        player = manager.FindEntity(finalContainderId.Value).Player;
                    }
                }

                if (player != null)
                {
                    player.NextActionTick += message.Delay;
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(DiedMessage message, GameManager state)
        {
            var player = message.BeingEntity.Player;
            if (player != null)
            {
                player.NextActionTick = null;
            }

            return MessageProcessingResult.ContinueProcessing;
        }
    }
}
