using System;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Primitives;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Time;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Actors;

public class AISystem :
    ActorSystemBase,
    IGameSystem<PerformActionMessage>,
    IGameSystem<DecideNextActionMessage>,
    IGameSystem<DiedMessage>,
    IGameSystem<EntityAddedMessage<GameEntity>>,
    IGameSystem<PropertyValueChangedMessage<GameEntity, bool>>,
    IGameSystem<DelayMessage>
{
    public MessageProcessingResult Process(PerformActionMessage message, GameManager manager)
    {
        var actorEntity = message.Actor;
        var ai = actorEntity.AI;

        Debug.Assert(actorEntity.Being.IsAlive);

        var action = ai.NextAction;
        var target = ai.NextActionTarget;
        var target2 = ai.NextActionTarget2;
        if (action == null)
        {
            ai.NextActionTick += TimeSystem.DefaultActionDelay / 2;
            DecideNextAction(actorEntity, manager);
            return MessageProcessingResult.ContinueProcessing;
        }

        ResetNextAction(ai);

        // TODO: If can't act - wait till player turn

        var position = actorEntity.Position;
        switch (action)
        {
            case ActorAction.Wait:
                ai.NextActionTick += TimeSystem.DefaultActionDelay;
                break;
            case ActorAction.ChangeHeading:
            case ActorAction.MoveOneCell:
                var moveDirection = (Direction)target.Value;
                Move(moveDirection, actorEntity, manager,
                    onlyChangeHeading: action == ActorAction.ChangeHeading);

                break;
            case ActorAction.DropItem:
                var itemToDrop = GetItem(target.Value, actorEntity, manager);
                if (itemToDrop == null)
                {
                    return MessageProcessingResult.ContinueProcessing;
                }

                var dropMessage = MoveItemMessage.Create(manager);
                dropMessage.ItemEntity = itemToDrop;
                dropMessage.TargetLevelEntity = position.LevelEntity;
                dropMessage.TargetCell = position.LevelCell;

                manager.Enqueue(dropMessage);
                break;
            case ActorAction.EquipItem:
                var itemToEquip = GetItem(target.Value, actorEntity, manager);
                var slot = (EquipmentSlot)target2.Value;
                if (itemToEquip == null)
                {
                    return MessageProcessingResult.ContinueProcessing;
                }

                var equipMessage = EquipItemMessage.Create(manager);
                equipMessage.ActorEntity = actorEntity;
                equipMessage.ItemEntity = itemToEquip;
                equipMessage.Slot = slot;

                manager.Enqueue(equipMessage);
                break;
            case ActorAction.UnequipItem:
                var itemToUnequip = GetItem(target.Value, actorEntity, manager);
                if (itemToUnequip == null)
                {
                    return MessageProcessingResult.ContinueProcessing;
                }

                var unequipMessage = EquipItemMessage.Create(manager);
                unequipMessage.ActorEntity = actorEntity;
                unequipMessage.ItemEntity = itemToUnequip;
                unequipMessage.Slot = EquipmentSlot.None;

                manager.Enqueue(unequipMessage);
                break;
            case ActorAction.UseAbilitySlot:
                ActivateAbility(actorEntity, target, target2, manager);
                break;
            case ActorAction.MoveToCell:
            case ActorAction.SetAbilitySlot:
            default:
                throw new InvalidOperationException(
                    $"Action {action} on actor {actorEntity.Id} is invalid.");
        }

        DecideNextActionMessage.Enqueue(actorEntity, manager);
        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(DecideNextActionMessage message, GameManager manager)
    {
        DecideNextAction(message.Actor, manager);
        return MessageProcessingResult.ContinueProcessing;
    }

    private void DecideNextAction(GameEntity actorEntity, GameManager manager)
    {
        if (TryAttackPlayerCharacter(actorEntity, manager))
        {
            return;
        }

        var position = actorEntity.Position;
        if (position.MovementDelay == 0)
        {
            return;
        }

        if (TryMoveToPlayer(actorEntity, position, manager))
        {
            return;
        }

        Wander(actorEntity, position, manager);
    }

    private static void ResetNextAction(AIComponent ai)
    {
        ai.NextAction = null;
        ai.NextActionTarget = null;
        ai.NextActionTarget2 = null;
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

            if (TryAttack(aiEntity, playerEntity, manager))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryAttack(GameEntity aiEntity, GameEntity targetEntity, GameManager manager)
    {
        var abilityEntity = GetDefaultAttack(aiEntity, targetEntity, null, manager);
        if (abilityEntity == null)
        {
            return false;
        }

        var ai = aiEntity.AI;
        ai.NextAction = ActorAction.UseAbilitySlot;
        ai.NextActionTarget = abilityEntity.Ability.Slot;
        ai.NextActionTarget2 = targetEntity.Position.LevelCell.ToInt32();

        return true;
    }

    public GameEntity GetDefaultAttack(GameEntity aiEntity, GameEntity targetEntity, bool? melee,
        GameManager manager)
    {
        foreach (var abilityEntity in aiEntity.Physical.Abilities)
        {
            var ability = abilityEntity.Ability;
            if (ability.Activation != ActivationType.Targeted
                || !ability.IsUsable
                || melee == (ability.Range != 1))
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

            manager.Queue.ReturnMessage(activationMessage);
            return abilityEntity;
        }

        return null;
    }

    private bool TryMoveToPlayer(GameEntity actorEntity, PositionComponent position, GameManager manager)
    {
        var aiPosition = actorEntity.Position;
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
            var directionToMove = manager.TravelSystem.GetFirstStepFromShortestPath(
                level, aiPosition.LevelCell, playerPosition.LevelCell, aiPosition.Heading.Value);

            if (directionToMove != null)
            {
                var changeHeading = position.Heading != directionToMove;
                var targetCell = changeHeading
                    ? position.LevelCell
                    : position.LevelCell.Translate(directionToMove.Value.AsVector());
                if (!changeHeading
                    && targetCell == playerPosition.LevelCell)
                {
                    // No need to move
                    return true;
                }

                var travelMessage = TravelMessage.Create(manager);
                travelMessage.ActorEntity = actorEntity;
                travelMessage.TargetCell = targetCell;
                travelMessage.TargetHeading = directionToMove.Value;

                if (manager.TravelSystem.CanTravel(travelMessage, manager))
                {
                    var ai = actorEntity.AI;
                    ai.NextAction = changeHeading ? ActorAction.ChangeHeading : ActorAction.MoveOneCell;
                    ai.NextActionTarget = (int)directionToMove.Value;
                    manager.Queue.ReturnMessage(travelMessage);
                    return true;
                }

                manager.Queue.ReturnMessage(travelMessage);
            }
        }

        return false;
    }

    private void Wander(GameEntity actorEntity, PositionComponent position, GameManager manager)
    {
        var possibleDirectionsToMove =
            manager.TravelSystem.GetPossibleMovementDirections(position, safe: true);
        if (possibleDirectionsToMove.Count == 0)
        {
            return;
        }

        Direction? directionToMove;
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

        var changeHeading = position.Heading != directionToMove;
        var ai = actorEntity.AI;
        ai.NextAction = changeHeading ? ActorAction.ChangeHeading : ActorAction.MoveOneCell;
        ai.NextActionTarget = (int)directionToMove.Value;
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
            || ability.Type == AbilityType.DefaultAttack)
        {
            return;
        }

        var i = manager.AbilitySlottingSystem.GetFirstFreeSlot(owner);
        if (i == null || !(i < owner.Physical.Capacity - 1))
        {
            // leave a slot for items
            return;
        }

        var setSlotMessage = SetAbilitySlotMessage.Create(manager);
        setSlotMessage.Slot = i;
        setSlotMessage.AbilityEntity = ability.Entity;

        manager.AbilitySlottingSystem.Process(setSlotMessage, manager);
        manager.Queue.ReturnMessage(setSlotMessage);
    }
}
