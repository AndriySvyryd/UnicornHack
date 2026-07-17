using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Senses;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Knowledge;

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
        var room = message.RoomEntity.Room!;
        UpdateAllEntitiesKnowledge(room.Actors, room.KnownActors, room, manager);
        UpdateAllEntitiesKnowledge(room.Connections, room.KnownConnections, room, manager);
        UpdateAllEntitiesKnowledge(room.Items, room.KnownItems, room, manager);

        return MessageProcessingResult.ContinueProcessing;
    }

    private void UpdateAllEntitiesKnowledge(
        IReadOnlyDictionary<Point, GameEntity> entities,
        IReadOnlyDictionary<Point, GameEntity> knownPositions,
        RoomComponent room,
        GameManager manager)
    {
        // TODO: Perf: use interval tree to only get the entities and knowledge within sense range
        foreach (var entity in entities.Values)
        {
            UpdateEntityKnowledge(entity, knownPositions, manager, room);
        }
    }

    public MessageProcessingResult Process(TraveledMessage message, GameManager manager)
    {
        if (message.Successful)
        {
            var room = message.Entity.Position!.RoomEntity.Room!;
            UpdateEntityKnowledge(message.Entity, room.KnownActors, manager, room,
                additionalCellToTest: message.InitialLevelCell);
        }
        else if (message.Entity.HasComponent(EntityComponent.Player))
        {
            var room = message.Entity.Position!.RoomEntity.Room!;
            var targetCell = message.TargetCell;

            // A blocked move into an in-room wall reveals that wall tile.
            if (targetCell.X < room.Width && targetCell.Y < room.Height)
            {
                var index = room.PointToIndex![targetCell.X, targetCell.Y];

                var tilesExplored = RevealTerrain(index, room);
                if (tilesExplored > 0)
                {
                    KnownTerrainChangedMessage.Enqueue(room.Entity, tilesExplored, manager);
                }
            }
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    private void UpdateEntityKnowledge(
        GameEntity entity,
        IReadOnlyDictionary<Point, GameEntity> knownPositions,
        GameManager manager,
        RoomComponent room,
        Point? additionalCellToTest = null)
    {
        var position = entity.Position;
        if (position == null)
        {
            return;
        }

        SenseType sensedType;
        if ((sensedType = manager.SensorySystem.SensedByPlayer(entity, position.Cell)) != SenseType.None
            || (additionalCellToTest != null
                && (sensedType = manager.SensorySystem.SensedByPlayer(entity, additionalCellToTest.Value))
                != SenseType.None))
        {
            var conflictingKnowledge = knownPositions.GetValueOrDefault(position.Cell);
            if (conflictingKnowledge != null)
            {
                var conflictingEntityId = conflictingKnowledge.Knowledge!.KnownEntityId;
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
                    var playerPosition = playerEntity.Position!;
                    if (position.RoomId == playerPosition.RoomId
                        && position.Cell.DistanceTo(playerPosition.Cell) <= 2)
                    {
                        var player = playerEntity.Player!;
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
                var knowledgePosition = knowledge.Position!;
                if (room != knowledgePosition.RoomEntity?.Room
                    || manager.SensorySystem.SensedByPlayer(knowledge, knowledgePosition.Cell) != SenseType.None)
                {
                    knowledge.RemoveComponent(EntityComponent.Knowledge);
                    knowledge.RemoveComponent(EntityComponent.Position);
                }
                else
                {
                    knowledge.Knowledge!.SensedType = SenseType.None;
                }
            }
        }
    }

    private void UpdateKnowledge(PositionComponent position, SenseType sensedType, GameManager manager)
    {
        var knowledgeEntity = position.Knowledge;
        if (knowledgeEntity == null)
        {
            using var entityReference = manager.CreateEntity();
            var knowledgeEntityReference = entityReference.Referenced;
            var knowledgeComponent = manager.CreateComponent<KnowledgeComponent>(EntityComponent.Knowledge);
            knowledgeComponent.KnownEntityId = position.EntityId;
            knowledgeComponent.SensedType = sensedType;
            if (sensedType.CanIdentify())
            {
                knowledgeComponent.IsIdentified = true;
            }

            knowledgeEntityReference.Knowledge = knowledgeComponent;

            var knowledgePosition = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
            knowledgePosition.SetRoomPosition(position.RoomId, position.Cell);
            knowledgePosition.Heading = position.Heading;

            knowledgeEntityReference.Position = knowledgePosition;
        }
        else
        {
            knowledgeEntity.Knowledge!.SensedType = sensedType;
            if (sensedType.CanIdentify())
            {
                knowledgeEntity.Knowledge.IsIdentified = true;
            }

            var knowledgePosition = knowledgeEntity.Position!;
            knowledgePosition.SetRoomPosition(position.RoomId, position.Cell);
            knowledgePosition.Heading = position.Heading;
        }
    }

    public int RevealTerrain(short index, RoomComponent room)
    {
        var terrain = room.Terrain[index];
        var knownTerrain = room.KnownTerrain[index];
        if (knownTerrain != terrain)
        {
            room.KnownTerrain[index] = terrain;
            if (room.KnownTerrainChanges != null)
            {
                room.KnownTerrainChanges[index] = terrain;
            }

            if (knownTerrain == MapFeature.Unexplored)
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
            var room = itemEntity.Position?.RoomEntity.Room ??
                        itemEntity.Item!.ContainerEntity?.Position?.RoomEntity.Room;
            if (room != null)
            {
                UpdateEntityKnowledge(itemEntity, room.KnownItems, manager, room,
                    additionalCellToTest: message.InitialRoomCell);
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

                UpdateKnowledge(message.ActivatorEntity.Position!, attackerSensed, manager);
            }
        }

        return MessageProcessingResult.ContinueProcessing;
    }
}
