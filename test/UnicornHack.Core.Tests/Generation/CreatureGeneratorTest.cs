using UnicornHack.Data.Branches;

namespace UnicornHack.Generation;

/// <summary>
///     Regression: no creature may be generated within
///     <see cref="CreatureGenerator.ConnectionExclusionRadius" /> cells (Chebyshev) of an
///     incoming inter-level connection's landing cell — the player arrives there, so they must
///     never materialize surrounded by monsters. The connecting-fragment weight-zeroing in
///     <see cref="CreatureGenerator" /> does NOT cover this case: the room that <em>receives</em>
///     an incoming connection hosts the landing cell on its regular (defining) fragment, whose
///     chunk has normal creature weight, so a dedicated exclusion zone is required.
/// </summary>
public class CreatureGeneratorTest
{
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
}
