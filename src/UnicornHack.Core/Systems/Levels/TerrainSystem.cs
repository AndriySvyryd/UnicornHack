using UnicornHack.Systems.Actors;

namespace UnicornHack.Systems.Levels;

public static class TerrainSystem
{
    public static bool TryInteract(
        GameEntity actorEntity, Point point, RoomComponent room, GameManager manager)
    {
        if (actorEntity.Player == null
            || actorEntity.Position?.Cell is not { } actorCell
            || actorCell.DistanceTo(point) != 1
            || !room.IsValid(point))
        {
            return false;
        }

        var feature = room.Terrain[room.PointToIndex![point.X, point.Y]];
        return feature switch
        {
            MapFeature.RacePedestal => manager.PlayerChoiceSystem.TryPresent(
                actorEntity, PlayerChoiceSystem.RaceChoice),
            _ => false
        };
    }

    public static bool TryFindAdjacentFeature(
        GameEntity actorEntity,
        MapFeature feature,
        out RoomComponent room,
        out Point featureCell)
    {
        if (actorEntity.Position is not
            {
                Cell: { } actorCell,
                Heading: >= Direction.East and <= Direction.Southeast
            } position)
        {
            room = null!;
            featureCell = default;
            return false;
        }

        room = position.RoomEntity.Room!;
        featureCell = actorCell.Translate(position.Heading.Value.AsVector());
        return room.IsValid(featureCell)
               && room.Terrain[room.PointToIndex![featureCell.X, featureCell.Y]] == feature;
    }

    public static void DeactivateFeature(
        MapFeature feature, Point point, RoomComponent room)
    {
        var inactiveFeature = feature switch
        {
            MapFeature.DeityAltar => MapFeature.InactiveDeityAltar,
            MapFeature.MajorShrine => MapFeature.InactiveMajorShrine,
            MapFeature.MinorShrine => MapFeature.InactiveMinorShrine,
            MapFeature.RitualShrine => MapFeature.InactiveRitualShrine,
            MapFeature.SmallFountain => MapFeature.InactiveSmallFountain,
            MapFeature.BigFountain => MapFeature.InactiveBigFountain,
            MapFeature.SkillPedestal => MapFeature.InactiveSkillPedestal,
            MapFeature.RacePedestal => MapFeature.InactiveRacePedestal,
            MapFeature.TraitPedestal => MapFeature.InactiveTraitPedestal,
            MapFeature.FeatPedestal => MapFeature.InactiveFeatPedestal,
            MapFeature.ArmorStand => MapFeature.InactiveArmorStand,
            MapFeature.ItemCase => MapFeature.InactiveItemCase,
            MapFeature.WeaponStand => MapFeature.InactiveWeaponStand,
            _ => throw new InvalidOperationException(
                $"{feature} does not have an inactive terrain type.")
        };

        SetTerrain(inactiveFeature, point, room);
    }

    public static void SetTerrain(MapFeature feature, Point point, RoomComponent room)
    {
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
