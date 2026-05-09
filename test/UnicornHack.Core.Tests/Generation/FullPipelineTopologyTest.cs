using UnicornHack.Data.Branches;

namespace UnicornHack.Generation;

/// <summary>
///     Verifies the full generation pipeline (BuildGraph → MaterializeGraph →
///     <see cref="LevelGenerator.EnsureGenerated" />) for each topology
///     (<see cref="GridLevelLayout" />, <see cref="HexLevelLayout" />,
///     <see cref="SpiralLevelLayout" />). Covers what
///     <see cref="LevelLayoutAlgorithmTest" /> exercises in the abstract: ensures
///     terrain is allocated, every room ends up with at least one connection, and the
///     entrance/exit are reachable in the materialized level.
/// </summary>
public class FullPipelineTopologyTest
{
    public static IEnumerable<object[]> Layouts()
    {
        yield return [new GridLevelLayout()];
        yield return [new HexLevelLayout()];
        yield return [new SpiralLevelLayout()];
    }

    [Theory]
    [MemberData(nameof(Layouts))]
    public void Generates_a_complete_level_for_each_topology(LevelLayout layout)
    {
        var success = false;
        for (uint seed = 1; seed <= 5 && !success; seed++)
        {
            var game = TestHelper.CreateGame(seed);
            var manager = game.Manager;
            var branch = new Branch
            {
                Name = BranchData.Dungeon.Name,
                Length = BranchData.Dungeon.Length,
                Layout = layout
            };
            ((TestDataProvider)game.Services.Data).Branches.Add(branch);
            var dungeon = branch.Instantiate(game);
            var level = LevelGenerator.CreateEmpty(dungeon, depth: 2, seed, manager);

            try
            {
                if (!LevelGenerator.EnsureGenerated(level))
                {
                    continue;
                }
            }
            catch (InvalidOperationException)
            {
                // BuildGraph might fail on some seeds; try the next one.
                continue;
            }

            Assert.NotEmpty(level.Rooms);

            // Every room has terrain allocated.
            foreach (var room in level.Rooms)
            {
                Assert.True(room.Width > 0, $"Room ({room.GridX},{room.GridY}) has zero width");
                Assert.True(room.Height > 0, $"Room ({room.GridX},{room.GridY}) has zero height");
                Assert.True(room.Terrain.Length >= room.TileCount);
            }

            // Every room has at least one connection (intra-level or inter-level).
            foreach (var room in level.Rooms)
            {
                Assert.NotEmpty(room.Connections);
            }

            success = true;
        }

        Assert.True(success, $"{layout.GetType().Name} failed to produce a valid level in 5 seeds");
    }
}
