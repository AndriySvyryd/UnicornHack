using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Primitives;
using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs
{
    public class LevelSnapshot
    {
        private HashSet<GameEntity> ActorsSnapshot { get; } =
            new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);

        private Dictionary<GameEntity, LevelItemSnapshot> ItemsSnapshot { get; } =
            new Dictionary<GameEntity, LevelItemSnapshot>(EntityEqualityComparer<GameEntity>.Instance);

        private HashSet<GameEntity> ConnectionsSnapshot { get; } =
            new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);

        public LevelSnapshot Snapshot(GameEntity levelEntity, SerializationContext context)
        {
            var level = levelEntity.Level;
            if (level.VisibleTerrainSnapshot == null)
            {
                level.VisibleTerrainSnapshot = (byte[])level.VisibleTerrain.Clone();
            }
            else
            {
                level.VisibleTerrain.CopyTo(level.VisibleTerrainSnapshot, 0);
            }

            var manager = levelEntity.Manager;
            var actors = GetActors(levelEntity, manager);
            ActorsSnapshot.Clear();
            ActorsSnapshot.AddRange(actors);

            var items = GetItems(levelEntity, manager);
            ItemsSnapshot.Clear();
            ItemsSnapshot.AddRange(items, i => new LevelItemSnapshot().Snapshot(i, context));

            var abilities = GetConnections(levelEntity, manager);
            ConnectionsSnapshot.Clear();
            ConnectionsSnapshot.AddRange(abilities);

            return this;
        }

        public static List<object> Serialize(
            GameEntity levelEntity, EntityState? state, LevelSnapshot snapshot, SerializationContext context)
        {
            var manager = context.Manager;
            var level = levelEntity.Level;
            List<object> properties;
            switch (state)
            {
                case null:
                case EntityState.Added:
                    var knownTerrain = new List<short>();
                    for (short j = 0; j < level.KnownTerrain.Length; j++)
                    {
                        var feature = level.KnownTerrain[j];
                        if (feature != (byte)MapFeature.Unexplored)
                        {
                            knownTerrain.Add(j);
                            knownTerrain.Add(feature);
                        }
                    }

                    var wallNeighbours = new List<short>();
                    for (short j = 0; j < level.WallNeighbours.Length; j++)
                    {
                        if (level.KnownTerrain[j] == (byte)MapFeature.Unexplored)
                        {
                            continue;
                        }

                        var neighbours = level.WallNeighbours[j] & (byte)DirectionFlags.Cross;
                        if (neighbours != (byte)DirectionFlags.None)
                        {
                            wallNeighbours.Add(j);
                            wallNeighbours.Add((byte)neighbours);
                        }
                    }

                    var visibleTerrain = new List<short>();
                    for (short j = 0; j < level.VisibleTerrain.Length; j++)
                    {
                        var visibility = level.VisibleTerrain[j];
                        if (visibility != 0)
                        {
                            visibleTerrain.Add(j);
                            visibleTerrain.Add(visibility);
                        }
                    }

                    properties = state == null
                        ? new List<object>(10)
                        : new List<object>(11) {(int)state};

                    properties.Add(GetActors(levelEntity, manager)
                        .Select(a => LevelActorSnapshot.Serialize(a, null, context)).ToList());
                    properties.Add(GetItems(levelEntity, manager)
                        .Select(t => LevelItemSnapshot.Serialize(t, null, null, context)).ToList());
                    properties.Add(GetConnections(levelEntity, manager)
                        .Select(c => ConnectionSnapshot.Serialize(c, null, context)).ToList());
                    properties.Add(knownTerrain);
                    properties.Add(wallNeighbours);
                    properties.Add(visibleTerrain);
                    properties.Add(level.BranchName);
                    properties.Add(level.Depth);
                    properties.Add(level.Width);
                    properties.Add(level.Height);
                    return properties;
                default:
                    var levelEntry = context.DbContext.Entry(level);
                    properties = new List<object> {(int)state};

                    var i = 2;
                    var serializedActors = GameTransmissionProtocol.Serialize(
                        GetActors(levelEntity, manager),
                        snapshot.ActorsSnapshot,
                        LevelActorSnapshot.Serialize,
                        context);
                    if (serializedActors.Count > 0)
                    {
                        properties.Add(i);
                        properties.Add(serializedActors);
                    }

                    i++;
                    var serializedItems = GameTransmissionProtocol.Serialize(
                        GetItems(levelEntity, manager),
                        snapshot.ItemsSnapshot,
                        LevelItemSnapshot.Serialize,
                        context);
                    if (serializedItems.Count > 0)
                    {
                        properties.Add(i);
                        properties.Add(serializedItems);
                    }

                    i++;
                    var serializedConnections = GameTransmissionProtocol.Serialize(
                        GetConnections(levelEntity, manager),
                        snapshot.ConnectionsSnapshot,
                        ConnectionSnapshot.Serialize,
                        context);
                    if (serializedConnections.Count > 0)
                    {
                        properties.Add(i);
                        properties.Add(serializedConnections);
                    }

                    if (levelEntry.State != EntityState.Unchanged)
                    {
                        i++;
                        var wallNeighboursChanges = new List<object>();
                        var knownTerrainChanges = new List<object>(level.KnownTerrainChanges.Count * 2);
                        if (level.KnownTerrainChanges.Count > 0)
                        {
                            foreach (var terrainChange in level.KnownTerrainChanges)
                            {
                                knownTerrainChanges.Add(terrainChange.Key);
                                knownTerrainChanges.Add(terrainChange.Value);

                                wallNeighboursChanges.Add(terrainChange.Key);
                                wallNeighboursChanges.Add(level.WallNeighbours[terrainChange.Key] &
                                                          (byte)DirectionFlags.Cross);
                            }
                        }

                        if (level.TerrainChanges.Count > 0)
                        {
                            foreach (var terrainChange in level.TerrainChanges)
                            {
                                if (level.VisibleTerrain[terrainChange.Key] == 0)
                                {
                                    continue;
                                }

                                knownTerrainChanges.Add(terrainChange.Key);
                                knownTerrainChanges.Add(terrainChange.Value);
                            }
                        }

                        if (knownTerrainChanges.Count > 0)
                        {
                            properties.Add(i);
                            properties.Add(knownTerrainChanges);
                        }

                        i++;
                        if (level.WallNeighboursChanges.Count > 0)
                        {
                            foreach (var wallNeighboursChange in level.WallNeighboursChanges)
                            {
                                if (level.VisibleTerrain[wallNeighboursChange.Key] == 0)
                                {
                                    continue;
                                }

                                wallNeighboursChanges.Add(wallNeighboursChange.Key);
                                wallNeighboursChanges.Add(wallNeighboursChange.Value & (byte)DirectionFlags.Cross);
                            }
                        }

                        if (wallNeighboursChanges.Count > 0)
                        {
                            properties.Add(i);
                            properties.Add(wallNeighboursChanges);
                        }

                        i++;
                        if (level.VisibleTerrainChanges.Count > 0)
                        {
                            properties.Add(i);
                            var changes = new object[level.VisibleTerrainChanges.Count * 2];
                            var j = 0;
                            foreach (var visibleTerrainChange in level.VisibleTerrainChanges)
                            {
                                changes[j++] = visibleTerrainChange.Key;
                                changes[j++] = visibleTerrainChange.Value;
                            }

                            properties.Add(changes);
                        }
                    }

                    return properties.Count > 1 ? properties : null;
            }
        }

        private static IEnumerable<GameEntity> GetConnections(GameEntity levelEntity, GameManager manager)
            => manager.LevelKnowledgesToLevelRelationship[levelEntity.Id]
                .Select(c => c.Knowledge)
                .Where(c => c.KnownEntity.HasComponent(EntityComponent.Connection))
                .Select(c => c.Entity);

        private static IEnumerable<GameEntity> GetItems(GameEntity levelEntity, GameManager manager)
            => manager.LevelKnowledgesToLevelRelationship[levelEntity.Id]
                .Select(t => t.Knowledge)
                .Where(t => t.KnownEntity.HasComponent(EntityComponent.Item))
                .Select(t => t.Entity);

        private static IEnumerable<GameEntity> GetActors(GameEntity levelEntity, GameManager manager)
            => manager.LevelKnowledgesToLevelRelationship[levelEntity.Id]
                .Select(a => a.Knowledge)
                .Where(a => a.KnownEntity.HasComponent(EntityComponent.Being))
                .Select(a => a.Entity);
    }
}
