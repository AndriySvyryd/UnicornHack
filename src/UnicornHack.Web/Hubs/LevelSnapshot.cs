using System.Collections;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs;

public class LevelSnapshot
{
    private Dictionary<GameEntity, LevelActorSnapshot> ActorsSnapshot { get; } = new(EntityEqualityComparer<GameEntity>.Instance);
    private Dictionary<GameEntity, LevelItemSnapshot> ItemsSnapshot { get; } = new(EntityEqualityComparer<GameEntity>.Instance);
    private HashSet<GameEntity> ConnectionsSnapshot { get; } = new(EntityEqualityComparer<GameEntity>.Instance);

    private readonly HashSet<GameEntity> _tempHashSet = new(EntityEqualityComparer<GameEntity>.Instance);

    private readonly Dictionary<GameEntity, LevelActorSnapshot> _tempActors =
        new(EntityEqualityComparer<GameEntity>.Instance);

    private readonly Dictionary<GameEntity, LevelItemSnapshot> _tempItems =
        new(EntityEqualityComparer<GameEntity>.Instance);

    public LevelSnapshot CaptureState(GameEntity levelEntity, SerializationContext context)
    {
        CaptureVisibleTerrain(levelEntity);

        var actors = GetActors(levelEntity);
        ActorsSnapshot.Clear();
        ActorsSnapshot.AddRange(actors, i => new LevelActorSnapshot().CaptureState(i, context));

        var items = GetItems(levelEntity);
        ItemsSnapshot.Clear();
        ItemsSnapshot.AddRange(items, i => new LevelItemSnapshot().CaptureState(i, context));

        var connections = GetConnections(levelEntity);
        ConnectionsSnapshot.Clear();
        ConnectionsSnapshot.AddRange(connections);

        return this;
    }

    private static void CaptureVisibleTerrain(GameEntity levelEntity)
    {
        var level = levelEntity.Level!;
        if (level.VisibleTerrainSnapshot == null)
        {
            level.VisibleTerrainSnapshot = (byte[])level.VisibleTerrain.Clone();
        }
        else
        {
            level.VisibleTerrain.CopyTo(level.VisibleTerrainSnapshot, 0);
        }
    }

