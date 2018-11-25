using System.Collections.Generic;
using UnicornHack.Primitives;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Systems.Levels
{
    public static class TerrainSystem
    {
        public static void SetTerrain(MapFeature feature, Point point, LevelComponent levelComponent)
        {
            var index = levelComponent.PointToIndex[point.X, point.Y];
            levelComponent.Terrain[index] = (byte)feature;
            if (levelComponent.TerrainChanges != null)
            {
                levelComponent.TerrainChanges[index] = (byte)feature;
                if (levelComponent.VisibleTerrain[index] != 0)
                {
                    levelComponent.KnownTerrain[index] = (byte)feature;
                    levelComponent.KnownTerrainChanges[index] = (byte)feature;
                }
            }

            switch (feature)
            {
                case MapFeature.Pool:
                case MapFeature.RockFloor:
                case MapFeature.StoneFloor:
                    if (ModifyNeighbours(levelComponent, levelComponent.VisibleNeighbours, null, point, add: true))
                    {
                        levelComponent.VisibleNeighboursChanged = true;
                    }

                    if (levelComponent.TerrainChanges != null)
                    {
                        ModifyNeighbours(levelComponent, levelComponent.WallNeighbours,
                            levelComponent.WallNeighboursChanges, point, add: false);
                    }

                    break;
                case MapFeature.StoneArchway:
                case MapFeature.StoneWall:
                    ModifyNeighbours(levelComponent, levelComponent.WallNeighbours,
                        levelComponent.WallNeighboursChanges, point, add: true);

                    if (levelComponent.TerrainChanges != null
                        && ModifyNeighbours(levelComponent, levelComponent.VisibleNeighbours, null, point, add: false))
                    {
                        levelComponent.VisibleNeighboursChanged = true;
                    }

                    break;
            }
        }

        private static bool ModifyNeighbours(
            LevelComponent levelComponent, byte[] neighbours, Dictionary<int, byte> changes, Point point, bool add)
        {
            var changed = false;
            for (var directionIndex = 0; directionIndex < 8; directionIndex++)
            {
                var direction = Vector.MovementDirections[directionIndex];
                var newLocation = point.Translate(direction);

                if (!levelComponent.IsValid(newLocation))
                {
                    continue;
                }

                var newLocationIndex = levelComponent.PointToIndex[newLocation.X, newLocation.Y];
                var neighbourBit = (byte)(1 << Vector.OppositeDirectionIndexes[directionIndex]);
                var oldValue = neighbours[newLocationIndex];
                var newValue = add
                    ? (byte)(oldValue | neighbourBit)
                    : (byte)(oldValue & (byte)~neighbourBit);

                if (oldValue != newValue)
                {
                    changed = true;
                    neighbours[newLocationIndex] = newValue;
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
}
