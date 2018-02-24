using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Data;

namespace UnicornHack.Models.GameHubModels
{
    public static class CompactLevel
    {
        public static List<object> Serialize(
            Level level, EntityState? state, SerializationContext context)
        {
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

                    properties.Add(level.ActorsKnowledge.Select(a => CompactActor.Serialize(a, null, context))
                        .ToList());
                    properties.Add(level.ItemsKnowledge.Select(t => CompactItem.Serialize(t, null, context)).ToList());
                    properties.Add(level.Connections.Where(c => c.Known)
                        .Select(c => CompactConnection.Serialize(c, null, context)).ToList());
                    properties.Add(knownTerrain);
                    properties.Add(wallNeighbours);
                    properties.Add(visibleTerrain);
                    properties.Add(level.BranchName);
                    properties.Add(level.Depth);
                    properties.Add(level.Width);
                    properties.Add(level.Height);
                    return properties;
            }

            var dbContext = context.Context;
            var levelEntry = dbContext.Entry(level);
            properties = new List<object> {(int)state};

            var i = 2;
            var serializedActors = CollectionChanges
                .SerializeCollection(level.ActorsKnowledge, CompactActor.Serialize, context);
            if (serializedActors.Count > 0)
            {
                properties.Add(i);
                properties.Add(serializedActors);
            }

            i++;
            var serializedItems = CollectionChanges
                .SerializeCollection(level.ItemsKnowledge, CompactItem.Serialize, context);
            if (serializedItems.Count > 0)
            {
                properties.Add(i);
                properties.Add(serializedItems);
            }

            i++;
            var serializedConnections = CollectionChanges
                .SerializeCollection(level.Connections, CompactConnection.Serialize,
                    v => (bool)v[nameof(Connection.Known)], context);
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
                        wallNeighboursChanges.Add(level.WallNeighbours[terrainChange.Key] & (byte)DirectionFlags.Cross);
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
}