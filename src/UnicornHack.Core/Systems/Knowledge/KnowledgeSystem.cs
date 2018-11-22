using System;
using System.Linq;
using UnicornHack.Primitives;
using UnicornHack.Services.LogEvents;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Senses;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Knowledge
{
    /// <summary>
    ///     Tracks the entities on the level that the player knows about and player XP
    /// </summary>
    public class KnowledgeSystem :
        IGameSystem<VisibleTerrainChangedMessage>,
        IGameSystem<TraveledMessage>,
        IGameSystem<ItemMovedMessage>,
        IGameSystem<ItemEquippedMessage>,
        IGameSystem<ItemActivatedMessage>,
        IGameSystem<EffectsAppliedMessage>,
        IGameSystem<DiedMessage>
    {
        public const string XPGainedMessageName = "XPGained";

        public MessageProcessingResult Process(VisibleTerrainChangedMessage message, GameManager manager)
        {
            if (message.TilesExplored > 0)
            {
                AddPlayerXP(message.TilesExplored, manager);
            }

            var level = message.LevelEntity.Level;

            UpdateAllEntitiesKnowledge(manager.LevelActorsToLevelRelationship, manager.LevelActors.Matcher, level,
                manager);
            UpdateAllEntitiesKnowledge(manager.ConnectionsToLevelRelationship, manager.Connections.Matcher, level,
                manager);
            UpdateAllEntitiesKnowledge(manager.LevelItemsToLevelRelationship, manager.LevelItems.Matcher, level,
                manager);

            return MessageProcessingResult.ContinueProcessing;
        }

        private void UpdateAllEntitiesKnowledge(
            EntityRelationship<GameEntity> levelEntitiesToLevelRelationship,
            EntityMatcher<GameEntity> matcher,
            LevelComponent level,
            GameManager manager)
        {
            // TODO: Perf: use interval tree to only get the entities and knowledge within sense range
            foreach (var levelEntity in levelEntitiesToLevelRelationship[level.EntityId])
            {
                UpdateEntityKnowledge(levelEntity, matcher, manager, level);
            }
        }

        public MessageProcessingResult Process(TraveledMessage message, GameManager manager)
        {
            if (message.Successful)
            {
                UpdateEntityKnowledge(message.Entity, manager.LevelActors.Matcher, manager);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        private void UpdateEntityKnowledge(
            GameEntity entity,
            EntityMatcher<GameEntity> matcher,
            GameManager manager,
            LevelComponent level = null)
        {
            var position = entity.Position;
            if (position != null)
            {
                level = level ?? position.LevelEntity.Level;
            }

            SenseType sensedType;
            if (position != null
                && (sensedType = manager.SensorySystem.SensedByPlayer(entity, position.LevelCell)) !=
                    SenseType.None)
            {
                foreach (var conflictingKnowledge in
                    manager.LevelKnowledgeToLevelCellIndex[(position.LevelId, position.LevelX, position.LevelY)])
                {
                    var conflictingEntityId = conflictingKnowledge.Knowledge.KnownEntityId;
                    if (conflictingEntityId != entity.Id
                        && matcher.Matches(manager.FindEntity(conflictingEntityId)))
                    {
                        conflictingKnowledge.RemoveComponent(EntityComponent.Knowledge);
                        conflictingKnowledge.RemoveComponent(EntityComponent.Position);
                        break;
                    }
                }

                // TODO: Write to log for first encounter
                // TODO: Only interrupt queued action if hostile when first seen
                var ai = entity.AI;
                if (ai != null)
                {
                    foreach (var playerEntity in manager.Players)
                    {
                        var playerPosition = playerEntity.Position;
                        if (position.LevelId == playerPosition.LevelId
                            && position.LevelCell.DistanceTo(playerPosition.LevelCell) <= 2)
                        {
                            var player = playerEntity.Player;
                            if (player.QueuedAction)
                            {
                                player.NextAction = null;
                            }
                        }
                    }
                }

                UpdateKnowledge(position, sensedType, manager);
            }
            else
            {
                var knowledge = manager.LevelKnowledgeToLevelEntityRelationship[entity.Id];
                if (knowledge != null)
                {
                    var knowledgePosition = knowledge.Position;
                    if (level == null)
                    {
                        level = knowledgePosition.LevelEntity.Level;
                    }

                    if (level != knowledgePosition.LevelEntity?.Level
                        || (manager.SensorySystem.SensedByPlayer(knowledge, knowledgePosition.LevelCell)
                                & knowledge.Knowledge.SensedType) != SenseType.None)
                    {
                        knowledge.RemoveComponent(EntityComponent.Knowledge);
                        knowledge.RemoveComponent(EntityComponent.Position);
                    }
                }
            }
        }

        private void UpdateKnowledge(PositionComponent position, SenseType sensedType, GameManager manager)
        {
            var knowledge = manager.LevelKnowledgeToLevelEntityRelationship[position.EntityId];
            if (knowledge == null)
            {
                using (var entityReference = manager.CreateEntity())
                {
                    var knowledgeEntityReference = entityReference.Referenced;
                    var knowledgeComponent = manager.CreateComponent<KnowledgeComponent>(EntityComponent.Knowledge);
                    knowledgeComponent.KnownEntityId = position.EntityId;
                    knowledgeComponent.SensedType = sensedType;
                    knowledgeEntityReference.Knowledge = knowledgeComponent;

                    var knowledgePosition = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
                    knowledgePosition.SetLevelPosition(position.LevelId, position.LevelCell);
                    knowledgePosition.Heading = position.Heading;

                    knowledgeEntityReference.Position = knowledgePosition;
                }
            }
            else
            {
                knowledge.Knowledge.SensedType = sensedType;

                var knowledgePosition = knowledge.Position;
                knowledgePosition.SetLevelPosition(position.LevelId, position.LevelCell);
                knowledgePosition.Heading = position.Heading;
            }
        }

        public void AddPlayerXP(int xp, GameManager manager)
        {
            foreach (var playerEntity in manager.Players)
            {
                AddXP(playerEntity, xp, manager);
            }
        }

        public RaceComponent GetLearningRace(GameEntity actorEntity, GameManager manager)
            => manager.RacesToBeingRelationship[actorEntity.Id].Values.First().Race;

        public byte GetXPLevel(GameEntity actorEntity, GameManager manager)
            => (byte)manager.RacesToBeingRelationship[actorEntity.Id].Values.Sum(r => r.Race.Level);

        private void AddXP(GameEntity actorEntity, int xp, GameManager manager)
        {
            var player = actorEntity.Player;
            var being = actorEntity.Being;
            var leftoverXP = xp;
            while (leftoverXP != 0)
            {
                being.ExperiencePoints += leftoverXP;
                if (being.ExperiencePoints >= player.NextLevelXP)
                {
                    leftoverXP = being.ExperiencePoints - player.NextLevelXP;
                    being.ExperiencePoints = 0;

                    // TODO: fire an event and trigger abilities
                    var race = GetLearningRace(actorEntity, manager);
                    race.Level++;

                    UpdateNextLevelXP(actorEntity);
                }
                else
                {
                    leftoverXP = 0;
                }
            }

            var xpGained = manager.Queue.CreateMessage<XPGainedMessage>(XPGainedMessageName);
            xpGained.Entity = actorEntity;
            xpGained.ExperiencePoints = xp;
            manager.Enqueue(xpGained);
        }

        public void UpdateNextLevelXP(GameEntity actorEntity)
        {
            var player = actorEntity.Player;
            var playerLevel = GetXPLevel(actorEntity, actorEntity.Manager);
            if (playerLevel > player.MaxLevel)
            {
                player.MaxLevel = playerLevel;
            }

            var effectiveLevel = player.MaxLevel > playerLevel ? player.MaxLevel - playerLevel : playerLevel;
            player.NextLevelXP = (int)((1 + Math.Ceiling(Math.Pow(effectiveLevel, 1.5))) * 500);
        }

        public MessageProcessingResult Process(ItemMovedMessage message, GameManager manager)
        {
            if (message.Successful)
            {
                var itemEntity = message.ItemEntity;
                UpdateEntityKnowledge(itemEntity, manager.LevelItems.Matcher, manager);

                var item = itemEntity.Item;
                var position = itemEntity.Position;
                foreach (var playerEntity in manager.Players)
                {
                    var finalSensed = manager.SensorySystem.CanSense(playerEntity, itemEntity);

                    var initialTopContainer = message.InitialContainer != null
                        ? manager.ItemMovingSystem.GetTopContainer(message.InitialContainer, manager)
                        : itemEntity;
                    var initialPosition = message.InitialPosition ?? initialTopContainer.Position?.LevelCell;
                    var initialSensed = initialPosition != null
                        ? manager.SensorySystem.CanSense(playerEntity, initialTopContainer, initialPosition.Value)
                        : SenseType.None;

                    if (initialSensed == SenseType.None && finalSensed == SenseType.None)
                    {
                        continue;
                    }

                    string logMessage = null;
                    if (message.InitialContainer != null)
                    {
                        if (position != null)
                        {
                            if (message.InitialContainer.Being?.IsAlive != false)
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
                    else if (message.InitialPosition != null)
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
                var position = message.ActorEntity.Position;
                foreach (var playerEntity in manager.Players)
                {
                    if (playerEntity.Position.LevelId != position.LevelId
                        || !message.ActorEntity.Being.IsAlive)
                    {
                        continue;
                    }

                    var actorSensed = manager.SensorySystem.CanSense(playerEntity, message.ActorEntity);
                    var itemSensed = manager.SensorySystem.CanSense(playerEntity, message.ItemEntity);

                    var logMessage = manager.Game.Services.Language.GetString(new ItemEquipmentEvent(
                        playerEntity, message.ActorEntity, message.ItemEntity,
                        actorSensed, itemSensed, message.Slot));
                    WriteLog(logMessage, playerEntity, manager);
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(ItemActivatedMessage message, GameManager manager)
        {
            if (message.Successful)
            {
                var position = message.ActivatorEntity.Position;
                foreach (var playerEntity in manager.Players)
                {
                    if (playerEntity.Position.LevelId != position.LevelId)
                    {
                        continue;
                    }

                    var itemSensed = manager.SensorySystem.CanSense(playerEntity, message.ItemEntity);
                    var activatorSensed = manager.SensorySystem.CanSense(playerEntity, message.ActivatorEntity);
                    var targetSensed = manager.SensorySystem.CanSense(playerEntity, message.TargetEntity);

                    var logMessage = manager.Game.Services.Language.GetString(new ItemActivationEvent(
                        playerEntity, message.ItemEntity, message.ActivatorEntity, message.TargetEntity,
                        message.TargetCell, itemSensed, activatorSensed, targetSensed,
                        message.ActivationType, message.Successful));
                    WriteLog(logMessage, playerEntity, manager);
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(EffectsAppliedMessage message, GameManager manager)
        {
            if ((message.AbilityType & ActivationType.OnAttack) == 0
                || message.TargetEntity == null)
            {
                return MessageProcessingResult.ContinueProcessing;
            }

            var targetPosition = message.TargetEntity.Position;
            foreach (var playerEntity in manager.Players)
            {
                if (playerEntity.Position.LevelId != targetPosition.LevelId)
                {
                    continue;
                }

                var attackerSensed = manager.SensorySystem.CanSense(playerEntity, message.ActivatorEntity);
                if (message.TargetEntity == playerEntity)
                {
                    // TODO: Interrupt current action

                    attackerSensed |= SenseType.Touch;

                    UpdateKnowledge(message.ActivatorEntity.Position, attackerSensed, manager);
                }

                var victimSensed = manager.SensorySystem.CanSense(playerEntity, message.TargetEntity);
                if (victimSensed != SenseType.None)
                {
                    var weaponId = message.AppliedEffects
                        .SingleOrDefault(e => e.Effect.EffectType == EffectType.Activate)?
                        .Effect.ActivatableEntityId;
                    var weapon = manager.FindEntity(weaponId);
                    var logMessage = manager.Game.Services.Language.GetString(new AttackEvent(
                        playerEntity, message.ActivatorEntity, message.TargetEntity,
                        attackerSensed, victimSensed, message.AppliedEffects, message.AbilityEntity.Ability.Action,
                        weapon, ranged: (message.AbilityType & ActivationType.OnRangedAttack) != 0,
                        hit: message.SuccessfulApplication));
                    WriteLog(logMessage, playerEntity, manager);
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(DiedMessage message, GameManager manager)
        {
            if (!message.BeingEntity.HasComponent(EntityComponent.Player))
            {
                manager.KnowledgeSystem.AddPlayerXP(GetXPLevel(message.BeingEntity, manager) * 100, manager);
            }

            var deceasedPosition = message.BeingEntity.Position;
            foreach (var playerEntity in manager.Players)
            {
                if (playerEntity.Position.LevelId != deceasedPosition.LevelId)
                {
                    continue;
                }

                var deceasedSensed = manager.SensorySystem.CanSense(playerEntity, message.BeingEntity);
                if (deceasedSensed != SenseType.None)
                {
                    var logMessage = manager.Game.Services.Language.GetString(
                        new DeathEvent(playerEntity, message.BeingEntity, deceasedSensed));
                    WriteLog(logMessage, playerEntity, manager);
                }
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
