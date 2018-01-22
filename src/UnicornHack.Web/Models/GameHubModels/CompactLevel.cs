using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Data;

namespace UnicornHack.Models.GameHubModels
{
    public static class CompactLevel
    {
        public static List<object> Serialize(
            Level level, EntityState state, int previousTick, SerializationContext context,
            Dictionary<int, byte> visibleTerrainChanges)
        {
            if (state == EntityState.Added)
            {
                return new List<object>(12)
                {
                    (int)state,
                    level.CurrentTick,
                    level.Actors.Select(a => CompactActor.Serialize(a, null, context)).ToList(),
                    level.Items.Select(t => CompactItem.Serialize(t, null, context)).ToList(),
                    level.Connections.Select(c => CompactConnection.Serialize(c, null, context)).ToList(),
                    level.Terrain,
                    level.WallNeighbours,
                    level.VisibleTerrain,
                    level.BranchName,
                    level.Depth,
                    level.Width,
                    level.Height
                };
            }

            var dbContext = context.Context;
            var levelEntry = dbContext.Entry(level);
            var properties = new List<object> {(int)state, previousTick, level.CurrentTick};

            var i = 2;
            var serializedActors = CollectionChanges
                .SerializeCollection(level.Actors, CompactActor.Serialize, context);
            if (serializedActors.Count > 0)
            {
                properties.Add(i);
                properties.Add(serializedActors);
            }

            i++;
            var serializedItems = CollectionChanges
                .SerializeCollection(level.Items, CompactItem.Serialize, context);
            if (serializedItems.Count > 0)
            {
                properties.Add(i);
                properties.Add(serializedItems);
            }

            i++;
            var serializedConnections = CollectionChanges
                .SerializeCollection(level.Connections, CompactConnection.Serialize, context);
            if (serializedConnections.Count > 0)
            {
                properties.Add(i);
                properties.Add(serializedConnections);
            }

            if (levelEntry.State != EntityState.Unchanged)
            {
                i++;
                if (level.TerrainChanges.Count > 0)
                {
                    properties.Add(i);
                    var changes = new object[level.TerrainChanges.Count * 2];
                    var j = 0;
                    foreach (var levelTerrainChange in level.TerrainChanges)
                    {
                        changes[j++] = levelTerrainChange.Key;
                        changes[j++] = levelTerrainChange.Value;
                    }

                    properties.Add(changes);
                }

                i++;
                if (level.WallNeighboursChanges.Count > 0)
                {
                    properties.Add(i);
                    var changes = new object[level.WallNeighboursChanges.Count * 2];
                    var j = 0;
                    foreach (var levelWallNeighboursChange in level.WallNeighboursChanges)
                    {
                        changes[j++] = levelWallNeighboursChange.Key;
                        changes[j++] = levelWallNeighboursChange.Value;
                    }

                    properties.Add(changes);
                }

                i++;
                if (visibleTerrainChanges.Count > 0)
                {
                    properties.Add(i);
                    // TODO: send the whole array if too many changes
                    var changes = new object[visibleTerrainChanges.Count * 2];
                    var j = 0;
                    foreach (var levelVisibleTerrainChange in visibleTerrainChanges)
                    {
                        changes[j++] = levelVisibleTerrainChange.Key;
                        changes[j++] = levelVisibleTerrainChange.Value;
                    }

                    properties.Add(changes);
                }
            }

            return properties.Count > 3 ? properties : null;
        }

        public static void Snapshot(Level level)
        {
            level.Actors.CreateSnapshot();
            foreach (var actor in level.Actors)
            {
                CompactActor.Snapshot(actor);
            }

            level.Items.CreateSnapshot();
            foreach (var item in level.Items)
            {
                CompactItem.Snapshot(item);
            }

            level.Connections.CreateSnapshot();

            if (level.TerrainChanges != null)
            {
                level.TerrainChanges.Clear();
            }
            else
            {
                level.TerrainChanges = new Dictionary<int, byte>();
            }

            if (level.WallNeighboursChanges != null)
            {
                level.WallNeighboursChanges.Clear();
            }
            else
            {
                level.WallNeighboursChanges = new Dictionary<int, byte>();
            }

            level.VisibleNeighboursChanged = false;
        }

        public static Dictionary<int, byte> DetectVisibilityChanges(Level level, byte[] oldVisibleTerrain)
        {
            var visibleTerrainChanges = new Dictionary<int, byte>();

            for (int i = 0; i < level.VisibleTerrain.Length; i++)
            {
                var newValue = level.VisibleTerrain[i];
                if (newValue != oldVisibleTerrain[i])
                {
                    visibleTerrainChanges.Add(i, newValue);
                }
            }

            return visibleTerrainChanges;
        }
    }
}