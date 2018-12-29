using System;
using System.Collections.Generic;
using UnicornHack.Primitives;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Senses
{
    public class SensorySystem : IGameSystem<TraveledMessage>, IGameSystem<EntityAddedMessage<GameEntity>>
    {
        public const string VisibleTerrainChangedMessageName = "VisibleTerrainChanged";

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
            ResetVisibility(manager, message.Entity.Sensor);

            return MessageProcessingResult.ContinueProcessing;
        }

        private void ResetVisibility(GameManager manager, SensorComponent sensor)
        {
            sensor.VisibleTerrainIsCurrent = false;

            if (sensor.Entity.HasComponent(EntityComponent.Player))
            {
                UpdateVisibleTerrain(sensor.Entity.Position.LevelEntity.Level, manager);
            }
        }

        public byte GetVisibility(GameEntity sensorEntity, Point p)
            => GetVisibleTiles(sensorEntity)[sensorEntity.Position.LevelEntity.Level.PointToIndex[p.X, p.Y]];

        public byte[] GetVisibleTiles(GameEntity sensorEntity)
        {
            var position = sensorEntity.Position;
            var sensor = sensorEntity.Sensor;
            var level = position.LevelEntity.Level;

            var tileCount = level.Height * level.Width;
            if (sensor.VisibleTerrain == null
                || sensor.VisibleTerrain.Length != tileCount)
            {
                // TODO: Perf: Use pooling
                // TODO: Perf: Use a smaller array
                sensor.VisibleTerrain = new byte[tileCount];
            }
            else
            {
                if (sensor.VisibleTerrainIsCurrent)
                {
                    return sensor.VisibleTerrain;
                }

                Array.Clear(sensor.VisibleTerrain, 0, sensor.VisibleTerrain.Length);
            }

            level.VisibilityCalculator.ComputeDirected(
                position.LevelCell,
                position.Heading.Value,
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

        public IReadOnlyList<(int, byte)> GetLOS(GameEntity sensorEntity, Point target)
        {
            var position = sensorEntity.Position;
            var level = position.LevelEntity.Level;

            // TODO: Perf: Use pooling
            var losTiles = new List<(int, byte)>();

            level.VisibilityCalculator.ComputeLOS(
                position.LevelCell,
                target,
                TileBlocksVisibility,
                losTiles);

            return losTiles;
        }

        public static (bool, int) TileBlocksVisibility(byte x, byte y, LevelComponent level)
        {
            if (x < level.Width && y < level.Height)
            {
                var index = level.PointToIndex[x, y];
                var blocking = false;
                switch ((MapFeature)level.Terrain[index])
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

        private byte[] UpdateVisibleTerrain(LevelComponent level, GameManager manager)
        {
            Array.Clear(level.VisibleTerrain, 0, level.VisibleTerrain.Length);

            var tilesExplored = 0;
            foreach (var playerEntity in manager.Players)
            {
                if (playerEntity.Position.LevelId != level.EntityId)
                {
                    continue;
                }

                var visibleTiles = GetVisibleTiles(playerEntity);

                // TODO: Only check in the max visibility range
                for (var i = 0; i < visibleTiles.Length; i++)
                {
                    var visibility = visibleTiles[i];
                    if (visibility == 0)
                    {
                        continue;
                    }

                    level.VisibleTerrain[i] = visibility;

                    // KnownTerrain updated here instead of a separate method for performance
                    if (level.KnownTerrain[i] == (byte)MapFeature.Unexplored)
                    {
                        tilesExplored++;
                        var terrain = level.Terrain[i];
                        level.KnownTerrain[i] = terrain;
                        if (level.KnownTerrainChanges != null)
                        {
                            level.KnownTerrainChanges[i] = terrain;
                        }
                    }
                }
            }

            var terrainChanged =
                manager.Queue.CreateMessage<VisibleTerrainChangedMessage>(VisibleTerrainChangedMessageName);
            terrainChanged.LevelEntity = level.Entity;
            terrainChanged.TilesExplored = tilesExplored;

            manager.Enqueue(terrainChanged);

            return level.VisibleTerrain;
        }

        public SenseType SensedByPlayer(GameEntity target, Point targetPosition)
        {
            var sensed = SenseType.None;
            if (target.HasComponent(EntityComponent.Player))
            {
                sensed |= SenseType.Telepathy | SenseType.Touch;
            }

            var manager = target.Manager;
            var level = manager.ItemMovingSystem.GetTopContainer(target, manager).Position.LevelEntity.Level;
            if (level.VisibleTerrain[level.PointToIndex[targetPosition.X, targetPosition.Y]] != 0)
            {
                sensed |= SenseType.Sight;
            }

            return sensed;
        }

        public SenseType? CanSense(GameEntity sensor, GameEntity target)
        {
            var sense = SenseType.None;
            if (target == null)
            {
                return sense;
            }

            var manager = sensor.Manager;
            target = manager.ItemMovingSystem.GetTopContainer(target, manager);
            if (target.Position == null
                || sensor.Position?.LevelId != target.Position.LevelId)
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

            return sense | CanSense(sensor, target, target.Position.LevelCell);
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
}