    public static List<object?>? Serialize(
        GameEntity levelEntity, EntityState? state, LevelSnapshot? snapshot, SerializationContext context)
    {
        var level = levelEntity.Level!;
        List<object?> properties;
        var tileCount = level.TileCount;

        switch (state)
        {
            case null:
            case EntityState.Added:
                if (snapshot != null)
                {
                    CaptureVisibleTerrain(levelEntity);
                }

                properties = new List<object?>(10) { null };

                var actors = new Dictionary<int, List<object?>>();
                foreach (var actor in GetActors(levelEntity))
                {
                    LevelActorSnapshot? actorSnapshot = null;
                    if (snapshot != null
                        && !snapshot.ActorsSnapshot.TryGetValue(actor, out actorSnapshot))
                    {
                        actorSnapshot = new LevelActorSnapshot();
                        snapshot.ActorsSnapshot[actor] = actorSnapshot;
                    }

                    actors.Add(actor.Id, LevelActorSnapshot.Serialize(actor, null, actorSnapshot!, context)!);
                }

                properties.Add(actors);

                var items = new Dictionary<int, List<object?>>();
                foreach (var item in GetItems(levelEntity))
                {
                    LevelItemSnapshot? itemSnapshot = null;
                    if (snapshot != null
                        && !snapshot.ItemsSnapshot.TryGetValue(item, out itemSnapshot))
                    {
                        itemSnapshot = new LevelItemSnapshot();
                        snapshot.ItemsSnapshot[item] = itemSnapshot;
                    }

                    items.Add(item.Id, LevelItemSnapshot.Serialize(item, null, itemSnapshot!, context)!);
                }

                properties.Add(items);

                var connections = new Dictionary<int, List<object?>>();
                foreach (var connection in GetConnections(levelEntity))
                {
                    snapshot?.ConnectionsSnapshot.Add(connection);

                    connections.Add(connection.Id, ConnectionSnapshot.Serialize(connection, null, context)!);
                }

                properties.Add(connections);
                SerializeMap(level, properties);
                properties.Add(level.BranchName);
                properties.Add(level.Depth);
                properties.Add(level.Width);
                properties.Add(level.Height);
                return properties;
            default:
                var i = 0;
                var setValues = new BitArray(10);
                setValues[i++] = true;
                properties = [setValues];
                var levelEntry = context.DbContext.Entry(level);

                var serializedActors = GameTransmissionProtocol.Serialize(
                    GetActors(levelEntity),
                    snapshot!.ActorsSnapshot,
                    LevelActorSnapshot.Serialize,
                    snapshot._tempActors,
                    context);
                if (serializedActors.Count > 0)
                {
                    setValues[i] = true;
                    properties.Add(serializedActors);
                }
                i++;

                var serializedItems = GameTransmissionProtocol.Serialize(
                    GetItems(levelEntity),
                    snapshot.ItemsSnapshot,
                    LevelItemSnapshot.Serialize,
                    snapshot._tempItems,
                    context);
                if (serializedItems.Count > 0)
                {
                    setValues[i] = true;
                    properties.Add(serializedItems);
                }
                i++;

                var serializedConnections = GameTransmissionProtocol.Serialize(
                    GetConnections(levelEntity),
                    snapshot.ConnectionsSnapshot,
                    ConnectionSnapshot.Serialize,
                    snapshot._tempHashSet,
                    context);
                if (serializedConnections.Count > 0)
                {
                    setValues[i] = true;
                    properties.Add(serializedConnections);
                }
                i++;

                if (levelEntry.State != EntityState.Unchanged)
                {
                    var changes = SerializeMapChanges(level, out var fullMap);
                    if (changes != null)
                    {
                        properties.Add(changes);
                        setValues[i++] = fullMap;
                        setValues[i++] = !fullMap;
                    }
                    else
                    {
                        i += 2;
                    }

                    CaptureVisibleTerrain(levelEntity);
                }
                else
                {
                    i += 2;
                }

                i += 4;
                Debug.Assert(i == 10);
                return properties.Count > 1 ? properties : null;
        }
    }

    private static List<object?>? SerializeMap(LevelComponent level, List<object?> properties)
    {
        var tileCount = level.TileCount;
        var knownTerrainChanges = new List<(short, byte)>();
        var wallNeighborsChanges = new List<(short, byte)>();
        for (short j = 0; j < tileCount; j++)
        {
            var feature = level.KnownTerrain[j];
            if (feature == (byte)MapFeature.Unexplored)
            {
                continue;
            }

            knownTerrainChanges.Add((j, feature));
            var neighbors = level.WallNeighbors[j] & (byte)DirectionFlags.Cross;
            if (neighbors != (byte)DirectionFlags.None)
            {
                wallNeighborsChanges.Add((j, (byte)neighbors));
            }
        }

        var visibleTerrainChanges = new List<(short, byte)>();
        for (short j = 0; j < tileCount; j++)
        {
            var visibility = level.VisibleTerrain[j];
            if (visibility != 0)
            {
                visibleTerrainChanges.Add((j, visibility));
            }
        }

        if (knownTerrainChanges.Count == 0 && wallNeighborsChanges.Count == 0 && visibleTerrainChanges.Count == 0)
        {
            properties.Add(null);
            properties.Add(null);
            return null;
        }
        else if ((knownTerrainChanges.Count + wallNeighborsChanges.Count + visibleTerrainChanges.Count) > tileCount)
        {
            var levelMap = new List<object?>
            {
                level.KnownTerrain,
                level.WallNeighbors,
                level.VisibleTerrain
            };
            properties.Add(levelMap);
            properties.Add(null);
            return levelMap;
        }
        else
        {
            var levelMap = new List<object?>();
            if (knownTerrainChanges.Count == 0 && wallNeighborsChanges.Count == 0)
            {
                levelMap.Add(new BitArray([true, false, false, true]));
                levelMap.Add(visibleTerrainChanges);
            }
            else
            {
                levelMap.Add(null);
                levelMap.Add(knownTerrainChanges);
                levelMap.Add(wallNeighborsChanges);
                levelMap.Add(visibleTerrainChanges);
            }

            properties.Add(null);
            properties.Add(levelMap);
            return levelMap;
        }
    }

