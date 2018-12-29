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

            UpdateAllEntitiesKnowledge(
                manager.LevelActorsToLevelRelationship, manager.LevelActors.Matcher, level, manager);
            UpdateAllEntitiesKnowledge(
                manager.ConnectionsToLevelRelationship, manager.Connections.Matcher, level, manager);
            UpdateAllEntitiesKnowledge(
                manager.LevelItemsToLevelRelationship, manager.LevelItems.Matcher, level, manager);

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
                UpdateEntityKnowledge(levelEntity, matcher, manager, level: level);
            }
        }

        public MessageProcessingResult Process(TraveledMessage message, GameManager manager)
        {
            if (message.Successful)
            {
                UpdateEntityKnowledge(message.Entity, manager.LevelActors.Matcher, manager,
                    additionalCellToTest: message.InitialLevelCell);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        private void UpdateEntityKnowledge(
            GameEntity entity,
            EntityMatcher<GameEntity> matcher,
            GameManager manager,
            Point? additionalCellToTest = null,
            LevelComponent level = null)
        {
            var position = entity.Position;
            if (position != null)
            {
                level = level ?? position.LevelEntity.Level;
            }

            SenseType sensedType;
            if (position != null
                && ((sensedType = manager.SensorySystem.SensedByPlayer(entity, position.LevelCell))
                    != SenseType.None
                    || (additionalCellToTest != null
                        && (sensedType = manager.SensorySystem.SensedByPlayer(entity, additionalCellToTest.Value))
                        != SenseType.None)))
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

        public MessageProcessingResult Process(ItemMovedMessage message, GameManager manager)
        {
            if (message.Successful)
            {
                var itemEntity = message.ItemEntity;
                UpdateEntityKnowledge(itemEntity, manager.LevelItems.Matcher, manager,
                    additionalCellToTest: message.InitialLevelCell);
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

            var targetPosition = message.TargetEntity.Position;
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
