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
        private Dictionary<GameEntity, LevelActorSnapshot> ActorsSnapshot { get; } =
            new Dictionary<GameEntity, LevelActorSnapshot>(EntityEqualityComparer<GameEntity>.Instance);

        private Dictionary<GameEntity, LevelItemSnapshot> ItemsSnapshot { get; } =
            new Dictionary<GameEntity, LevelItemSnapshot>(EntityEqualityComparer<GameEntity>.Instance);

        private HashSet<GameEntity> ConnectionsSnapshot { get; } =
            new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);

        private readonly HashSet<GameEntity> _tempHashSet = new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);

        private readonly Dictionary<GameEntity, LevelActorSnapshot> _tempActors =
            new Dictionary<GameEntity, LevelActorSnapshot>(EntityEqualityComparer<GameEntity>.Instance);

        private readonly Dictionary<GameEntity, LevelItemSnapshot> _tempItems =
            new Dictionary<GameEntity, LevelItemSnapshot>(EntityEqualityComparer<GameEntity>.Instance);

        public LevelSnapshot CaptureState(GameEntity levelEntity, SerializationContext context)
        {
            CaptureVisibleTerrain(levelEntity);

            var manager = levelEntity.Manager;
            var actors = GetActors(levelEntity, manager);
            ActorsSnapshot.Clear();
            ActorsSnapshot.AddRange(actors, i => new LevelActorSnapshot().CaptureState(i, context));

            var items = GetItems(levelEntity, manager);
            ItemsSnapshot.Clear();
            ItemsSnapshot.AddRange(items, i => new LevelItemSnapshot().CaptureState(i, context));

            var connections = GetConnections(levelEntity, manager);
            ConnectionsSnapshot.Clear();
            ConnectionsSnapshot.AddRange(connections);

            return this;
        }

        private static void CaptureVisibleTerrain(GameEntity levelEntity)
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

                    var wallNeighbors = new List<short>();
                    for (short j = 0; j < level.WallNeighbors.Length; j++)
                    {
                        if (level.KnownTerrain[j] == (byte)MapFeature.Unexplored)
                        {
                            continue;
                        }

                        var neighbors = level.WallNeighbors[j] & (byte)DirectionFlags.Cross;
                        if (neighbors != (byte)DirectionFlags.None)
                        {
                            wallNeighbors.Add(j);
                            wallNeighbors.Add((byte)neighbors);
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

                    if (snapshot != null)
                    {
                        CaptureVisibleTerrain(levelEntity);
                    }

                    properties = state == null
                        ? new List<object>(10)
                        : new List<object>(11) {(int)state};

                    var actors = new List<object>();
                    foreach (var actor in GetActors(levelEntity, manager))
                    {
                        LevelActorSnapshot actorSnapshot = null;
                        if (snapshot != null
                            && !snapshot.ActorsSnapshot.TryGetValue(actor, out actorSnapshot))
                        {
                            actorSnapshot = new LevelActorSnapshot();
                            snapshot.ActorsSnapshot[actor] = actorSnapshot;
                        }

                        actors.Add(LevelActorSnapshot.Serialize(actor, null, actorSnapshot, context));
                    }
                    properties.Add(actors);

                    var items = new List<object>();
                    foreach (var item in GetItems(levelEntity, manager))
                    {
                        LevelItemSnapshot itemSnapshot = null;
                        if (snapshot != null
                            && !snapshot.ItemsSnapshot.TryGetValue(item, out itemSnapshot))
                        {
                            itemSnapshot = new LevelItemSnapshot();
                            snapshot.ItemsSnapshot[item] = itemSnapshot;
                        }

                        items.Add(LevelItemSnapshot.Serialize(item, null, itemSnapshot, context));
                    }
                    properties.Add(items);

                    var connections = new List<object>();
                    foreach (var connection in GetConnections(levelEntity, manager))
                    {
                        snapshot?.ConnectionsSnapshot.Add(connection);

                        connections.Add(ConnectionSnapshot.Serialize(connection, null, context));
                    }
                    properties.Add(connections);

                    properties.Add(knownTerrain);
                    properties.Add(wallNeighbors);
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
                        snapshot._tempActors,
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
                        snapshot._tempItems,
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
                        snapshot._tempHashSet,
                        context);
                    if (serializedConnections.Count > 0)
                    {
                        properties.Add(i);
                        properties.Add(serializedConnections);
                    }

                    if (levelEntry.State != EntityState.Unchanged)
                    {
                        i++;
                        var wallNeighborsChanges = new List<object>();
                        var knownTerrainChanges = new List<object>(level.KnownTerrainChanges.Count * 2);
                        if (level.KnownTerrainChanges.Count > 0)
                        {
                            foreach (var terrainChange in level.KnownTerrainChanges)
                            {
                                knownTerrainChanges.Add(terrainChange.Key);
                                knownTerrainChanges.Add(terrainChange.Value);

                                wallNeighborsChanges.Add(terrainChange.Key);
                                wallNeighborsChanges.Add(level.WallNeighbors[terrainChange.Key] &
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

                                wallNeighborsChanges.Add(wallNeighboursChange.Key);
                                wallNeighborsChanges.Add(wallNeighboursChange.Value & (byte)DirectionFlags.Cross);
                            }
                        }

                        if (wallNeighborsChanges.Count > 0)
                        {
                            properties.Add(i);
                            properties.Add(wallNeighborsChanges);
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

                        CaptureVisibleTerrain(levelEntity);
                    }

                    return properties.Count > 1 ? properties : null;
            }
        }

        private static IEnumerable<GameEntity> GetConnections(GameEntity levelEntity, GameManager manager)
            => manager.LevelKnowledgesToLevelRelationship[levelEntity.Id]
                .Select(c => c.Knowledge)
                .Where(c => c.KnownEntity.HasComponent(EntityComponent.Connection)
                            && (c.KnownEntity.Connection.Direction == null
                                || (c.KnownEntity.Connection.Direction & ConnectionDirection.Source) != 0))
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
