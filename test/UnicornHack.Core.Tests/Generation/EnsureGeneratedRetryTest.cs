using UnicornHack.Data.Branches;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation;

/// <summary>
///     Verifies the null-return error model in <see cref="LevelGenerator.EnsureGenerated" />:
///     when the layout returns null the generator should retry up to 5 times, increment
///     <see cref="GenerationStats" /> counters appropriately, then throw
///     <see cref="InvalidOperationException" /> if all attempts fail.
/// </summary>
public class EnsureGeneratedRetryTest
{
    [Fact]
    public void Successful_generation_increments_attempt_counters_once()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;
        var dungeon = TestHelper.InstantiateBranch(BranchData.Dungeon, game);
        var level = LevelGenerator.CreateEmpty(dungeon, depth: 1, seed: 1u, manager);

        var generated = LevelGenerator.EnsureGenerated(level);

        Assert.True(generated);
        Assert.True(level.GenerationStats.LevelAttempts >= 1);
        Assert.True(level.GenerationStats.LevelAttempts <= 5);
        Assert.Equal(level.GenerationStats.LevelAttempts, level.GenerationStats.RoomGraphAttempts);
        Assert.Equal(level.GenerationStats.LevelAttempts - 1, level.GenerationStats.ValidationFailures);
    }

    [Fact]
    public void EnsureGenerated_short_circuits_when_already_generated()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;
        var dungeon = TestHelper.InstantiateBranch(BranchData.Dungeon, game);
        var level = LevelGenerator.CreateEmpty(dungeon, depth: 1, seed: 1u, manager);
        Assert.True(LevelGenerator.EnsureGenerated(level));

        // Second call must be a no-op and return false.
        var attemptsBefore = level.GenerationStats.LevelAttempts;
        var second = LevelGenerator.EnsureGenerated(level);

        Assert.False(second);
        Assert.Equal(attemptsBefore, level.GenerationStats.LevelAttempts);
    }

    [Fact]
    public void EnsureGenerated_retries_on_null_return_then_throws_after_max_attempts()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;
        var branch = new Branch
        {
            Name = BranchData.Dungeon.Name,
            Length = BranchData.Dungeon.Length,
            Layout = new AlwaysFailLayout()
        };
        ((TestDataProvider)game.Services.Data).Branches.Add(branch);
        var dungeon = branch.Instantiate(game);
        var level = LevelGenerator.CreateEmpty(dungeon, depth: 1, seed: 1u, manager);

        var failingLayout = (AlwaysFailLayout)branch.Layout;

        var ex = Assert.Throws<InvalidOperationException>(() => LevelGenerator.EnsureGenerated(level));
        Assert.Contains("Failed to generate", ex.Message);
        Assert.Equal(5, failingLayout.FillCallCount);
        Assert.Equal(5, level.GenerationStats.LevelAttempts);
        Assert.Equal(5, level.GenerationStats.ValidationFailures);
    }

    [Fact]
    public void EnsureGenerated_succeeds_after_retries_when_layout_eventually_succeeds()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;
        var branch = new Branch
        {
            Name = BranchData.Dungeon.Name,
            Length = BranchData.Dungeon.Length,
            Layout = new FailNTimesLayout(failCount: 2, fallbackLayout: BranchData.Dungeon.Layout)
        };
        ((TestDataProvider)game.Services.Data).Branches.Add(branch);
        var dungeon = branch.Instantiate(game);
        var level = LevelGenerator.CreateEmpty(dungeon, depth: 1, seed: 1u, manager);

        var flakyLayout = (FailNTimesLayout)branch.Layout;

        var generated = LevelGenerator.EnsureGenerated(level);

        Assert.True(generated);
        Assert.True(flakyLayout.FillCallCount >= 3);
        Assert.True(level.GenerationStats.LevelAttempts >= 3);
        Assert.True(level.GenerationStats.ValidationFailures >= 2);
    }

    private sealed class AlwaysFailLayout : LevelLayout
    {
        public int FillCallCount { get; private set; }

        public override List<Chunk>? Fill(GameLevel gameLevel, Branch branch)
        {
            FillCallCount++;
            return null;
        }
    }

    private sealed class FailNTimesLayout : LevelLayout
    {
        private readonly int _failCount;
        private readonly LevelLayout _fallbackLayout;

        public FailNTimesLayout(int failCount, LevelLayout fallbackLayout)
        {
            _failCount = failCount;
            _fallbackLayout = fallbackLayout;
        }

        public int FillCallCount { get; private set; }

        public override List<Chunk>? Fill(GameLevel gameLevel, Branch branch)
        {
            FillCallCount++;
            if (FillCallCount <= _failCount)
            {
                return null;
            }

            return _fallbackLayout.Fill(gameLevel, branch);
        }
    }
}
