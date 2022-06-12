using System.Collections.Generic;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Senses;
using UnicornHack.Utils.DataStructures;
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
        IGameSystem<EffectsAppliedMessage>
    {
        public MessageProcessingResult Process(VisibleTerrainChangedMessage message, GameManager manager)
        {
            var level = message.LevelEntity.Level;
            UpdateAllEntitiesKnowledge(level.Actors, level.KnownActors, level, manager);
            UpdateAllEntitiesKnowledge(level.Connections, level.KnownConnections, level, manager);
            UpdateAllEntitiesKnowledge(level.Items, level.KnownItems, level, manager);

            return MessageProcessingResult.ContinueProcessing;
        }

        private void UpdateAllEntitiesKnowledge(
            IReadOnlyDictionary<Point, GameEntity> levelEntities,
            IReadOnlyDictionary<Point, GameEntity> knownPositions,
            LevelComponent level,
            GameManager manager)
        {
            // TODO: Perf: use interval tree to only get the entities and knowledge within sense range
            foreach (var levelEntity in levelEntities.Values)
            {
                UpdateEntityKnowledge(levelEntity, knownPositions, manager, level);
            }
        }

        public MessageProcessingResult Process(TraveledMessage message, GameManager manager)
        {
            if (message.Successful)
            {
                var level = message.Entity.Position.LevelEntity.Level;
                UpdateEntityKnowledge(message.Entity, level.KnownActors, manager, level, additionalCellToTest: message.InitialLevelCell);
            }
            else if (message.Entity.HasComponent(EntityComponent.Player))
            {
                var level = message.Entity.Position.LevelEntity.Level;
                var index = level.PointToIndex[message.TargetCell.X, message.TargetCell.Y];

                var tilesExplored = RevealTerrain(index, level);
                if (tilesExplored > 0)
                {
                    KnownTerrainChangedMessage.Enqueue(level.Entity, tilesExplored, manager);
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        private void UpdateEntityKnowledge(
            GameEntity entity,
            IReadOnlyDictionary<Point, GameEntity> knownPositions,
            GameManager manager,
            LevelComponent level,
            Point? additionalCellToTest = null)
        {
            var position = entity.Position;
            if (position == null)
            {
                return;
            }

            SenseType sensedType;
            if ((sensedType = manager.SensorySystem.SensedByPlayer(entity, position.LevelCell)) != SenseType.None
                || (additionalCellToTest != null
                    && (sensedType = manager.SensorySystem.SensedByPlayer(entity, additionalCellToTest.Value))
                    != SenseType.None))
            {
                var conflictingKnowledge = knownPositions.GetValueOrDefault(position.LevelCell);
                if (conflictingKnowledge != null)
                {
                    var conflictingEntityId = conflictingKnowledge.Knowledge.KnownEntityId;
                    if (conflictingEntityId != entity.Id)
                    {
                        conflictingKnowledge.RemoveComponent(EntityComponent.Knowledge);
                        conflictingKnowledge.RemoveComponent(EntityComponent.Position);
                    }
                }

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
                var knowledge = position.Knowledge;
                if (knowledge != null)
                {
                    var knowledgePosition = knowledge.Position;
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
            var knowledge = position.Knowledge;
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

        public int RevealTerrain(int index, LevelComponent level)
        {
            var terrain = level.Terrain[index];
            var knownTerrain = level.KnownTerrain[index];
            if (knownTerrain != terrain)
            {
                level.KnownTerrain[index] = terrain;
                if (level.KnownTerrainChanges != null)
                {
                    level.KnownTerrainChanges[index] = terrain;
                }

                if (knownTerrain == (byte)MapFeature.Unexplored)
                {
                    return 1;
                }
            }

            return 0;
        }

        public MessageProcessingResult Process(ItemMovedMessage message, GameManager manager)
        {
            if (message.Successful)
            {
                var itemEntity = message.ItemEntity;
                var level = itemEntity.Position?.LevelEntity.Level ?? itemEntity.Item.ContainerEntity?.Position?.LevelEntity.Level;
                if (level != null)
                {
                    UpdateEntityKnowledge(itemEntity, level.KnownItems, manager, level, additionalCellToTest: message.InitialLevelCell);
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(EffectsAppliedMessage message, GameManager manager)
        {
            if ((message.AbilityTrigger & ActivationType.OnAttack) == 0
                || message.TargetEntity == null)
            {
                return MessageProcessingResult.ContinueProcessing;
            }

            foreach (var playerEntity in manager.Players)
            {
                var attackerSensed = manager.SensorySystem.CanSense(playerEntity, message.ActivatorEntity) ??
                                     SenseType.None;
                if (message.TargetEntity == playerEntity)
                {
                    // TODO: Interrupt current action

                    attackerSensed |= SenseType.Touch;

                    UpdateKnowledge(message.ActivatorEntity.Position, attackerSensed, manager);
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }
    }
}
