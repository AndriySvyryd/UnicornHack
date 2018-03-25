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
    ///     Tracks the entities on the level that the player knows about
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

            // TODO: Use CanSense
            if (position != null
                && level.VisibleTerrain[level.PointToIndex[position.LevelX, position.LevelY]] != 0)
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

                UpdateKnowledge(position, manager);
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
                        || level.VisibleTerrain
                            [level.PointToIndex[knowledge.Position.LevelX, knowledge.Position.LevelY]] != 0)
                    {
                        // TODO: Only if the perception level matches
                        knowledge.RemoveComponent(EntityComponent.Knowledge);
                        knowledge.RemoveComponent(EntityComponent.Position);
                    }
                }
            }
        }

        private void UpdateKnowledge(PositionComponent position, GameManager manager)
        {
            var knowledge = manager.LevelKnowledgeToLevelEntityRelationship[position.EntityId];
            if (knowledge == null)
            {
                using (var entityReference = manager.CreateEntity())
                {
                    var knowledgeComponent = manager.CreateComponent<KnowledgeComponent>(EntityComponent.Knowledge);
                    knowledgeComponent.KnownEntityId = position.EntityId;
                    entityReference.Referenced.Knowledge = knowledgeComponent;

                    var knowledgePosition = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
                    knowledgePosition.LevelId = position.LevelId;
                    knowledgePosition.Heading = position.Heading;
                    knowledgePosition.LevelCell = position.LevelCell;

                    entityReference.Referenced.Position = knowledgePosition;
                }
            }
            else
            {
                var knowledgePosition = knowledge.Position;
                knowledgePosition.LevelId = position.LevelId;
                knowledgePosition.Heading = position.Heading;
                knowledgePosition.LevelCell = position.LevelCell;
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
            => manager.RacesToBeingRelationship[actorEntity.Id].Values.Last().Race;

        public byte GetXPLevel(GameEntity actorEntity, GameManager manager)
            => (byte)manager.RacesToBeingRelationship[actorEntity.Id].Values.Sum(r => r.Race.Level);

        private void AddXP(GameEntity actorEntity, int xp, GameManager manager)
        {
            var player = actorEntity.Player;
            var leftoverXP = xp;
            while (leftoverXP != 0)
            {
                var race = GetLearningRace(actorEntity, manager);
                race.ExperiencePoints += leftoverXP;
                if (race.ExperiencePoints >= race.NextLevelXP)
                {
                    leftoverXP = race.ExperiencePoints - race.NextLevelXP;
                    race.ExperiencePoints = 0;
                    // TODO: fire an event and trigger abilities
                    race.Level++;
                    if (race.Level > player.MaxXPLevel)
                    {
                        player.MaxXPLevel = race.Level;
                    }

                    UpdateNextLevelXP(race, actorEntity);
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

        public void UpdateNextLevelXP(RaceComponent race, GameEntity entity)
        {
            var entityLevel = GetXPLevel(entity, entity.Manager);
            var player = entity.Player;
            var currentLevel = player.MaxXPLevel > race.Level ? race.Level : entityLevel;
            race.NextLevelXP = (int)((1 + Math.Ceiling(Math.Pow(currentLevel, 1.5))) * 500);
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
                    var finalSensed = manager.SensorySystem.CanSense(playerEntity, itemEntity, manager);

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
                                playerEntity, manager.FindEntity(item.ContainerId.Value), message.ItemEntity,
                                message.InitialCount, finalSensed, initialSensed));
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

                    var actorSensed = manager.SensorySystem.CanSense(playerEntity, message.ActorEntity, manager);
                    var itemSensed = manager.SensorySystem.CanSense(playerEntity, message.ItemEntity, manager);

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

                    var itemSensed = manager.SensorySystem.CanSense(playerEntity, message.ItemEntity, manager);
                    var activatorSensed =
                        manager.SensorySystem.CanSense(playerEntity, message.ActivatorEntity, manager);
                    var targetSensed =
                        manager.SensorySystem.CanSense(playerEntity, message.TargetEntity, manager);

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
            if ((message.AbilityType & ActivationType.OnAttack) == 0)
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

                var attackerSensed = manager.SensorySystem.CanSense(playerEntity, message.ActivatorEntity, manager);
                if (message.TargetEntity == playerEntity)
                {
                    // TODO: Interrupt current action

                    if ((attackerSensed & SenseType.Sight) == 0)
                    {
                        var position = message.TargetEntity.Position;
                        // TODO: partial perception level
                        UpdateKnowledge(position, manager);
                    }
                }

                var victimSensed = manager.SensorySystem.CanSense(playerEntity, message.TargetEntity, manager);
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

                var deceasedSensed = manager.SensorySystem.CanSense(playerEntity, message.BeingEntity, manager);
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
