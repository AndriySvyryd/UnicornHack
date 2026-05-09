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
        var wallNeighborsChanges = new List<(short, DirectionFlags)>();
        var knownTerrainChanges = new List<(short, MapFeature)>(room.KnownTerrainChanges!.Count);
        if (room.KnownTerrainChanges.Count > 0)
        {
            foreach (var terrainChange in room.KnownTerrainChanges)
            {
                knownTerrainChanges.Add((terrainChange.Key, terrainChange.Value));
                wallNeighborsChanges.Add((terrainChange.Key, room.Neighbors[terrainChange.Key]));
            }
        }

        if (room.TerrainChanges!.Count > 0)
        {
            foreach (var terrainChange in room.TerrainChanges)
            {
                if (room.VisibleTerrain[terrainChange.Key] == 0)
                {
                    continue;
                }

                knownTerrainChanges.Add((terrainChange.Key, terrainChange.Value));
            }
        }

        if (room.NeighborChanges!.Count > 0)
        {
            foreach (var wallNeighborsChange in room.NeighborChanges)
            {
                if (room.VisibleTerrain[wallNeighborsChange.Key] == 0)
                {
                    continue;
                }

                wallNeighborsChanges.Add((wallNeighborsChange.Key, wallNeighborsChange.Value));
            }
        }

        var changesCount = knownTerrainChanges.Count + wallNeighborsChanges.Count + room.VisibleTerrainChanges!.Count;
        if (changesCount == 0)
        {
            return;
        }

        if (changesCount > room.TileCount)
        {
            change.LevelMap = new RoomMap
            {
                Terrain = room.KnownTerrain,
                WallNeighbors = MaskWallNeighborsToExplored(room),
                VisibleTerrain = room.VisibleTerrain
            };
            return;
        }

        var visibleTerrainChanges = new List<(short, byte)>(room.VisibleTerrainChanges!.Count);
        foreach (var visibleTerrainChange in room.VisibleTerrainChanges)
        {
            visibleTerrainChanges.Add((visibleTerrainChange.Key, visibleTerrainChange.Value));
        }

        if (knownTerrainChanges.Count == 0 && wallNeighborsChanges.Count == 0)
        {
            change.LevelMapChanges = new RoomMapChanges
            {
                VisibleTerrainChanges = visibleTerrainChanges
            };
            return;
        }

        change.LevelMapChanges = new RoomMapChanges
        {
            ChangedProperties = null,
            TerrainChanges = knownTerrainChanges,
            WallNeighborsChanges = wallNeighborsChanges,
            VisibleTerrainChanges = visibleTerrainChanges
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
