using UnicornHack.Data.Branches;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation;

public class LevelGeneratorTest
{
    /// <summary>
    ///     Regression: with an initial seed of 0, every generated level had the same
    ///     layout because the xorshift RNG is degenerate at state 0 and never advances.
    ///     Even with non-zero seeds the generator must derive a fresh seed for the next
    ///     level from the current level's post-generation RNG state, otherwise depth N
    ///     and depth N+1 would share the same RNG inputs whenever no random calls
    ///     happened during generation.
    /// </summary>
    [Theory]
    [InlineData(1u)]
    [InlineData(42u)]
    public void Consecutive_levels_have_distinct_terrain(uint seed)
    {
        var game = TestHelper.CreateGame(seed);
        var manager = game.Manager;
        var dungeon = BranchData.Dungeon.Instantiate(game);
        game.Branches.Add(dungeon);

        var firstLevel = LevelGenerator.CreateEmpty(dungeon, depth: 1, seed, manager);
        LevelGenerator.EnsureGenerated(firstLevel);
        manager.Queue.ProcessQueue(manager);
        var firstTerrain = SnapshotTerrain(firstLevel);

        // Mirror LevelConnection.Create: derive the next level's seed from the current
        // level's post-generation RNG state.
        firstLevel.GenerationRandom.Next(int.MaxValue);
        var secondSeed = firstLevel.GenerationRandom.Seed;
        var secondLevel = LevelGenerator.CreateEmpty(dungeon, depth: 2, secondSeed, manager);
        LevelGenerator.EnsureGenerated(secondLevel);
        manager.Queue.ProcessQueue(manager);
        var secondTerrain = SnapshotTerrain(secondLevel);

        Assert.False(TerrainEquals(firstTerrain, secondTerrain),
            $"Depth 1 and depth 2 produced identical terrain for initial seed {seed}.");
    }

    private static byte[] SnapshotTerrain(LevelComponent level)
    {
        var snapshot = new byte[level.TileCount];
        Array.Copy(level.Terrain, snapshot, level.TileCount);
        return snapshot;
    }

    private static bool TerrainEquals(byte[] first, byte[] second)
    {
        if (first.Length != second.Length)
        {
            return false;
        }

        for (var i = 0; i < first.Length; i++)
        {
            if (first[i] != second[i])
            {
                return false;
            }
        }

        return true;
    }
}
