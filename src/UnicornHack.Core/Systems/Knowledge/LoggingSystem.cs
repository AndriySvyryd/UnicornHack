using UnicornHack.Services.LogEvents;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Knowledge;

/// <summary>
///     Writes messages to the player log
/// </summary>
public class LoggingSystem :
    IGameSystem<ItemMovedMessage>,
    IGameSystem<ItemEquippedMessage>,
    IGameSystem<AttackResultMessage>,
    IGameSystem<ItemActivationResultMessage>,
    IGameSystem<TraveledMessage>,
    IGameSystem<DiedMessage>
{
    // TODO: Write to log for first encounter with a creature or item

    public void WriteFailedAction(
        GameEntity actorEntity, ActorActionType action, GameManager manager,
        Direction? direction = null, AbilityComponent? ability = null)
    {
        foreach (var playerEntity in manager.Players)
        {
            var actorSensed = manager.SensorySystem.CanSense(playerEntity, actorEntity) ?? SenseType.None;
            if (!actorSensed.CanIdentify())
            {
                continue;
            }

            var logMessage = manager.Game.Services.Language.GetString(
                new ActorActionFailedEvent(
                    playerEntity, actorEntity, actorSensed, action, direction, ability));
            WriteLog(logMessage, playerEntity, manager);
        }
    }

    public MessageProcessingResult Process(TraveledMessage message, GameManager manager)
    {
        if (message.Successful)
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        var actorEntity = message.Entity;
        if (actorEntity.Player is { } player)
        {
            if (player.PendingChoice != null)
            {
                return MessageProcessingResult.ContinueProcessing;
            }

            var language = manager.Game.Services.Language;
            var statusAbility = !actorEntity.Being!.CanMove
                ? EffectApplicationSystem.GetAbilitiesAffectingProperty(
                        actorEntity, nameof(BeingComponent.CanMove), AbilityType.Debuff)
                    .FirstOrDefault()
                : null;
            if (statusAbility != null)
            {
                WriteLog(language.StatusPreventsMovement(statusAbility), actorEntity, manager);
                return MessageProcessingResult.ContinueProcessing;
            }

            if (message.Delay == 0)
            {
                WriteLog(language.CannotMove(message.TargetHeading), actorEntity, manager);
                return MessageProcessingResult.ContinueProcessing;
            }
        }
        else if (actorEntity.AI == null)
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        WriteFailedAction(
            actorEntity,
            message.TargetCell == message.InitialLevelCell
                ? ActorActionType.ChangeHeading
                : ActorActionType.MoveOneCell,
            manager,
            message.TargetHeading);
        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(ItemMovedMessage message, GameManager manager)
    {
        if (!message.Successful)
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        var itemEntity = message.ItemEntity;
        var item = itemEntity.Item!;
        var position = itemEntity.Position;
        foreach (var playerEntity in manager.Players)
        {
            var finalSensed = manager.SensorySystem.CanSense(playerEntity, itemEntity) ?? SenseType.None;

            var initialTopContainer = message.InitialContainer != null
                ? manager.ItemMovingSystem.GetTopContainer(message.InitialContainer, manager)
                : itemEntity;
            var initialPosition = message.InitialRoomCell ?? initialTopContainer.Position?.Cell;
            var initialRoomId = message.InitialRoomEntity?.Id ?? initialTopContainer.Position?.RoomId;
            var initialSensed = initialPosition != null
                && playerEntity.Position!.RoomId == initialRoomId
                ? manager.SensorySystem.CanSense(playerEntity, initialTopContainer, initialPosition.Value)
                : SenseType.None;

            if (initialSensed == SenseType.None
                && finalSensed == SenseType.None)
            {
                continue;
            }

            string? logMessage = null;
            if (message.InitialContainer != null)
            {
                if (position != null)
                {
                    if (!message.SuppressLog)
                    {
                        if (playerEntity == message.InitialContainer)
                        {
                            finalSensed |= SenseType.Telepathy;
                        }

                        logMessage = manager.Game.Services.Language.GetString(new ItemDropEvent(
                            playerEntity, message.InitialContainer, message.ItemEntity,
                            message.InitialCount, initialSensed, finalSensed));
                    }
                }
                else if (item.ContainerId != null)
                {
                    // Changed container
                }
            }
            else if (message.InitialRoomCell != null)
            {
                if (item.ContainerId != null)
                {
                    logMessage = manager.Game.Services.Language.GetString(new ItemPickUpEvent(
                        playerEntity, manager.ItemMovingSystem.GetTopContainer(itemEntity, manager),
                        message.ItemEntity, message.InitialCount, finalSensed, initialSensed));
                }
                else if (position != null)
                {
                    // Moved across
                }
            }

            WriteLog(logMessage, playerEntity, manager);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(ItemEquippedMessage message, GameManager manager)
    {
        if (message.Successful)
        {
            foreach (var playerEntity in manager.Players)
            {
                var actorSensed = manager.SensorySystem.CanSense(playerEntity, message.ActorEntity) ??
                                  SenseType.None;
                if (message.SuppressLog
                    || !message.ActorEntity.Being!.IsAlive
                    || actorSensed == SenseType.None)
                {
                    continue;
                }

                var itemSensed = manager.SensorySystem.CanSense(playerEntity, message.ItemEntity) ?? SenseType.None;

                var logMessage = manager.Game.Services.Language.GetString(new ItemEquipmentEvent(
                    playerEntity, message.ActorEntity, message.ItemEntity,
                    actorSensed, itemSensed, message.Slot));
                WriteLog(logMessage, playerEntity, manager);
            }
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(AttackResultMessage message, GameManager manager)
    {
        foreach (var playerEntity in manager.Players)
        {
            var targetEntity = message.TargetEntity;
            var attackerSensed = manager.SensorySystem.CanSense(playerEntity, message.ActivatorEntity) ??
                                 SenseType.None;
            var victimSensed = targetEntity == null
                ? SenseType.None
                : manager.SensorySystem.CanSense(playerEntity, targetEntity) ?? SenseType.None;
            if (attackerSensed == SenseType.None
                && victimSensed == SenseType.None)
            {
                continue;
            }

            var logMessage = manager.Game.Services.Language.GetString(new AttackEvent(
                playerEntity, message.ActivatorEntity, targetEntity,
                attackerSensed, victimSensed, message.ResourceChanges, message.AbilityAction,
                message.WeaponEntity, message.Ranged, message.Hit, message.AnyEffectApplied));
            WriteLog(logMessage, playerEntity, manager);

            if (targetEntity != null
                && message.LingeringEffects.Count != 0
                && (attackerSensed.CanIdentify()
                    || victimSensed.CanIdentify()))
            {
                var lingeringMessage = manager.Game.Services.Language.GetString(
                    new LingeringAttackEvent(
                        playerEntity, message.ActivatorEntity, targetEntity,
                        attackerSensed, victimSensed, message.LingeringEffects));
                WriteLog(lingeringMessage, playerEntity, manager);
            }
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(ItemActivationResultMessage message, GameManager manager)
    {
        foreach (var playerEntity in manager.Players)
        {
            var activatorSensed = manager.SensorySystem.CanSense(playerEntity, message.ActivatorEntity) ??
                                  SenseType.None;
            var targetSensed = manager.SensorySystem.CanSense(playerEntity, message.TargetEntity) ??
                               SenseType.None;

            if (activatorSensed == SenseType.None
                && targetSensed == SenseType.None)
            {
                continue;
            }

            var itemSensed = playerEntity == message.ActivatorEntity
                ? activatorSensed
                : manager.SensorySystem.CanSense(playerEntity, message.ItemEntity) ?? activatorSensed;
            var logMessage = manager.Game.Services.Language.GetString(new ItemActivationEvent(
                playerEntity, message.ItemEntity, message.ActivatorEntity, message.TargetEntity, itemSensed,
                activatorSensed, targetSensed, message.ItemConsumed, message.Successful));
            WriteLog(logMessage, playerEntity, manager);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(DiedMessage message, GameManager manager)
    {
        foreach (var playerEntity in manager.Players)
        {
            var deceasedSensed =
                manager.SensorySystem.CanSense(playerEntity, message.BeingEntity) ?? SenseType.None;
            if (deceasedSensed == SenseType.None)
            {
                continue;
            }

            var logMessage = manager.Game.Services.Language.GetString(
                new DeathEvent(playerEntity, message.BeingEntity, deceasedSensed));
            WriteLog(logMessage, playerEntity, manager);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public void WriteLog(
        string? message, GameEntity playerEntity, GameManager manager,
        LogEntryImportance importance = LogEntryImportance.Default)
    {
        if (message == null)
        {
            return;
        }

        var entry = new LogEntry
        {
            Id = ++manager.Game.NextLogId,
            Message = message,
            PlayerId = playerEntity.Id,
            Tick = manager.Game.CurrentTick,
            Importance = importance
        };

        playerEntity.Player!.LogEntries.Add(entry);
    }
}
