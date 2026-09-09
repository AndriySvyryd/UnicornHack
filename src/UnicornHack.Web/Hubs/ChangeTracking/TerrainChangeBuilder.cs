using UnicornHack.Systems.Levels;

namespace UnicornHack.Hubs.ChangeTracking;

public static class TerrainChangeBuilder
{
    public static bool HasChanges(RoomComponent room)
        => (room.KnownTerrainChanges?.Count > 0)
           || (room.TerrainChanges?.Count > 0)
           || (room.NeighborChanges?.Count > 0)
           || (room.VisibleTerrainChanges?.Count > 0);

    public static void WriteToRoomChange(RoomComponent room, RoomChange change)
    {
        var knownTerrainChangeCount = room.KnownTerrainChanges!.Count;
        var wallNeighborsChangeCount = knownTerrainChangeCount;
        foreach (var terrainChange in room.TerrainChanges!)
        {
            if (room.VisibleTerrain[terrainChange.Key] != 0)
            {
                knownTerrainChangeCount++;
            }
        }

        foreach (var neighborChange in room.NeighborChanges!)
        {
            if (room.VisibleTerrain[neighborChange.Key] != 0)
            {
                wallNeighborsChangeCount++;
            }
        }

        var visibleTerrainChangeCount = room.VisibleTerrainChanges!.Count;
        var mapChanges = CreateMapChanges(
            room, change, knownTerrainChangeCount, wallNeighborsChangeCount, visibleTerrainChangeCount);
        if (mapChanges == null)
        {
            return;
        }

        if (knownTerrainChangeCount != 0)
        {
            var knownTerrainChanges = mapChanges.TerrainChanges!;
            foreach (var terrainChange in room.KnownTerrainChanges)
            {
                knownTerrainChanges.Add((terrainChange.Key, terrainChange.Value));
            }

            foreach (var terrainChange in room.TerrainChanges)
            {
                if (room.VisibleTerrain[terrainChange.Key] != 0)
                {
                    knownTerrainChanges.Add((terrainChange.Key, terrainChange.Value));
                }
            }
        }

        if (wallNeighborsChangeCount != 0)
        {
            var wallNeighborsChanges = mapChanges.WallNeighborsChanges!;
            foreach (var terrainChange in room.KnownTerrainChanges)
            {
                wallNeighborsChanges.Add((terrainChange.Key, room.Neighbors[terrainChange.Key]));
            }

            foreach (var neighborChange in room.NeighborChanges)
            {
                if (room.VisibleTerrain[neighborChange.Key] != 0)
                {
                    wallNeighborsChanges.Add((neighborChange.Key, neighborChange.Value));
                }
            }
        }

        if (visibleTerrainChangeCount != 0)
        {
            var visibleTerrainChanges = mapChanges.VisibleTerrainChanges!;
            foreach (var visibleTerrainChange in room.VisibleTerrainChanges)
            {
                visibleTerrainChanges.Add((visibleTerrainChange.Key, visibleTerrainChange.Value));
            }
        }

        change.LevelMapChanges = mapChanges;
    }

    public static void WriteSnapshotToRoomChange(RoomComponent room, RoomChange change)
    {
        var tileCount = room.TileCount;
        var terrainCount = 0;
        var wallNeighborsCount = 0;
        var visibleTerrainCount = 0;
        for (short i = 0; i < tileCount; i++)
        {
            if (room.KnownTerrain[i] != MapFeature.Unexplored)
            {
                terrainCount++;
                if (room.Neighbors[i] != DirectionFlags.None)
                {
                    wallNeighborsCount++;
                }
            }

            if (room.VisibleTerrain[i] != 0)
            {
                visibleTerrainCount++;
            }
        }

        var mapChanges = CreateMapChanges(
            room, change, terrainCount, wallNeighborsCount, visibleTerrainCount);
        if (mapChanges == null)
        {
            return;
        }

        var terrainChanges = mapChanges.TerrainChanges;
        var wallNeighborsChanges = mapChanges.WallNeighborsChanges;
        var visibleTerrainChanges = mapChanges.VisibleTerrainChanges;
        for (short i = 0; i < tileCount; i++)
        {
            var feature = room.KnownTerrain[i];
            if (feature != MapFeature.Unexplored)
            {
                terrainChanges!.Add((i, feature));
                var neighbors = room.Neighbors[i];
                if (neighbors != DirectionFlags.None)
                {
                    wallNeighborsChanges!.Add((i, neighbors));
                }
            }

            var visibility = room.VisibleTerrain[i];
            if (visibility != 0)
            {
                visibleTerrainChanges!.Add((i, visibility));
            }
        }

        change.LevelMapChanges = mapChanges;
    }

    private static RoomMapChanges? CreateMapChanges(
        RoomComponent room,
        RoomChange change,
        int terrainCount,
        int wallNeighborsCount,
        int visibleTerrainCount)
    {
        var changeCount = terrainCount + wallNeighborsCount + visibleTerrainCount;
        if (changeCount == 0)
        {
            return null;
        }

        if (changeCount > room.TileCount)
        {
            var tileCount = room.TileCount;
            var terrain = new MapFeature[tileCount];
            var visibleTerrain = new byte[tileCount];
            Array.Copy(room.KnownTerrain, terrain, tileCount);
            Array.Copy(room.VisibleTerrain, visibleTerrain, tileCount);
            change.LevelMap = new RoomMap
            {
                Terrain = terrain,
                WallNeighbors = MaskWallNeighborsToExplored(room),
                VisibleTerrain = visibleTerrain
            };
            return null;
        }

        return new RoomMapChanges
        {
            ChangedProperties = null,
            TerrainChanges = terrainCount == 0 ? null : new List<(short, MapFeature)>(terrainCount),
            WallNeighborsChanges = wallNeighborsCount == 0
                ? null
                : new List<(short, DirectionFlags)>(wallNeighborsCount),
            VisibleTerrainChanges = visibleTerrainCount == 0
                ? null
                : new List<(short, byte)>(visibleTerrainCount)
        };
    }

    /// <summary>
    ///     Builds a copy of <see cref="RoomComponent.Neighbors" /> with bits zeroed for
    ///     tiles the player hasn't explored. The client only stores wall-neighbor bits for
    ///     tiles whose <c>KnownTerrain</c> is set (see <see cref="WriteToRoomChange" /> and
    ///     <c>roomChangeBuilder.SerializeMap</c>, which both gate WallNeighbors emission on
    ///     <c>KnownTerrain != Unexplored</c> / <c>VisibleTerrain != 0</c>). Sending raw
    ///     <see cref="RoomComponent.Neighbors" /> in a full <see cref="RoomMap" />
    ///     would leak bits for unexplored tiles and would not match what the next
    ///     <c>GetState</c> reports, breaking client state convergence.
    /// </summary>
    public static DirectionFlags[] MaskWallNeighborsToExplored(RoomComponent room)
    {
        var tileCount = room.TileCount;
        var masked = new DirectionFlags[tileCount];
        for (var i = 0; i < tileCount; i++)
        {
            if (room.KnownTerrain[i] != MapFeature.Unexplored)
            {
                masked[i] = room.Neighbors[i];
            }
        }

        return masked;
    }
}
