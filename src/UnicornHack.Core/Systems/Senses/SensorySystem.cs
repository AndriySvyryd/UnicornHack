using System.Buffers;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Senses;

public class SensorySystem :
    IGameSystem<TraveledMessage>,
    IGameSystem<EntityAddedMessage<GameEntity>>,
    IGameSystem<PropertyValueChangedMessage<GameEntity, int>>
{
    public static readonly int DistantSoundThreshold = 20;

    public MessageProcessingResult Process(TraveledMessage message, GameManager manager)
    {
        if (!message.Successful)
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        var sensor = message.Entity.Being;
        if (sensor == null)
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        ResetVisibility(manager, sensor);

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(EntityAddedMessage<GameEntity> message, GameManager manager)
    {
        ResetVisibility(manager, message.Entity.Being!);

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(
        PropertyValueChangedMessage<GameEntity, int> message, GameManager manager)
    {
        if (message.Entity.Being is { } sensor)
        {
            ResetVisibility(message.Entity.Manager, sensor);
        }
        return MessageProcessingResult.ContinueProcessing;
    }

    private void ResetVisibility(GameManager manager, BeingComponent sensor)
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
        var sensor = sensorEntity.Being!;
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

        if (position.Cell is { } cell
            && sensor.PrimaryVisionRange > 0
            && (sensor.TotalFOVQuadrants <= sensor.PrimaryFOVQuadrants
                || sensor.SecondaryVisionRange > 0))
        {
            level.VisibilityCalculator!.ComputeDirected(
                cell,
                position.Heading!.Value,
                sensor.PrimaryFOVQuadrants,
                sensor.PrimaryVisionRange,
                sensor.TotalFOVQuadrants,
                sensor.SecondaryVisionRange,
                noFalloff: false,
                TileBlocksVisibility,
                sensor.VisibleTerrain);
        }

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

        var topContainer = manager.ItemMovingSystem.GetTopContainer(target, manager);
        var level = topContainer.Position!.RoomEntity.Room!;
        if (level.VisibleTerrain[level.PointToIndex![targetPosition.X, targetPosition.Y]] != 0)
        {
            sensed |= SenseType.Sight;
        }

        if (topContainer == target)
        {
            foreach (var player in manager.Players)
            {
                if (player.Position is { Cell: { } playerCell } playerPosition
                    && playerPosition.RoomId == level.Entity.Id
                    && playerCell.DistanceTo(targetPosition) <= 1)
                {
                    sensed |= SenseType.Touch;
                    break;
                }
            }
        }

        return sensed;
    }

    public SenseType? CanSense(GameEntity sensor, GameEntity target)
    {
        var manager = sensor.Manager;
        var topContainer = manager.ItemMovingSystem.GetTopContainer(target, manager);
        if (topContainer.Position == null
            || sensor.Position?.RoomId != topContainer.Position.RoomId)
        {
            return null;
        }

        var sense = SenseType.None;
        if (topContainer == sensor)
        {
            sense |= SenseType.Telepathy | SenseType.Touch;
        }
        else if (sensor.HasComponent(EntityComponent.Player)
                 && topContainer.HasComponent(EntityComponent.Player))
        {
            sense |= SenseType.Telepathy;
        }

        if (manager.Game.Options.RevealRooms
            && sensor.HasComponent(EntityComponent.Player))
        {
            sense |= SenseType.Sight;
        }

        var sensed = topContainer.Position.Cell is { } targetCell
            ? sense | CanSense(sensor, topContainer, targetCell)
            : sense;
        return topContainer == target ? sensed : sensed & ~SenseType.Touch;
    }

    public SenseType CanSense(GameEntity sensor, GameEntity target, Point targetPosition)
    {
        var sense = SenseType.None;
        if (sensor.Game.Options.RevealRooms
            && sensor.HasComponent(EntityComponent.Player))
        {
            sense |= SenseType.Sight;
        }

        if (sensor.Position is { Cell: { } sensorCell } sensorPosition
            && target.Position?.RoomId == sensorPosition.RoomId
            && sensorCell.DistanceTo(targetPosition) <= 1)
        {
            sense |= SenseType.Touch;
        }

        // TODO: Take invisibility, light level and concealment into account
        if (GetVisibility(sensor, targetPosition) > 0)
        {
            sense |= SenseType.Sight;
        }

        return sense;
    }

    /// <summary>
    ///     Whether <paramref name="sensor" /> can hear an action performed by <paramref name="source" />.
    ///     Positive action noise reduces the source's intrinsic quietness, clamped to the authored 0-254
    ///     scale. A cheap Chebyshev-distance bound rejects impossible paths before a weighted search accounts
    ///     for floor, doorway and wall attenuation.
    /// </summary>
    public bool CanHear(GameEntity sensor, GameEntity source, int actionNoise)
    {
        if (source.Being is not { } sourceBeing
            || source.Position is not { Cell: { } sourceCell } sourcePosition)
        {
            return false;
        }

        var effectiveQuietness = Math.Clamp(sourceBeing.Quietness - actionNoise, 0, 254);
        var room = sourcePosition.RoomEntity.Room!;
        if (!TryGetSoundTarget(
                sensor, sourcePosition.RoomId, sourceCell, room.PointToIndex!, effectiveQuietness,
                out var sensorIndex, out var attenuationBudget))
        {
            return false;
        }

        var sourceIndex = room.PointToIndex![sourceCell.X, sourceCell.Y];
        var distance = WeightedGraphSearch.GetShortestDistanceAStar(
                new SoundGraph(room), sourceIndex, sensorIndex, attenuationBudget);
        return distance != int.MaxValue;
    }

    public void CalculateSensedSound(
        GameEntity source, int actionNoise,
        ReadOnlySpan<GameEntity> listeners, Span<SenseType> soundSenses)
    {
        if (soundSenses.Length < listeners.Length)
        {
            throw new ArgumentException("The result span is shorter than the listener span.", nameof(soundSenses));
        }

        soundSenses = soundSenses[..listeners.Length];
        soundSenses.Clear();
        if (listeners.Length == 0
            || source.Being is not { } sourceBeing
            || source.Position is not { Cell: { } sourceCell } sourcePosition)
        {
            return;
        }

        var room = sourcePosition.RoomEntity.Room!;
        var sourceIndex = room.PointToIndex![sourceCell.X, sourceCell.Y];
        var effectiveQuietness = Math.Clamp(sourceBeing.Quietness - actionNoise, 0, 254);
        var targetIndexes = ArrayPool<int>.Shared.Rent(listeners.Length);
        var attenuationBudgets = ArrayPool<int>.Shared.Rent(listeners.Length);
        var pathAttenuations = ArrayPool<int>.Shared.Rent(listeners.Length);
        try
        {
            for (var i = 0; i < listeners.Length; i++)
            {
                if (!TryGetSoundTarget(
                        listeners[i], sourcePosition.RoomId, sourceCell, room.PointToIndex, effectiveQuietness,
                        out targetIndexes[i], out attenuationBudgets[i]))
                {
                    targetIndexes[i] = sourceIndex;
                    attenuationBudgets[i] = -1;
                }
            }

            WeightedGraphSearch.GetShortestDistancesAStar(
                new SoundGraph(room), sourceIndex,
                targetIndexes.AsSpan(0, listeners.Length),
                attenuationBudgets.AsSpan(0, listeners.Length),
                pathAttenuations.AsSpan(0, listeners.Length));

            for (var i = 0; i < listeners.Length; i++)
            {
                if (pathAttenuations[i] != int.MaxValue)
                {
                    soundSenses[i] = GetSoundSense(attenuationBudgets[i], pathAttenuations[i]);
                }
            }
        }
        finally
        {
            ArrayPool<int>.Shared.Return(targetIndexes);
            ArrayPool<int>.Shared.Return(attenuationBudgets);
            ArrayPool<int>.Shared.Return(pathAttenuations);
        }
    }

    private static bool TryGetSoundTarget(
        GameEntity listener, int roomId, Point sourceCell, short[,] pointToIndex, int effectiveQuietness,
        out int targetIndex, out int attenuationBudget)
    {
        targetIndex = 0;
        attenuationBudget = -1;
        if (listener.Being is not { HearingLevel: > 0 } hearing
            || listener.Position is not { Cell: { } listenerCell } position
            || position.RoomId != roomId)
        {
            return false;
        }

        attenuationBudget = hearing.HearingLevel * 10 - effectiveQuietness;
        if (attenuationBudget < 0
            || sourceCell.DistanceTo(listenerCell) * 10 > attenuationBudget)
        {
            return false;
        }

        targetIndex = pointToIndex[listenerCell.X, listenerCell.Y];
        return true;
    }

    private static SenseType GetSoundSense(int attenuationBudget, int pathAttenuation)
        => attenuationBudget - pathAttenuation < DistantSoundThreshold
            ? SenseType.SoundDistant
            : SenseType.Sound;

    private readonly struct SoundGraph(RoomComponent room) : IWeightedGraph
    {
        public int NodeCount => room.TileCount;
        public int NeighborCount => Vector.MovementDirections.Length;

        public bool TryGetNeighbor(
            int node, int neighborIndex, out int neighbor, out int distance)
        {
            var current = room.IndexToPoint![node];
            var direction = Vector.MovementDirections[neighborIndex];
            var x = current.X + direction.X;
            var y = current.Y + direction.Y;
            if (x < 0 || x >= room.Width || y < 0 || y >= room.Height)
            {
                neighbor = 0;
                distance = 0;
                return false;
            }

            neighbor = room.PointToIndex![x, y];
            distance = GetSoundAttenuation(room.Terrain[neighbor]);
            return true;
        }

        public int GetEstimatedDistance(int node, int target)
            => room.IndexToPoint![node].DistanceTo(room.IndexToPoint[target]) * 10;
    }

    private static int GetSoundAttenuation(MapFeature feature)
        => feature switch
        {
            MapFeature.StoneArchway => 40,
            MapFeature.Default or MapFeature.StoneWall or MapFeature.RockWall => 100,
            _ => 10
        };
}
