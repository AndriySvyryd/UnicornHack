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
        ActorSystemBase,
        IGameSystem<PerformActionMessage>,
        IGameSystem<DiedMessage>,
        IGameSystem<DelayMessage>,
        IGameSystem<TraveledMessage>
    {
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

            Debug.Assert(playerEntity.Being.IsAlive);

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

            ResetNextAction(player);

            var position = playerEntity.Position;
            switch (action)
            {
                case ActorAction.Wait:
                    player.NextActionTick += TimeSystem.DefaultActionDelay;
                    break;
                case ActorAction.ChangeHeading:
                case ActorAction.MoveOneCell:
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
                                onlyChangeHeading: action == ActorAction.ChangeHeading);
                            break;
                    }

                    break;
                case ActorAction.MoveToCell:
                    var targetCell = Point.Unpack(target).Value;

                    if (position.LevelCell.DistanceTo(targetCell) == 1)
                    {
                        return Move(position.LevelCell.DifferenceTo(targetCell).AsDirection(), playerEntity, manager);
                    }
                    else
                    {
                        if (!Move(targetCell, playerEntity, manager))
                        {
                            return false;
                        }

                        if (position.LevelCell != targetCell)
                        {
                            player.NextAction = ActorAction.MoveToCell;
                            player.NextActionTarget = target;
                            player.QueuedAction = true;
                        }
                    }

                    break;
                case ActorAction.DropItem:
                    var itemToDrop = GetItem(target.Value, playerEntity, manager);
                    if (itemToDrop == null)
                    {
                        return false;
                    }

                    var dropMessage = MoveItemMessage.Create(manager);
                    dropMessage.ItemEntity = itemToDrop;
                    dropMessage.TargetLevelEntity = position.LevelEntity;
                    dropMessage.TargetCell = position.LevelCell;

                    manager.Enqueue(dropMessage);
                    break;
                case ActorAction.EquipItem:
                    var itemToEquip = GetItem(target.Value, playerEntity, manager);
                    var slot = (EquipmentSlot)target2.Value;
                    if (itemToEquip == null)
                    {
                        return false;
                    }

                    var equipMessage = EquipItemMessage.Create(manager);
                    equipMessage.ActorEntity = playerEntity;
                    equipMessage.ItemEntity = itemToEquip;
                    equipMessage.Slot = slot;
                    equipMessage.Force = true;

                    manager.Enqueue(equipMessage);
                    break;
                case ActorAction.UnequipItem:
                    var itemToUnequip = GetItem(target.Value, playerEntity, manager);
                    if (itemToUnequip == null)
                    {
                        return false;
                    }

                    var unequipMessage = EquipItemMessage.Create(manager);
                    unequipMessage.ActorEntity = playerEntity;
                    unequipMessage.ItemEntity = itemToUnequip;
                    unequipMessage.Slot = EquipmentSlot.None;

                    manager.Enqueue(unequipMessage);
                    break;
                case ActorAction.SetAbilitySlot:
                    var setSlotMessage = SetAbilitySlotMessage.Create(manager);
                    setSlotMessage.AbilityEntity = manager.FindEntity(target);
                    setSlotMessage.OwnerEntity = playerEntity;
                    setSlotMessage.Slot = target2;

                    manager.Enqueue(setSlotMessage);
                    break;
                case ActorAction.UseAbilitySlot:
                    ActivateAbility(playerEntity, target, target2, manager);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Action {action} on character {playerEntity.Player.ProperName} is invalid.");
            }

            return true;
        }

        private static void ResetNextAction(PlayerComponent player)
        {
            player.NextAction = null;
            player.NextActionTarget = null;
            player.NextActionTarget2 = null;
            player.QueuedAction = false;
        }

        protected override bool ActivateAbility(
            GameEntity abilityEntity, GameEntity actorEntity, Point targetCell, GameEntity targetEntity, GameManager manager)
        {
            var ability = abilityEntity.Ability;
            var position = actorEntity.Position;
            if ((ability.Activation & ActivationType.Manual) == 0)
            {
                // TODO: Also check LOS and move closer if none
                if (position.LevelCell.DistanceTo(targetCell) > ability.Range)
                {
                    if (!Move(targetCell, actorEntity, manager))
                    {
                        return false;
                    }

                    var player = actorEntity.Player;
                    player.NextAction = ActorAction.UseAbilitySlot;
                    player.NextActionTarget = ability.Slot;
                    player.NextActionTarget2 = (-targetEntity?.Id) ?? targetCell.ToInt32();
                    player.QueuedAction = true;

                    return true;
                }
            }

            return base.ActivateAbility(abilityEntity, actorEntity, targetCell, targetEntity, manager);
        }

        private bool Move(Point targetCell, GameEntity playerEntity, GameManager manager)
        {
            // TODO: avoid actors and connections
            var position = playerEntity.Position;
            var direction = manager.TravelSystem.GetFirstStepFromShortestPath(
                position.LevelEntity.Level, position.LevelCell, targetCell, position.Heading.Value, knownOnly: true);
            if (direction == null)
            {
                manager.LoggingSystem.WriteLog(manager.Game.Services.Language.NoPath(), playerEntity, manager);
                return false;
            }

            return Move(direction.Value, playerEntity, manager);
        }

        public MessageProcessingResult Process(TraveledMessage message, GameManager state)
        {
            var player = message.Entity.Player;
            if (!message.Successful
                && player != null)
            {
                var manager = message.Entity.Manager;
                manager.LoggingSystem.WriteLog(
                    manager.Game.Services.Language.UnableToMove(message.TargetHeading), message.Entity, manager);
                ResetNextAction(player);

                if (manager.Game.ActingPlayer == null)
                {
                    manager.Game.ActingPlayer = message.Entity;
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(DelayMessage message, GameManager state)
        {
            var player = message.ActorEntity.Player;
            if (player?.NextActionTick != null)
            {
                player.NextActionTick += message.Delay;
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
