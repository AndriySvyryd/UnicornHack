namespace UnicornHack.Systems.Levels;

public static class TerrainSystem
{
    public static bool TryInteract(
        GameEntity actorEntity, Point point, RoomComponent room, GameManager manager,
        bool pretend = false)
    {
        if (actorEntity.Player == null
            || actorEntity.Position?.Cell is not { } actorCell
            || actorCell.DistanceTo(point) != 1
            || !room.IsValid(point))
        {
            return false;
        }

        if (room.Features.TryGetValue(point, out var featureEntity)
            && featureEntity.Feature!.Template?.RewardType is { } rewardType && rewardType != RewardType.None
            && featureEntity.Feature.Destroyed != true
            && !Actors.PlayerChoiceSystem.HasUsedFeature(featureEntity.Feature, actorEntity))
        {
            return pretend || manager.PlayerChoiceSystem.TryPresentReward(actorEntity, room, point);
        }

        return false;
    }

    public static void SetTerrain(MapFeature feature, Point point, RoomComponent room)
    {
        room.Entity.Manager.CellThreatTracker.TerrainChanged(room);
        var index = room.PointToIndex![point.X, point.Y];
        room.Terrain[index] = feature;
        if (room.TerrainChanges != null)
        {
            room.TerrainChanges[index] = feature;
            if (room.VisibleTerrain[index] != 0)
            {
                room.KnownTerrain[index] = feature;
                if (room.KnownTerrainChanges != null)
                {
                    room.KnownTerrainChanges[index] = feature;
                }
            }
        }

        switch (feature)
        {
            case MapFeature.Pool:
            case MapFeature.RockFloor:
            case MapFeature.StoneFloor:
                if (ModifyNeighbors(room, room.KnownNeighbors, null, point, add: true))
                {
                    room.KnownNeighborsChanged = true;
                }

                if (room.TerrainChanges != null)
                {
                    ModifyNeighbors(room, room.Neighbors,
                        room.NeighborChanges, point, add: false);
                }

                break;
            case MapFeature.StoneArchway:
            case MapFeature.StoneWall:
            case MapFeature.RockWall:
                ModifyNeighbors(room, room.Neighbors,
                    room.NeighborChanges, point, add: true);

                if (room.TerrainChanges != null
                    && ModifyNeighbors(room, room.KnownNeighbors, null, point, add: false))
                {
                    room.KnownNeighborsChanged = true;
                }

                break;
        }
    }

    private static bool ModifyNeighbors(
        RoomComponent room, DirectionFlags[] neighbors, Dictionary<short, DirectionFlags>? changes, Point point, bool add)
    {
        var changed = false;
        for (var directionIndex = 0; directionIndex < 8; directionIndex++)
        {
            var direction = Vector.MovementDirections[directionIndex];
            var newLocation = point.Translate(direction);

            if (!room.IsValid(newLocation))
            {
                continue;
            }

            var newLocationIndex = room.PointToIndex![newLocation.X, newLocation.Y];
            var neighborBit = (DirectionFlags)(1 << Vector.OppositeDirectionIndexes[directionIndex]);
            var oldValue = neighbors[newLocationIndex];
            var newValue = add
                ? oldValue | neighborBit
                : oldValue & ~neighborBit;

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
}
