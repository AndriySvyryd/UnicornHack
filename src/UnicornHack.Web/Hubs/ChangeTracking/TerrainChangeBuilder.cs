using UnicornHack.Systems.Levels;

namespace UnicornHack.Hubs.ChangeTracking;

public static class TerrainChangeBuilder
{
    public static bool HasChanges(LevelComponent level)
        => (level.KnownTerrainChanges?.Count > 0)
           || (level.TerrainChanges?.Count > 0)
           || (level.WallNeighborsChanges?.Count > 0)
           || (level.VisibleTerrainChanges?.Count > 0);

    public static void WriteToLevelChange(LevelComponent level, LevelChange change)
    {
        var wallNeighborsChanges = new List<(short, byte)>();
        var knownTerrainChanges = new List<(short, byte)>(level.KnownTerrainChanges!.Count);
        if (level.KnownTerrainChanges.Count > 0)
        {
            foreach (var terrainChange in level.KnownTerrainChanges)
            {
                knownTerrainChanges.Add((terrainChange.Key, terrainChange.Value));
                wallNeighborsChanges.Add((terrainChange.Key, level.WallNeighbors[terrainChange.Key]));
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

                wallNeighborsChanges.Add((wallNeighborsChange.Key, wallNeighborsChange.Value));
            }
        }

        var changesCount = knownTerrainChanges.Count + wallNeighborsChanges.Count + level.VisibleTerrainChanges!.Count;
        if (changesCount == 0)
        {
            return;
        }

        if (changesCount > level.TileCount)
        {
            change.LevelMap = new LevelMap
            {
                Terrain = level.KnownTerrain,
                WallNeighbors = MaskWallNeighborsToExplored(level),
                VisibleTerrain = level.VisibleTerrain
            };
            return;
        }

        var visibleTerrainChanges = new List<(short, byte)>(level.VisibleTerrainChanges!.Count);
        foreach (var visibleTerrainChange in level.VisibleTerrainChanges)
        {
            visibleTerrainChanges.Add((visibleTerrainChange.Key, visibleTerrainChange.Value));
        }

        if (knownTerrainChanges.Count == 0 && wallNeighborsChanges.Count == 0)
        {
            change.LevelMapChanges = new LevelMapChanges
            {
                VisibleTerrainChanges = visibleTerrainChanges
            };
            return;
        }

        change.LevelMapChanges = new LevelMapChanges
        {
            ChangedProperties = null,
            TerrainChanges = knownTerrainChanges,
            WallNeighborsChanges = wallNeighborsChanges,
            VisibleTerrainChanges = visibleTerrainChanges
        };
    }

    /// <summary>
    ///     Builds a copy of <see cref="LevelComponent.WallNeighbors" /> with bits zeroed for
    ///     tiles the player hasn't explored. The client only stores wall-neighbor bits for
    ///     tiles whose <c>KnownTerrain</c> is set (see <see cref="WriteToLevelChange" /> and
    ///     <c>LevelChangeBuilder.SerializeMap</c>, which both gate WallNeighbors emission on
    ///     <c>KnownTerrain != Unexplored</c> / <c>VisibleTerrain != 0</c>). Sending raw
    ///     <see cref="LevelComponent.WallNeighbors" /> in a full <see cref="LevelMap" />
    ///     would leak bits for unexplored tiles and would not match what the next
    ///     <c>GetState</c> reports, breaking client state convergence.
    /// </summary>
    public static byte[] MaskWallNeighborsToExplored(LevelComponent level)
    {
        var tileCount = level.TileCount;
        var masked = new byte[tileCount];
        for (var i = 0; i < tileCount; i++)
        {
            if (level.KnownTerrain[i] != (byte)MapFeature.Unexplored)
            {
                masked[i] = level.WallNeighbors[i];
            }
        }

        return masked;
    }
}
