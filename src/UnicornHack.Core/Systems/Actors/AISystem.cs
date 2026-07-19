using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
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
        var ai = actorEntity.AI!;

        Debug.Assert(actorEntity.Being!.IsAlive);

        ProcessTimedEffects(actorEntity, ai, manager);
        if (!actorEntity.Being.IsAlive)
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        if (!CanAct(actorEntity, manager))
        {
            ResetNextAction(ai);
            ai.NextAction = ActorActionType.Wait;
            ai.NextActionTick += TimeSystem.DefaultActionDelay;
            return MessageProcessingResult.ContinueProcessing;
        }

        var action = ai.NextAction;
        var target = ai.NextActionTarget;
        var target2 = ai.NextActionTarget2;
        if (action == null)
        {
            ai.NextActionTick += TimeSystem.DefaultActionDelay / 2;
            DecideNextAction(actorEntity, manager);
            return MessageProcessingResult.ContinueProcessing;
        }

        var performedAction = action.Value;
        if (!IsAllowedWhileTaunted(actorEntity, performedAction))
        {
            performedAction = ActorActionType.Wait;
        }

        RandomizeActionTargetIfConfused(
            actorEntity, ref performedAction, ref target, ref target2, manager);
        EnqueueActionNoise(actorEntity, performedAction, target, manager);
        ResetNextAction(ai);

        var position = actorEntity.Position;
        switch (performedAction)
        {
            case ActorActionType.Wait:
                ai.NextActionTick += TimeSystem.DefaultActionDelay;
                break;
            case ActorActionType.ChangeHeading:
            case ActorActionType.MoveOneCell:
                var moveDirection = (Direction)target!.Value;
                Move(moveDirection, actorEntity, manager,
                    onlyChangeHeading: performedAction == ActorActionType.ChangeHeading);

                break;
            case ActorActionType.DropItem:
                var itemToDrop = GetItem(target!.Value, actorEntity, manager)!;
                var dropMessage = MoveItemMessage.Create(manager);
                dropMessage.ItemEntity = itemToDrop;
                dropMessage.TargetRoomEntity = position!.RoomEntity;
                dropMessage.TargetCell = position.Cell;

                manager.Enqueue(dropMessage);
                break;
            case ActorActionType.EquipItem:
                var itemToEquip = GetItem(target!.Value, actorEntity, manager)!;
                var slot = (EquipmentSlot)target2!.Value;
                var equipMessage = EquipItemMessage.Create(manager);
                equipMessage.ActorEntity = actorEntity;
                equipMessage.ItemEntity = itemToEquip;
                equipMessage.Slot = slot;

                manager.Enqueue(equipMessage);
                break;
            case ActorActionType.UnequipItem:
                var itemToUnequip = GetItem(target!.Value, actorEntity, manager)!;
                var unequipMessage = EquipItemMessage.Create(manager);
                unequipMessage.ActorEntity = actorEntity;
                unequipMessage.ItemEntity = itemToUnequip;
                unequipMessage.Slot = EquipmentSlot.None;

                manager.Enqueue(unequipMessage);
                break;
            case ActorActionType.UseAbilitySlot:
                ActivateAbility(actorEntity, target, target2, manager);
                break;
            case ActorActionType.MoveToCell:
            case ActorActionType.SetAbilitySlot:
            default:
                throw new InvalidOperationException(
                    $"Action {performedAction} on actor {actorEntity.Id} is invalid.");
        }

        AddRandomMove(actorEntity, manager);
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
        if (EffectApplicationSystem.GetAbilityLevel(actorEntity, AbilityData.Engulfed.Name) >= 2)
        {
            // Engulfed: the only permitted attack is against whatever holds the actor.
            if (manager.EngulferToVictimRelationship.GetEngulfer(actorEntity) is { } engulfer
                && manager.AbilitySlottingSystem.GetAbility(
                    actorEntity, AbilitySlottingSystem.DefaultMeleeAttackSlot) != null)
            {
                actorEntity.AI!.NextAction = ActorActionType.UseAbilitySlot;
                actorEntity.AI.NextActionTarget = AbilitySlottingSystem.DefaultMeleeAttackSlot;
                actorEntity.AI.NextActionTarget2 = -engulfer.Id;
            }
            else
            {
                actorEntity.AI!.NextAction = ActorActionType.Wait;
            }

            return;
        }

        var position = actorEntity.Position!;

        // Stay dormant while no player can observe this room.
        if (!manager.PlayerSystem.IsPlayerInRoom(position.RoomId, manager))
        {
            actorEntity.AI!.NextAction = ActorActionType.Wait;
            return;
        }

        if (TryFleeFearSource(actorEntity, position, manager))
        {
            return;
        }

        if (TryAttackPlayerCharacter(actorEntity, manager))
        {
            return;
        }

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

    private static bool TryFleeFearSource(
        GameEntity actorEntity, PositionComponent position, GameManager manager)
    {
        var frightenedLevel = EffectApplicationSystem.GetAbilityLevel(actorEntity, AbilityData.Frightened.Name);
        if (frightenedLevel < 2
            || FindPerceivedStatusSource(actorEntity, EffectType.Frighten, requiredLevel: 2, manager)
                is not { Position.Cell: { } sourceCell })
        {
            return false;
        }

        if (frightenedLevel >= 3)
        {
            actorEntity.AI!.NextAction = ActorActionType.MoveOneCell;
            actorEntity.AI.NextActionTarget = (int)(GetPanicDirection(position, sourceCell, manager)
                                                     ?? position.Heading!.Value);
            return true;
        }

        var currentCell = position.Cell!.Value;
        var currentDistance = currentCell.DistanceTo(sourceCell);
        Direction? fleeDirection = null;
        var fleeDistance = currentDistance;
        Span<Direction> directions = stackalloc Direction[8];
        var directionCount = manager.TravelSystem.GetPossibleMovementDirections(
            position, safe: true, directions);
        for (var i = 0; i < directionCount; i++)
        {
            var direction = directions[i];
            var distance = currentCell.Translate(direction.AsVector()).DistanceTo(sourceCell);
            if (distance > fleeDistance)
            {
                fleeDirection = direction;
                fleeDistance = distance;
            }
        }

        if (fleeDirection == null)
        {
            actorEntity.AI!.NextAction = ActorActionType.Wait;
            return true;
        }

        actorEntity.AI!.NextAction = position.Heading == fleeDirection
            ? ActorActionType.MoveOneCell
            : ActorActionType.ChangeHeading;
        actorEntity.AI.NextActionTarget = (int)fleeDirection.Value;
        return true;
    }

    private static void ResetNextAction(AIComponent ai)
    {
        ai.NextAction = null;
        ai.NextActionTarget = null;
        ai.NextActionTarget2 = null;
    }

    private bool TryAttackPlayerCharacter(GameEntity aiEntity, GameManager manager)
    {
        var aiPosition = aiEntity.Position!;
        var aiCell = aiPosition.Cell!.Value;
        foreach (var playerEntity in manager.Players)
        {
            var playerPosition = playerEntity.Position!;
            if (playerPosition.Cell is not { } playerCell
                || playerPosition.RoomId != aiPosition.RoomId
                || playerCell.DistanceTo(aiCell) > 1
                || !playerEntity.Being!.IsAlive)
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

        var ai = aiEntity.AI!;
        ai.NextAction = ActorActionType.UseAbilitySlot;
        ai.NextActionTarget = abilityEntity.Ability!.Slot;
        ai.NextActionTarget2 = targetEntity.Position!.Cell!.Value.ToInt32();

        return true;
    }

    public GameEntity? GetDefaultAttack(GameEntity aiEntity, GameEntity targetEntity, bool? melee,
        GameManager manager)
    {
        foreach (var abilityEntity in aiEntity.ActiveEffects!.Abilities)
        {
            var ability = abilityEntity.Ability!;
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
            if (manager.AbilityActivationSystem.CanActivateAbility(activationMessage) != null)
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
        var aiPosition = actorEntity.Position!;
        var aiCell = aiPosition.Cell!.Value;
        foreach (var playerEntity in manager.Players)
        {
            var playerPosition = playerEntity.Position!;
            if (playerPosition.Cell is not { } playerCell
                || playerPosition.RoomId != aiPosition.RoomId
                || playerCell.DistanceTo(aiCell) > 8
                || !playerEntity.Being!.IsAlive)
            {
                continue;
            }

            var room = aiPosition.RoomEntity.Room!;

            // TODO: Check memory and senses
            //if (manager.SensorySystem.GetVisibility(aiEntity, playerPosition.Cell, room) == 0)
            //{
            //    continue;
            //}

            // TODO: Avoid connections
            var directionToMove = manager.TravelSystem.GetFirstStepFromShortestPath(
                room, aiCell, playerCell, aiPosition.Heading!.Value, avoidActors: true);

            if (directionToMove != null)
            {
                var changeHeading = position.Heading != directionToMove;
                var targetCell = changeHeading
                    ? aiCell
                    : aiCell.Translate(directionToMove.Value.AsVector());
                if (!changeHeading
                    && targetCell == playerCell)
                {
                    // No need to move
                    return true;
                }

                var travelMessage = TravelMessage.Create(manager);
                travelMessage.ActorEntity = actorEntity;
                travelMessage.TargetHeading = directionToMove.Value;
                travelMessage.Distance = changeHeading ? 0 : 1;

                if (manager.TravelSystem.CanTravel(travelMessage, manager))
                {
                    var ai = actorEntity.AI!;
                    ai.NextAction = changeHeading ? ActorActionType.ChangeHeading : ActorActionType.MoveOneCell;
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
        Span<Direction> possibleDirectionsToMove = stackalloc Direction[8];
        var directionCount = manager.TravelSystem.GetPossibleMovementDirections(
            position, safe: true, possibleDirectionsToMove);
        if (directionCount == 0)
        {
            return;
        }

        Direction? directionToMove;
        if (possibleDirectionsToMove[..directionCount].Contains(position.Heading!.Value)
            && manager.Game.Random.NextBool())
        {
            directionToMove = position.Heading.Value;
        }
        else
        {
            var directionIndex = manager.Game.Random.Next(0, directionCount);
            directionToMove = possibleDirectionsToMove[directionIndex];
        }

        var changeHeading = position.Heading != directionToMove;
        var ai = actorEntity.AI!;
        ai.NextAction = changeHeading ? ActorActionType.ChangeHeading : ActorActionType.MoveOneCell;
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
        // The ability can already be gone if it was torn down in the same tick it was added.
        if (message.Entity.Ability is { } ability)
        {
            TrySlot(ability, manager);
        }

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
        var owner = ability.OwnerEntity!;
        if (owner.AI == null
            || (ability.Activation & ActivationType.Slottable) == 0
            || ability.Slot != null
            || !ability.IsUsable
            || ability.Type.IsDefaultAttack())
        {
            return;
        }

        var i = manager.AbilitySlottingSystem.GetFirstFreeSlot(owner);
        if (i == null || !(i < owner.Physical!.Capacity - 1))
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