    private static List<object?>? SerializeMapChanges(LevelComponent level, out bool fullMap)
    {
        fullMap = false;
        var wallNeighborsChanges = new List<(short, byte)>();
        var knownTerrainChanges = new List<(short, byte)>(level.KnownTerrainChanges!.Count);
        if (level.KnownTerrainChanges.Count > 0)
        {
            foreach (var terrainChange in level.KnownTerrainChanges)
            {
                knownTerrainChanges.Add((terrainChange.Key, terrainChange.Value));

                wallNeighborsChanges.Add((terrainChange.Key,
                    (byte)(level.WallNeighbors[terrainChange.Key] & (byte)DirectionFlags.Cross)));
            }
        }

        if (level.TerrainChanges!.Count > 0)
        {
            foreach (var terrainChange in level.TerrainChanges)
            {
                if (level.VisibleTerrain[terrainChange.Key] == 0)
                {
                    continue;
                }

                knownTerrainChanges.Add((terrainChange.Key, terrainChange.Value));
            }
        }

        if (level.WallNeighborsChanges!.Count > 0)
        {
            foreach (var wallNeighborsChange in level.WallNeighborsChanges)
            {
                if (level.VisibleTerrain[wallNeighborsChange.Key] == 0)
                {
                    continue;
                }

                wallNeighborsChanges.Add((wallNeighborsChange.Key,
                    (byte)(wallNeighborsChange.Value & (byte)DirectionFlags.Cross)));
            }
        }

        var changesCount = knownTerrainChanges.Count + wallNeighborsChanges.Count + level.VisibleTerrainChanges!.Count;
        if (changesCount == 0)
        {
            return null;
        }

        if (changesCount > level.TileCount)
        {
            fullMap = true;

            return
            [
                level.KnownTerrain,
                level.WallNeighbors,
                level.VisibleTerrain
            ];
        }

        var visibleTerrainChanges = new List<(short, byte)>(level.VisibleTerrainChanges!.Count);
        foreach (var visibleTerrainChange in level.VisibleTerrainChanges)
        {
            visibleTerrainChanges.Add((visibleTerrainChange.Key, visibleTerrainChange.Value));
        }

        if (knownTerrainChanges.Count == 0 && wallNeighborsChanges.Count == 0)
        {
            return
            [
                new BitArray([true, false, false, true]),
                visibleTerrainChanges
            ];
        }
        else
        {
            return
            [
                null,
                knownTerrainChanges,
                wallNeighborsChanges,
                visibleTerrainChanges
            ];
        }
    }

    private static IEnumerable<GameEntity> GetConnections(GameEntity levelEntity)
        => levelEntity.Level!.KnownConnections.Values
            .Select(c => c.Knowledge!)
            .Where(c => c.KnownEntity.Connection!.Direction == null
                        || (c.KnownEntity.Connection.Direction & ConnectionDirection.Source) != 0)
            .Select(c => c.Entity);

    private static IEnumerable<GameEntity> GetItems(GameEntity levelEntity)
        => levelEntity.Level!.KnownItems.Values
            .Select(t => t.Knowledge!)
            .Select(t => t.Entity);

    private static IEnumerable<GameEntity> GetActors(GameEntity levelEntity)
        => levelEntity.Level!.KnownActors.Values
            .Select(a => a.Knowledge!)
            .Select(a => a.Entity);
}
