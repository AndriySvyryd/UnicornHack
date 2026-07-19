using UnicornHack.Data.Branches;
using UnicornHack.Data.Creatures;

namespace UnicornHack.Generation;

public class CreatureGeneratorTest
{
    [Fact]
    public void Shared_properties_fall_back_to_base_creature_and_allow_local_overrides()
    {
        Creature baseCreature = CreatureData.HumanWarrior;
        var derivedCreature = new Creature { BaseName = baseCreature.Name };
        var properties = typeof(Being).GetProperties()
            .Where(property => property.DeclaringType == typeof(Being)
                               && property.CanRead
                               && property.CanWrite
                               && property.Name is not nameof(Being.BaseName)
                                   and not nameof(Being.Name)
                                   and not nameof(Being.AbilityReferences))
            .ToArray();

        foreach (var property in properties)
        {
            var baseValue = property.GetValue(baseCreature);
            Assert.Equal(baseValue, property.GetValue(derivedCreature));

            var overrideValue = GetOverrideValue(property.PropertyType, baseValue);
            property.SetValue(derivedCreature, overrideValue);
            Assert.Equal(overrideValue, property.GetValue(derivedCreature));
        }
    }

    /// <summary>
    ///     Regression: no creature may be generated within
    ///     <see cref="CreatureGenerator.ConnectionExclusionRadius" /> cells (Chebyshev) of an
    ///     incoming inter-level connection's landing cell — the player arrives there, so they must
    ///     never materialize surrounded by monsters. The connecting-fragment weight-zeroing in
    ///     <see cref="CreatureGenerator" /> does NOT cover this case: the room that <em>receives</em>
    ///     an incoming connection hosts the landing cell on its regular (defining) fragment, whose
    ///     chunk has normal creature weight, so a dedicated exclusion zone is required.
    /// </summary>
    [Fact]
    public void No_creature_within_exclusion_radius_of_entrance_incoming_connection()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;

        var surfaceBranch = BranchData.Surface.Instantiate(game);
        BranchData.Entrance.Instantiate(game);
        BranchData.Dungeon.Instantiate(game);

        var surfaceLevel = LevelGenerator.CreateEmpty(surfaceBranch, depth: 1, seed: 1u, manager);
        LevelGenerator.EnsureGenerated(surfaceLevel);

        var entranceRoom = surfaceLevel.Rooms.First().Connections.Single().Value.Connection!.TargetRoomEntity.Room!;
        LevelGenerator.EnsureGenerated(entranceRoom);

        var landingCell = entranceRoom.IncomingConnections
            .Select(c => c.Connection!.TargetCell)
            .Single(c => c != null)!.Value;

        foreach (var (cell, actorEntity) in entranceRoom.Actors)
        {
            if (!actorEntity.HasComponent(EntityComponent.AI))
            {
                continue;
            }

            var distance = cell.DistanceTo(landingCell);
            Assert.True(distance > CreatureGenerator.ConnectionExclusionRadius,
                $"Creature at {cell} is {distance} cells from the incoming connection at {landingCell}; "
                + $"expected more than {CreatureGenerator.ConnectionExclusionRadius}.");
        }
    }

    private static object GetOverrideValue(Type propertyType, object? baseValue)
    {
        var valueType = Nullable.GetUnderlyingType(propertyType)!;
        if (valueType == typeof(int))
        {
            return !Equals(baseValue, 25) ? 25 : 50;
        }

        if (valueType == typeof(bool))
        {
            return !Equals(baseValue, true);
        }

        return Enum.GetValues(valueType)
            .Cast<object>()
            .First(value => !Equals(value, baseValue));
    }
}
