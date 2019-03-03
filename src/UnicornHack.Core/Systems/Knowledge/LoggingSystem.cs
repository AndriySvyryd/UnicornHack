using System.Linq;
using UnicornHack.Primitives;
using UnicornHack.Services.LogEvents;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Knowledge
{
    /// <summary>
    ///     Writes messages to the player log
    /// </summary>
    public class LoggingSystem :
        IGameSystem<ItemMovedMessage>,
        IGameSystem<ItemEquippedMessage>,
        IGameSystem<EffectsAppliedMessage>,
        IGameSystem<DiedMessage>,
        IGameSystem<LeveledUpMessage>
    {
        // TODO: Write to log for first encounter with a creature or item

        public MessageProcessingResult Process(ItemMovedMessage message, GameManager manager)
        {
            if (message.Successful)
            {
                var itemEntity = message.ItemEntity;
                var item = itemEntity.Item;
                var position = itemEntity.Position;
                foreach (var playerEntity in manager.Players)
                {
                    var finalSensed = manager.SensorySystem.CanSense(playerEntity, itemEntity) ?? SenseType.None;

                    var initialTopContainer = message.InitialContainer != null
                        ? manager.ItemMovingSystem.GetTopContainer(message.InitialContainer, manager)
                        : itemEntity;
                    var initialPosition = message.InitialLevelCell ?? initialTopContainer.Position?.LevelCell;
                    var initialSensed = initialPosition != null
                        ? manager.SensorySystem.CanSense(playerEntity, initialTopContainer, initialPosition.Value)
                        : SenseType.None;

                    if (initialSensed == SenseType.None
                        && finalSensed == SenseType.None)
                    {
                        continue;
                    }

                    string logMessage = null;
                    if (message.InitialContainer != null)
                    {
                        if (position != null)
                        {
                            if (!message.SuppressLog)
                            {
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
                    else if (message.InitialLevelCell != null)
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
                        || !message.ActorEntity.Being.IsAlive
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

        public MessageProcessingResult Process(EffectsAppliedMessage message, GameManager manager)
        {
            if (message.TargetEntity == null)
            {
                return MessageProcessingResult.ContinueProcessing;
            }

            var ability = message.AbilityEntity.Ability;
            if ((ability.Activation & ActivationType.OnAttack) != 0
                || (ability.Action != AbilityAction.Default
                    && ability.Action != AbilityAction.Modifier
                    && ability.Action != AbilityAction.Drink))
            {
                foreach (var playerEntity in manager.Players)
                {
                    var attackerSensed = manager.SensorySystem.CanSense(playerEntity, message.ActivatorEntity) ??
                                         SenseType.None;
                    var victimSensed = manager.SensorySystem.CanSense(playerEntity, message.TargetEntity) ??
                                       SenseType.None;
                    if (attackerSensed == SenseType.None
                        && victimSensed == SenseType.None)
                    {
                        continue;
                    }

                    var weaponId = message.AppliedEffects
                        .SingleOrDefault(e => e.Effect.EffectType == EffectType.Activate)?
                        .Effect.TargetEntityId;
                    var weapon = manager.FindEntity(weaponId);
                    var logMessage = manager.Game.Services.Language.GetString(new AttackEvent(
                        playerEntity, message.ActivatorEntity, message.TargetEntity,
                        attackerSensed, victimSensed, message.AppliedEffects, ability.Action,
                        weapon, ranged: (message.AbilityTrigger & ActivationType.OnRangedAttack) != 0,
                        hit: message.SuccessfulApplication));
                    WriteLog(logMessage, playerEntity, manager);
                }

                return MessageProcessingResult.ContinueProcessing;
            }

            var activatableId = message.AppliedEffects
                .SingleOrDefault(e => e.Effect.EffectType == EffectType.Activate)?
                .Effect.TargetEntityId;
            var activatableEntity = manager.FindEntity(activatableId);
            if (activatableEntity?.HasComponent(EntityComponent.Item) == false)
            {
                activatableEntity = activatableEntity.Ability?.OwnerEntity;
            }

            if (activatableEntity?.HasComponent(EntityComponent.Item) == true)
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

                    var consumed = message.AppliedEffects
                        .Any(e => e.Effect.EffectType == EffectType.RemoveItem);
                    var itemSensed = manager.SensorySystem.CanSense(playerEntity, activatableEntity) ?? activatorSensed;
                    var logMessage = manager.Game.Services.Language.GetString(new ItemActivationEvent(
                        playerEntity, activatableEntity, message.ActivatorEntity, message.TargetEntity, itemSensed,
                        activatorSensed, targetSensed, consumed, message.SuccessfulApplication));
                    WriteLog(logMessage, playerEntity, manager);
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(DiedMessage message, GameManager manager)
        {
            var deceasedPosition = message.BeingEntity.Position;
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

        public MessageProcessingResult Process(LeveledUpMessage message, GameManager manager)
        {
            var leveledPosition = message.Entity.Position;
            foreach (var playerEntity in manager.Players)
            {
                if (manager.SensorySystem.CanSense(playerEntity, message.Entity) == SenseType.None)
                {
                    continue;
                }

                var logMessage = manager.Game.Services.Language.GetString(new LeveledUpEvent(
                    playerEntity, message.Entity, message.Race,
                    message.SkillPointsGained, message.TraitPointsGained, message.MutationPointsGained));
                WriteLog(logMessage, playerEntity, manager);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public void WriteLog(
            string message, GameEntity playerEntity, GameManager manager,
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

            playerEntity.Player.LogEntries.Add(entry);
        }
    }
}
