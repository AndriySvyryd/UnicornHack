using System.Buffers;
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
    IGameSystem<AttackResultMessage>,
    IGameSystem<SoundMessage>
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
        Point? additionalCellToTest = null,
        SenseType forcedSense = SenseType.None,
        SenseType preservedSenseMask = SenseType.Sound)
    {
        var position = entity.Position;
        if (position?.Cell is not { } cell)
        {
            return;
        }

        var sensedType = forcedSense | manager.SensorySystem.SensedByPlayer(entity, cell);
        if (sensedType == SenseType.None
            && additionalCellToTest is { } additionalCell)
        {
            sensedType = manager.SensorySystem.SensedByPlayer(entity, additionalCell);
        }

        if (position.Knowledge?.Knowledge is { } existingKnowledge)
        {
            sensedType |= existingKnowledge.SensedType & preservedSenseMask;
        }

        if (sensedType != SenseType.None)
        {
            var conflictingKnowledge = knownPositions.GetValueOrDefault(cell);
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
                        && playerPosition.Cell is { } playerCell
                        && cell.DistanceTo(playerCell) <= 2)
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
                    || manager.SensorySystem.SensedByPlayer(knowledge, knowledgePosition.Cell!.Value) != SenseType.None)
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
            knowledgePosition.SetRoomPosition(position.RoomId, position.Cell!.Value);
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
            knowledgePosition.SetRoomPosition(position.RoomId, position.Cell!.Value);
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
                // A player always knows where they just dropped an item, so a drop out of a player's own
                // possession records the item's knowledge even when it spilled onto a cell the player can't
                // currently see.
                var forcedSense = message.InitialContainer is { } initialContainer
                    && manager.ItemMovingSystem.GetTopContainer(initialContainer, manager)
                        .HasComponent(EntityComponent.Player)
                    ? SenseType.Touch
                    : SenseType.None;

                UpdateEntityKnowledge(itemEntity, room.KnownItems, manager, room,
                    additionalCellToTest: message.InitialRoomCell, forcedSense: forcedSense);
            }
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(AttackResultMessage message, GameManager manager)
    {
        if (!message.RevealsAttacker)
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

    public MessageProcessingResult Process(SoundMessage message, GameManager manager)
    {
        var source = message.Source;
        if (source.Position is not { Cell: { } sourceCell } sourcePosition)
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        var room = sourcePosition.RoomEntity.Room!;
        var exactSoundSense = SenseType.None;
        var players = ArrayPool<GameEntity>.Shared.Rent(manager.Players.Count);
        var playerCount = 0;
        foreach (var player in manager.Players)
        {
            if (player != source)
            {
                players[playerCount++] = player;
            }
        }

        var soundSenses = ArrayPool<SenseType>.Shared.Rent(playerCount);
        try
        {
            manager.SensorySystem.CalculateSensedSound(
                source, message.ActionNoise,
                players.AsSpan(0, playerCount), soundSenses.AsSpan(0, playerCount));
            for (var i = 0; i < playerCount; i++)
            {
                var player = players[i];
                var actionSense = manager.SensorySystem.CanSense(player, source, sourceCell)
                                  | soundSenses[i];
                exactSoundSense |= soundSenses[i] & SenseType.Sound;
                if ((actionSense & SenseType.SoundDistant) != 0
                    && (actionSense & (SenseType.Sight | SenseType.Touch | SenseType.Telepathy | SenseType.Sound)) == 0)
                {
                    var direction = player.Position!.Cell!.Value.DifferenceTo(sourceCell).GetUnit().AsDirection();
                    manager.LoggingSystem.WriteLog(
                        manager.Game.Services.Language.SoundFrom(direction), player, manager);
                }
            }
        }
        finally
        {
            ArrayPool<SenseType>.Shared.Return(soundSenses);
            ArrayPool<GameEntity>.Shared.Return(players, clearArray: true);
        }

        UpdateEntityKnowledge(
            source, room.KnownActors, manager, room,
            forcedSense: exactSoundSense, preservedSenseMask: SenseType.None);

        return MessageProcessingResult.ContinueProcessing;
    }
}
