using UnicornHack.Data.Branches;
using UnicornHack.Data.Fragments;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation;

/// <summary>
///     Verifies per-room fragment selection: the picker honours
///     <see cref="DefiningMapFragment.GetWeight" /> with the room's <see cref="RoomFeatures" />
///     and bumps <see cref="GenerationStats.FragmentSelectionAttempts" />.
/// </summary>
public class PickFragmentForRoomTest
{
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

        var weight = picked.GetWeight(level.BranchName, level.Depth, RoomFeatures.Normal, 0, 0);
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
            Name = "synthetic-pick-branch-" + Guid.NewGuid().ToString("N"),
            Length = 1,
            Difficulty = 1,
            Layout = new EmptyLevelLayout()
        };
        var branchEntity = fakeBranch.Instantiate(game);
        game.Branches.Add(branchEntity);
        var level = LevelGenerator.CreateEmpty(branchEntity, depth: 1, seed: 1u, game.Manager);

        var injectedFragment = new DefiningMapFragment
        {
            Name = "synthetic-fragment-" + Guid.NewGuid().ToString("N"),
            GenerationWeight = $"$branch == '{fakeBranch.Name}' ? 1 : 0"
        };
        var testProvider = (TestDataProvider)game.Services.Data;
        testProvider.DefiningMapFragments.Add(injectedFragment);

        var picked = LevelGenerator.PickFragmentForRoom(level, RoomFeatures.Normal, level.GenerationStats);

        Assert.Same(injectedFragment, picked);
    }
}
