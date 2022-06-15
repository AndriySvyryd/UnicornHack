using System.Collections.Generic;
using UnicornHack.Primitives;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Systems.Levels;

public static class TerrainSystem
{
    public static void SetTerrain(MapFeature feature, Point point, LevelComponent level)
    {
        var index = level.PointToIndex[point.X, point.Y];
        level.Terrain[index] = (byte)feature;
        if (level.TerrainChanges != null)
        {
            level.TerrainChanges[index] = (byte)feature;
            if (level.VisibleTerrain[index] != 0)
            {
                level.KnownTerrain[index] = (byte)feature;
                level.KnownTerrainChanges[index] = (byte)feature;
            }
        }

        switch (feature)
        {
            case MapFeature.Pool:
            case MapFeature.RockFloor:
            case MapFeature.StoneFloor:
                if (ModifyNeighbors(level, level.VisibleNeighbors, null, point, add: true))
                {
                    level.VisibleNeighborsChanged = true;
                }

                if (level.TerrainChanges != null)
                {
                    ModifyNeighbors(level, level.WallNeighbors,
                        level.WallNeighborsChanges, point, add: false);
                }

                break;
            case MapFeature.StoneArchway:
            case MapFeature.StoneWall:
                ModifyNeighbors(level, level.WallNeighbors,
                    level.WallNeighborsChanges, point, add: true);

                if (level.TerrainChanges != null
                    && ModifyNeighbors(level, level.VisibleNeighbors, null, point, add: false))
                {
                    level.VisibleNeighborsChanged = true;
                }

                break;
        }
    }

    private static bool ModifyNeighbors(LevelComponent level, byte[] neighbors, Dictionary<int, byte> changes,
        Point point, bool add)
    {
        var changed = false;
        for (var directionIndex = 0; directionIndex < 8; directionIndex++)
        {
            var direction = Vector.MovementDirections[directionIndex];
            var newLocation = point.Translate(direction);

            if (!level.IsValid(newLocation))
            {
                continue;
            }

            var newLocationIndex = level.PointToIndex[newLocation.X, newLocation.Y];
            var neighborBit = (byte)(1 << Vector.OppositeDirectionIndexes[directionIndex]);
            var oldValue = neighbors[newLocationIndex];
            var newValue = add
                ? (byte)(oldValue | neighborBit)
                : (byte)(oldValue & (byte)~neighborBit);

            if (oldValue != newValue)
            {
                changed = true;
                neighbors[newLocationIndex] = newValue;
                if (changes != null)
                {
                    changes[newLocationIndex] = newValue;
                }
            }
        }

        return changed;
    }

    public static IEnumerable<Point> GetAdjacentPoints(
        LevelComponent level, Point point, Direction startingDirection, bool includeInitial = false)
    {
        if (includeInitial)
        {
            yield return point;
        }

        for (var i = 0; i < 8; i++)
        {
            var newPoint = point.Translate(startingDirection.Rotate(i).AsVector());
            if (newPoint.X < level.Width
                && newPoint.Y < level.Height)
            {
                yield return newPoint;
            }
        }
    }
}
