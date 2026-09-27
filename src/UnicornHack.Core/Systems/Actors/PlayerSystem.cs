using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Time;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Actors;

public class PlayerSystem :
    ActorSystemBase,
    IGameSystem<PerformActionMessage>,
    IGameSystem<DiedMessage>,
    IGameSystem<DelayMessage>,
    IGameSystem<TraveledMessage>
{
    public MessageProcessingResult Process(PerformActionMessage message, GameManager manager)
    {
        var player = message.Actor.Player!;
        ProcessTimedEffects(message.Actor, player, manager);
        if (!message.Actor.Being!.IsAlive)
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        if (player.PendingChoice != null
            || player.NextAction == ActorActionType.SelectChoice)
        {
            ProcessPlayerOptionSelection(message.Actor, player, manager);

            return MessageProcessingResult.ContinueProcessing;
        }

        if (!CanAct(message.Actor, manager))
        {
            if (EffectApplicationSystem.GetAbilitiesAffectingProperty(
                    message.Actor, nameof(BeingComponent.UnableToActPercent), AbilityType.Debuff)
                .FirstOrDefault() is { } statusAbility)
            {
                manager.LoggingSystem.WriteLog(
                    manager.Game.Services.Language.StatusPreventsAction(statusAbility), message.Actor, manager);
            }
            ResetNextAction(player);
            player.NextActionTick += TimeSystem.DefaultActionDelay;
            return MessageProcessingResult.ContinueProcessing;
        }

        ProcessTurn(message, manager);

        return MessageProcessingResult.ContinueProcessing;
    }

    private static bool ProcessPlayerOptionSelection(
        GameEntity playerEntity, PlayerComponent player, GameManager manager)
    {
        var action = player.NextAction;
        var optionIndex = player.NextActionTarget;

        if (action == null)
        {
            return false;
        }

        if (action != ActorActionType.SelectChoice)
        {
            if (!player.QueuedAction)
            {
                manager.LoggingSystem.WriteLog(
                    manager.Game.Services.Language.PlayerChoiceRequired(),
                    playerEntity,
                    manager);
            }

            ResetNextAction(player);
            return false;
        }

        if (optionIndex == null)
        {
            manager.LoggingSystem.WriteLog(
                manager.Game.Services.Language.InvalidPlayerChoiceSelection(),
                playerEntity,
                manager);
            ResetNextAction(player);
            return false;
        }

        ResetNextAction(player);
        var result = manager.PlayerChoiceSystem.Select(
            playerEntity,
            optionIndex.Value,
            manager);
        if (!result.IsAccepted)
        {
            manager.LoggingSystem.WriteLog(
                result.ErrorMessage
                ?? manager.Game.Services.Language.PlayerChoiceApplicationFailed(),
                playerEntity,
                manager);
            return false;
        }

        player.CommandHistory.Add(new PlayerCommand
        {
            GameId = playerEntity.GameId,
            Id = ++playerEntity.Game.NextCommandId,
            PlayerId = playerEntity.Id,
            Tick = manager.Game.CurrentTick,
            Action = action.Value,
            Target = optionIndex,
            Target2 = null
        });

        if (result.Delay == 0)
        {
            return false;
        }

        DelayMessage.Enqueue(playerEntity, result.Delay, manager);
        return true;
    }

    private bool ProcessTurn(PerformActionMessage message, GameManager manager)
    {
        var playerEntity = message.Actor;
        var player = playerEntity.Player!;

        Debug.Assert(playerEntity.Being!.IsAlive);

        // TODO: add an option to stop here and display current state while performing multi-action commands (every x actions)

        var action = player.NextAction;
        var target = player.NextActionTarget;
        var target2 = player.NextActionTarget2;
        if (action == null)
        {
            return false;
        }

        var performedAction = action.Value;

        if (!player.QueuedAction)
        {
            player.CommandHistory.Add(new PlayerCommand
            {
                GameId = playerEntity.GameId,
                Id = ++playerEntity.Game.NextCommandId,
                PlayerId = playerEntity.Id,
                Tick = manager.Game.CurrentTick,
                Action = performedAction,
                Target = target,
                Target2 = target2
            });
        }

        if (!IsAllowedWhileTaunted(playerEntity, performedAction))
        {
            if (manager.AffectableAbilitiesIndex[
                    (playerEntity.Id, AbilityData.Taunted.Name)]?.Ability is { } tauntedAbility)
            {
                manager.LoggingSystem.WriteLog(
                    manager.Game.Services.Language.StatusPreventsAttemptedAction(tauntedAbility),
                    playerEntity, manager);
            }

            performedAction = ActorActionType.Wait;
        } else if (GetPanicDirection(playerEntity, playerEntity.Position!, manager) is { } panicDirection)
        {
            performedAction = ActorActionType.MoveOneCell;
            target = (int)panicDirection;
            target2 = null;
        }
        else
        {
            RandomizeActionTargetIfConfused(playerEntity, ref performedAction, ref target, ref target2, manager);
        }

        if (performedAction is ActorActionType.PickupItem
            or ActorActionType.DropItem
            or ActorActionType.EquipItem
            or ActorActionType.UnequipItem
            && !playerEntity.Being!.CanMoveItems
            && EffectApplicationSystem.GetAbilitiesAffectingProperty(
                    playerEntity, nameof(BeingComponent.CanMoveItems), AbilityType.Debuff)
                .FirstOrDefault() is { } itemStatus)
        {
            manager.LoggingSystem.WriteLog(
                manager.Game.Services.Language.StatusPreventsItemManipulation(itemStatus), playerEntity, manager);
            ResetNextAction(player);
            return false;
        }

        EnqueueActionNoise(playerEntity, performedAction, target, manager);
        ResetNextAction(player);

        var position = playerEntity.Position!;
        switch (performedAction)
        {
            case ActorActionType.Wait:
                player.NextActionTick += TimeSystem.DefaultActionDelay;
                break;
            case ActorActionType.ChangeHeading:
            case ActorActionType.MoveOneCell:
                var moveDirection = (Direction)target!.Value;
                Move(moveDirection, playerEntity, manager,
                    onlyChangeHeading: performedAction == ActorActionType.ChangeHeading);

                break;
            case ActorActionType.MoveToCell:
                var targetCell = Point.Unpack(target)!.Value;

                if (targetCell == position.Cell)
                {
                    // Treat as Wait.
                    player.NextActionTick += TimeSystem.DefaultActionDelay;
                    break;
                }

                if (position.Cell!.Value.DistanceTo(targetCell) == 1)
                {
                    return Move(position.Cell!.Value.DifferenceTo(targetCell).AsDirection(), playerEntity, manager);
                }

                if (!Move(targetCell, playerEntity, manager))
                {
                    return false;
                }

                if (position.Cell != targetCell)
                {
                    player.NextAction = ActorActionType.MoveToCell;
                    player.NextActionTarget = target;
                    player.QueuedAction = true;
                }

                break;
            case ActorActionType.DropItem:
                var itemToDrop = GetItem(target!.Value, playerEntity, manager);
                if (itemToDrop == null)
                {
                    return false;
                }

                var dropMessage = MoveItemMessage.Create(manager);
                dropMessage.ItemEntity = itemToDrop;
                dropMessage.TargetRoomEntity = position.RoomEntity;
                dropMessage.TargetCell = position.Cell;

                manager.Enqueue(dropMessage);
                break;
            case ActorActionType.PickupItem:
                // There can be at most one item per cell, so the action carries no item
                // id; `target`, when supplied, is the ability slot the picked-up item's
                // primary ability should land in. A null target accepts the auto-slot
                // pick from AbilitySlottingSystem (first free slot).
                var itemToPickUp = position.RoomEntity.Room!.Items.GetValueOrDefault(position.Cell!.Value);
                if (itemToPickUp == null)
                {
                    manager.LoggingSystem.WriteLog(
                        manager.Game.Services.Language.NothingToPickUp(), playerEntity, manager);
                    return false;
                }

                var requestedSlot = target;

                // If a specific slot is requested and is already occupied, clear it
                // synchronously up-front so AbilitySlottingSystem's auto-slot lands the
                // new pickup ability directly into the now-free target slot. Without
                // this, the displaced ability would only be unslotted as a side effect
                // of the follow-up SetAbilitySlot below, which works but produces a
                // less intuitive event ordering for animations.
                if (requestedSlot.HasValue
                    && playerEntity.Being!.SlottedAbilities.TryGetValue(requestedSlot.Value, out var conflictingAbility))
                {
                    var clearSlotMessage = SetAbilitySlotMessage.Create(playerEntity, manager);
                    clearSlotMessage.AbilityEntity = conflictingAbility;
                    manager.Process(clearSlotMessage);

                    // Clearing an item ability's slot only *enqueues* the drop of the
                    // displaced item; drain the queue now so the "drop" is logged before
                    // the pickup below logs "pick up". Otherwise the synchronously-processed
                    // pickup would log first and the order would read backwards.
                    manager.Queue.ProcessQueue(manager);
                }

                // Process the pickup synchronously and drain the cascading effects
                // (container change -> availability relationship -> AbilitySlottingSystem auto-slot)
                // so the player's SlottedAbilities
                // map reflects the post-pickup state before we inspect it. The
                // recursive ProcessQueue call is safe: the queue's own secondary
                // drain uses the same recursion, and the parent drain loop sees an
                // empty queue on return and continues normally.
                var slotsBefore = requestedSlot.HasValue
                    ? new HashSet<int>(playerEntity.Being!.SlottedAbilities.Keys)
                    : null;

                var pickupMessage = MoveItemMessage.Create(manager);
                pickupMessage.ItemEntity = itemToPickUp;
                pickupMessage.TargetContainerEntity = playerEntity;

                manager.Process(pickupMessage);
                manager.Queue.ProcessQueue(manager);

                // If a specific slot was requested and the auto-slot still landed the
                // new ability somewhere else (e.g. the requested slot was beyond the
                // first free slot), move it. A pickup adds at most one available item-owned
                // ability, so any new entry in SlottedAbilities relative to
                // `slotsBefore` is the one to move.
                if (requestedSlot.HasValue && slotsBefore != null)
                {
                    foreach (var (pickupSlot, pickupAbilityEntity) in playerEntity.Being!.SlottedAbilities)
                    {
                        if (slotsBefore.Contains(pickupSlot) || pickupSlot == requestedSlot.Value)
                        {
                            continue;
                        }

                        var movePickupToSlotMessage = SetAbilitySlotMessage.Create(playerEntity, manager);
                        movePickupToSlotMessage.AbilityEntity = pickupAbilityEntity;
                        movePickupToSlotMessage.Slot = requestedSlot.Value;
                        manager.Process(movePickupToSlotMessage);
                        break;
                    }
                }
                break;
            case ActorActionType.EquipItem:
                var itemToEquip = GetItem(target!.Value, playerEntity, manager);
                var slot = (EquipmentSlot)target2!.Value;
                if (itemToEquip == null)
                {
                    return false;
                }

                var equipMessage = EquipItemMessage.Create(manager);
                equipMessage.ActorEntity = playerEntity;
                equipMessage.ItemEntity = itemToEquip;
                equipMessage.Slot = slot;

                if (!manager.ItemUsageSystem.CanEquipItem(equipMessage, manager))
                {
                    manager.ReturnMessage(equipMessage);
                    manager.LoggingSystem.WriteFailedAction(
                        playerEntity, ActorActionType.EquipItem, manager);
                    return false;
                }

                manager.Enqueue(equipMessage);
                break;
            case ActorActionType.UnequipItem:
                var itemToUnequip = GetItem(target!.Value, playerEntity, manager);
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
            case ActorActionType.SetAbilitySlot:
                var setSlotMessage = SetAbilitySlotMessage.Create(playerEntity, manager);
                setSlotMessage.AbilityEntity = manager.FindEntity(target);
                setSlotMessage.Slot = target2;

                manager.Enqueue(setSlotMessage);
                break;
            case ActorActionType.UseAbilitySlot:
                ActivateAbility(playerEntity, target, target2, manager);
                break;
            case ActorActionType.TravelToRoom:
                if (!playerEntity.Being!.CanMove
                    && EffectApplicationSystem.GetAbilitiesAffectingProperty(
                            playerEntity, nameof(BeingComponent.CanMove), AbilityType.Debuff)
                        .FirstOrDefault() is { } movementStatus)
                {
                    manager.LoggingSystem.WriteLog(
                        manager.Game.Services.Language.StatusPreventsMovement(movementStatus), playerEntity, manager);
                    return false;
                }

                if (!manager.TravelSystem.TravelToRoom(playerEntity, target!.Value, manager))
                {
                    return false;
                }

                break;
            default:
                throw new InvalidOperationException(
                    $"Action {performedAction} on character {playerEntity.Player!.ProperName} is invalid.");
        }

        AddRandomMove(playerEntity, manager);
        return true;
    }

    private static void ResetNextAction(PlayerComponent player)
    {
        player.NextAction = null;
        player.NextActionTarget = null;
        player.NextActionTarget2 = null;
        player.QueuedAction = false;
    }

    /// <summary>
    ///     Returns <c>true</c> when at least one player occupies the room identified by
    ///     <paramref name="roomId" />.
    /// </summary>
    public bool IsPlayerInRoom(int roomId, GameManager manager)
    {
        foreach (var playerEntity in manager.Players)
        {
            if (playerEntity.Position!.RoomId == roomId)
            {
                return true;
            }
        }

        return false;
    }

    protected override bool ActivateAbility(
        GameEntity abilityEntity, GameEntity actorEntity, Point targetCell, GameEntity? targetEntity,
        GameManager manager)
    {
        var ability = abilityEntity.Ability!;
        var position = actorEntity.Position!;
        if ((ability.Activation & ActivationType.Manual) == 0)
        {
            // TODO: Also check LOS and move closer if none
            if (position.Cell!.Value.DistanceTo(targetCell) > ability.Range)
            {
                if (!Move(targetCell, actorEntity, manager))
                {
                    return false;
                }

                var player = actorEntity.Player!;
                player.NextAction = ActorActionType.UseAbilitySlot;
                player.NextActionTarget = ability.Slot;
                player.NextActionTarget2 = (-targetEntity?.Id) ?? position.Cell.Value.DifferenceTo(targetCell).ToInt32();
                player.QueuedAction = true;

                return true;
            }
        }

        return base.ActivateAbility(abilityEntity, actorEntity, targetCell, targetEntity, manager);
    }

    private bool Move(Point targetCell, GameEntity playerEntity, GameManager manager)
    {
        // TODO: avoid actors and connections
        var position = playerEntity.Position!;
        var direction = manager.TravelSystem.GetFirstStepFromShortestPath(
            position.RoomEntity.Room!, position.Cell!.Value, targetCell, position.Heading!.Value, knownOnly: true);
        if (direction == null)
        {
            manager.LoggingSystem.WriteLog(manager.Game.Services.Language.NoPath(), playerEntity, manager);
            return false;
        }

        return Move(direction.Value, playerEntity, manager);
    }

    public MessageProcessingResult Process(TraveledMessage message, GameManager manager)
    {
        if (!message.Successful
            && message.Entity.Player is { } player)
        {
            ResetNextAction(player);
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
            state.PlayerChoiceSystem.Clear(message.BeingEntity);
            player.NextActionTick = null;
        }

        return MessageProcessingResult.ContinueProcessing;
    }
}
