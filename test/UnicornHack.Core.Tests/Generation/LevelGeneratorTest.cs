using UnicornHack.Data.Branches;
using UnicornHack.Data.Fragments;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation;

public class LevelGeneratorTest
{
    private static readonly IReadOnlyDictionary<string, int> EmptyTagInstances = new Dictionary<string, int>();


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
        var dungeon = TestHelper.InstantiateBranch(BranchData.Dungeon, game);

        var firstLevel = LevelGenerator.CreateEmpty(dungeon, depth: 1, seed, manager);
        LevelGenerator.EnsureGenerated(firstLevel);
        manager.Queue.ProcessQueue(manager);
        var firstTerrain = SnapshotTerrain(firstLevel.Rooms.First());

        // Mirror LevelConnection.Create: derive the next level's seed from the current
        // level's post-generation RNG state.
        firstLevel.GenerationRandom.Next(int.MaxValue);
        var secondSeed = firstLevel.GenerationRandom.Seed;
        var secondLevel = LevelGenerator.CreateEmpty(dungeon, depth: 2, secondSeed, manager);
        LevelGenerator.EnsureGenerated(secondLevel);
        manager.Queue.ProcessQueue(manager);
        var secondTerrain = SnapshotTerrain(secondLevel.Rooms.First());

        Assert.False(TerrainEquals(firstTerrain, secondTerrain),
            $"Depth 1 and depth 2 produced identical terrain for initial seed {seed}.");
    }

    private static byte[] SnapshotTerrain(RoomComponent level)
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

    /// <summary>
    ///     Per-room fragment selection: the picker honours
    ///     <see cref="DefiningMapFragment.GetWeight" /> with the room's <see cref="RoomFeatures" />
    ///     and bumps <see cref="GenerationStats.FragmentSelectionAttempts" />.
    /// </summary>
    [Fact]
    public void Picker_increments_FragmentSelectionAttempts()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var dungeon = TestHelper.InstantiateBranch(BranchData.Dungeon, game);
        var level = LevelGenerator.CreateEmpty(dungeon, depth: 1, seed: 1u, game.Manager);

        var stats = level.GenerationStats;
        var before = stats.FragmentSelectionAttempts;

        LevelGenerator.PickFragmentForRoom(level, RoomFeatures.Normal, stats);

        Assert.Equal(before + 1, stats.FragmentSelectionAttempts);
    }

    [Fact]
    public void Picker_returns_a_fragment_with_positive_weight_for_dungeon_branch()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var dungeon = TestHelper.InstantiateBranch(BranchData.Dungeon, game);
        var level = LevelGenerator.CreateEmpty(dungeon, depth: 1, seed: 1u, game.Manager);

        var picked = LevelGenerator.PickFragmentForRoom(level, RoomFeatures.Normal, level.GenerationStats);

        var weight = picked.GetWeight(level.BranchName, level.Depth, RoomFeatures.Normal, 0, EmptyTagInstances);
        Assert.True(weight > 0,
            $"Picked fragment '{picked.Name}' has non-positive weight {weight} for dungeon branch.");
        Assert.Same(DefiningMapFragmentData.DungeonRoom, picked);
    }

    [Fact]
    public void Picker_returns_registered_fragment_for_branch_not_in_default_data()
    {
        // For a synthetic branch with no matching fragment in the default data set, the test
        // registers its own fragment with weight 1 for that branch. The picker must find it.
        var game = TestHelper.CreateGame(seed: 1u);
        var fakeBranch = new Branch
        {
            Name = "synthetic-pick-branch",
            Length = 1,
            Difficulty = 1,
            Layout = new EmptyLevelLayout()
        };
        var branchEntity = fakeBranch.Instantiate(game);
        game.Branches.Add(branchEntity);
        var level = LevelGenerator.CreateEmpty(branchEntity, depth: 1, seed: 1u, game.Manager);

        var injectedFragment = new DefiningMapFragment
        {
            Name = "synthetic-fragment",
            GenerationWeight = $"$branch == '{fakeBranch.Name}' ? 1 : 0"
        };
        var testProvider = (TestDataProvider)game.Services.Data;
        testProvider.DefiningMapFragments.Add(injectedFragment);

        var picked = LevelGenerator.PickFragmentForRoom(level, RoomFeatures.Normal, level.GenerationStats);

        Assert.Same(injectedFragment, picked);
    }

    [Fact]
    public void Picker_records_instance_under_SourceName_not_variant_Name()
    {
        // Variants of a fragment all share SourceName so per-name statistics aggregate
        // across the whole D4 orbit. We exercise this by injecting a base fragment plus a
        // synthetic 'variant' whose SourceName is the source's name; either pick must
        // increment the source's counter.
        var game = TestHelper.CreateGame(seed: 1u);
        var fakeBranch = new Branch
        {
            Name = "synthetic-instance-branch",
            Length = 1,
            Difficulty = 1,
            Layout = new EmptyLevelLayout()
        };
        var branchEntity = fakeBranch.Instantiate(game);
        game.Branches.Add(branchEntity);
        var level = LevelGenerator.CreateEmpty(branchEntity, depth: 1, seed: 1u, game.Manager);

        var sourceFragment = new VariantTrackingFragment
        {
            Name = "synthetic-source",
            GenerationWeight = $"$branch == '{fakeBranch.Name}' ? 1 : 0"
        };
        sourceFragment.AssignSourceName("synthetic-source");
        var variantFragment = new VariantTrackingFragment
        {
            Name = "synthetic-source_Rotate90",
            GenerationWeight = $"$branch == '{fakeBranch.Name}' ? 1 : 0"
        };
        variantFragment.AssignSourceName("synthetic-source");

        var testProvider = (TestDataProvider)game.Services.Data;
        testProvider.DefiningMapFragments.Add(sourceFragment);
        testProvider.DefiningMapFragments.Add(variantFragment);

        LevelGenerator.PickFragmentForRoom(level, RoomFeatures.Normal, level.GenerationStats);
        LevelGenerator.PickFragmentForRoom(level, RoomFeatures.Normal, level.GenerationStats);

        // Either pick aggregates onto the shared SourceName.
        var aggregated = level.GenerationStats.FragmentInstancesByName.GetValueOrDefault("synthetic-source");
        Assert.Equal(2, aggregated);
    }

    [Fact]
    public void Picker_records_instance_into_game_wide_stats()
    {
        // Per-level and game-wide GenerationStats must both observe each pick so weight
        // expressions referencing $instances or $globalInstances stay in sync after a
        // single PickFragmentForRoom call.
        var game = TestHelper.CreateGame(seed: 1u);
        var fakeBranch = new Branch
        {
            Name = "synthetic-game-stats-branch",
            Length = 1,
            Difficulty = 1,
            Layout = new EmptyLevelLayout()
        };
        var branchEntity = fakeBranch.Instantiate(game);
        game.Branches.Add(branchEntity);
        var level = LevelGenerator.CreateEmpty(branchEntity, depth: 1, seed: 1u, game.Manager);

        var fragment = new DefiningMapFragment
        {
            Name = "global-count-fragment",
            GenerationWeight = $"$branch == '{fakeBranch.Name}' ? 1 : 0",
            Tags = ["scenic"]
        };
        var testProvider = (TestDataProvider)game.Services.Data;
        testProvider.DefiningMapFragments.Add(fragment);

        LevelGenerator.PickFragmentForRoom(level, RoomFeatures.Normal, level.GenerationStats);
        LevelGenerator.PickFragmentForRoom(level, RoomFeatures.Normal, level.GenerationStats);

        Assert.Equal(2, game.GenerationStats.FragmentInstancesByName.GetValueOrDefault("global-count-fragment"));
        Assert.Equal(2, game.GenerationStats.FragmentInstancesByTag.GetValueOrDefault("scenic"));
        Assert.Equal(2, level.GenerationStats.FragmentInstancesByName.GetValueOrDefault("global-count-fragment"));
    }

    private sealed class VariantTrackingFragment : DefiningMapFragment
    {
        public void AssignSourceName(string name)
            => SourceName = name;
    }
}
