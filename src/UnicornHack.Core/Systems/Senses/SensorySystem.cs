using System.Buffers;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Senses;

public class SensorySystem : IGameSystem<TraveledMessage>, IGameSystem<EntityAddedMessage<GameEntity>>
{
    public MessageProcessingResult Process(TraveledMessage message, GameManager manager)
    {
        if (!message.Successful)
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        var sensor = message.Entity.Sensor;
        if (sensor == null)
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        ResetVisibility(manager, sensor);

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(EntityAddedMessage<GameEntity> message, GameManager manager)
    {
        ResetVisibility(manager, message.Entity.Sensor!);

        return MessageProcessingResult.ContinueProcessing;
    }

    private void ResetVisibility(GameManager manager, SensorComponent sensor)
    {
        sensor.VisibleTerrainIsCurrent = false;

        if (sensor.Entity.HasComponent(EntityComponent.Player))
        {
            UpdateVisibleTerrain(sensor.Entity.Position!.RoomEntity.Room!, manager);
        }
    }

    public byte GetVisibility(GameEntity sensorEntity, Point p)
        => GetVisibleTiles(sensorEntity)[sensorEntity.Position!.RoomEntity.Room!.PointToIndex![p.X, p.Y]];

    public byte[] GetVisibleTiles(GameEntity sensorEntity)
    {
        var position = sensorEntity.Position!;
        var sensor = sensorEntity.Sensor!;
        var level = position.RoomEntity.Room!;

        var tileCount = level.TileCount;
        if (sensor.VisibleTerrain == null
            || sensor.VisibleTerrain.Length < tileCount)
        {
            if (sensor.VisibleTerrain != null)
            {
                ArrayPool<byte>.Shared.Return(sensor.VisibleTerrain);
            }

            // TODO: Perf: Use a smaller array
            sensor.VisibleTerrain = ArrayPool<byte>.Shared.Rent(tileCount);
        }
        else
        {
            if (sensor.VisibleTerrainIsCurrent)
            {
                return sensor.VisibleTerrain;
            }
        }

        Array.Clear(sensor.VisibleTerrain, 0, tileCount);

        level.VisibilityCalculator!.ComputeDirected(
            position.Cell,
            position.Heading!.Value,
            sensor.PrimaryFOVQuadrants,
            sensor.PrimaryVisionRange,
            sensor.TotalFOVQuadrants,
            sensor.SecondaryVisionRange,
            noFalloff: false,
            TileBlocksVisibility,
            sensor.VisibleTerrain);

        sensor.VisibleTerrainIsCurrent = true;

        return sensor.VisibleTerrain;
    }

    public IReadOnlyList<(Point, byte)> GetLOS(Point origin, Point target, RoomComponent room)
        => room.VisibilityCalculator!.ComputeLOS(
            origin,
            target,
            TileBlocksVisibility,
            room.Entity.Manager);

    public IReadOnlyList<(Point, byte)> GetLOS(
        Point origin,
        Vector targetVector,
        int range,
        RoomComponent room)
        => room.VisibilityCalculator!.ComputeLOS(
            origin,
            targetVector,
            range,
            TileBlocksVisibility,
            room.Entity.Manager);

    public static (bool, int) TileBlocksVisibility(byte x, byte y, RoomComponent room)
    {
        if (x < room.Width && y < room.Height)
        {
            var index = room.PointToIndex![x, y];
            var blocking = false;
            switch (room.Terrain[index])
            {
                // This is inlined for perf
                case MapFeature.Default:
                case MapFeature.StoneWall:
                case MapFeature.RockWall:
                    blocking = true;
                    break;
            }

            return (blocking, index);
        }

        return (true, -1);
    }

    private void UpdateVisibleTerrain(RoomComponent room, GameManager manager)
    {
        var tileCount = room.TileCount;
        Array.Clear(room.VisibleTerrain, 0, tileCount);

        var options = manager.Game.Options;
        var revealAll = options.RevealLevels || options.RevealRooms;

        var tilesExplored = 0;
        if (revealAll)
        {
            for (short i = 0; i < tileCount; i++)
            {
                room.VisibleTerrain[i] = byte.MaxValue;
                tilesExplored += manager.KnowledgeSystem.RevealTerrain(i, room);
            }

            if (options.RevealLevels)
            {
                RevealHiddenConnections(room);
            }
        }
        else
        {
            foreach (var playerEntity in manager.Players)
            {
                if (playerEntity.Position!.RoomId != room.EntityId)
                {
                    continue;
                }

                var visibleTiles = GetVisibleTiles(playerEntity);

                // TODO: Only check in the max visibility range
                for (short i = 0; i < tileCount; i++)
                {
                    var visibility = visibleTiles[i];
                    if (visibility == 0)
                    {
                        continue;
                    }

                    room.VisibleTerrain[i] = visibility;

                    // KnownTerrain updated here instead of a separate method for performance
                    tilesExplored += manager.KnowledgeSystem.RevealTerrain(i, room);
                }
            }
        }

        VisibleTerrainChangedMessage.Enqueue(room.Entity, manager);
        KnownTerrainChangedMessage.Enqueue(room.Entity, tilesExplored, manager);
    }

    private static void RevealHiddenConnections(RoomComponent room)
    {
        foreach (var connectionEntity in room.Connections.Values)
        {
            var connection = connectionEntity.Connection!;
            if (connection.Hidden)
            {
                connection.Hidden = false;
            }
        }
    }

    public SenseType SensedByPlayer(GameEntity target, Point targetPosition)
    {
        var sensed = SenseType.None;
        if (target.HasComponent(EntityComponent.Player))
        {
            sensed |= SenseType.Telepathy | SenseType.Touch;
        }

        var manager = target.Manager;
        var level = manager.ItemMovingSystem.GetTopContainer(target, manager).Position!.RoomEntity.Room!;
        if (level.VisibleTerrain[level.PointToIndex![targetPosition.X, targetPosition.Y]] != 0)
        {
            sensed |= SenseType.Sight;
        }

        return sensed;
    }

    public SenseType? CanSense(GameEntity sensor, GameEntity target)
    {
        var sense = SenseType.None;
        var manager = sensor.Manager;
        target = manager.ItemMovingSystem.GetTopContainer(target, manager);
        if (target.Position == null
            || sensor.Position?.RoomId != target.Position.RoomId)
        {
            return null;
        }

        // TODO: Also when touching (e.g. grappling)

        if (target == sensor)
        {
            sense |= SenseType.Telepathy | SenseType.Touch;
        }
        else if (sensor.HasComponent(EntityComponent.Player)
                 && target.HasComponent(EntityComponent.Player))
        {
            sense |= SenseType.Telepathy;
        }

        return sense | CanSense(sensor, target, target.Position.Cell);
    }

    public SenseType CanSense(GameEntity sensor, GameEntity target, Point targetPosition)
    {
        var sense = SenseType.None;
        // TODO: Take invisibility, light level and concealment into account
        if (GetVisibility(sensor, targetPosition) > 0)
        {
            sense |= SenseType.Sight;
        }

        return sense;
    }
}
