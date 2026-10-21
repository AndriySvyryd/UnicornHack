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
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Populated_attempts_roll_back_entities_and_counts_before_retry_or_recovery(bool throwOnFailure)
    {
        var game = TestHelper.CreateGame(seed: 17);
        var source = BranchData.RuinedStronghold;
        var layout = new RejectPopulatedAttemptLayout(source.Layout, throwOnFailure);
        var template = new Branch
        {
            Name = source.Name,
            Length = source.Length,
            IsTerminal = source.IsTerminal,
            Layout = layout,
            RoomLayout = source.RoomLayout,
            CreatureGenerator = source.CreatureGenerator,
            ItemGenerator = source.ItemGenerator
        };
        ((TestDataProvider)game.Services.Data).Branches.Add(template);
        var branch = template.Instantiate(game);
        var level = LevelGenerator.CreateEmpty(branch, 1, 17, game.Manager);

        if (throwOnFailure)
        {
            Assert.Throws<Exception>(() => LevelGenerator.EnsureGenerated(level));
            Assert.DoesNotContain(level.Rooms, room => room.Width != 0);
            Assert.Empty(level.GenerationStats.CreatureInstancesByName);
            Assert.Empty(level.GenerationStats.ItemInstancesByName);
            Assert.Empty(game.GenerationStats.CreatureInstancesByName);
            Assert.Empty(game.GenerationStats.ItemInstancesByName);
        }

        Assert.True(LevelGenerator.EnsureGenerated(level));
        Assert.True(layout.RejectedAttempt);
        Assert.NotEmpty(layout.AbandonedEntityIds);
        Assert.All(layout.AbandonedEntityIds, entityId => Assert.Null(game.Manager.FindEntity(entityId)));
        Assert.Equal(level.GenerationStats.CreatureInstancesByName.OrderBy(entry => entry.Key),
            game.GenerationStats.CreatureInstancesByName.OrderBy(entry => entry.Key));
        Assert.Equal(level.GenerationStats.ItemInstancesByName.OrderBy(entry => entry.Key),
            game.GenerationStats.ItemInstancesByName.OrderBy(entry => entry.Key));
        Assert.Equal(0, game.Manager.Queue.QueuedCount);
    }

    private sealed class RejectPopulatedAttemptLayout(LevelLayout source, bool throwOnFailure) : LevelLayout
    {
        public bool RejectedAttempt { get; private set; }

        public int[] AbandonedEntityIds { get; private set; } = [];

        public override List<Chunk>? Fill(GameLevel level, Branch branch)
        {
            if (RejectedAttempt)
            {
                Assert.Empty(level.GenerationStats.CreatureInstancesByName);
                Assert.Empty(level.GenerationStats.ItemInstancesByName);
                Assert.Empty(level.Game.GenerationStats.CreatureInstancesByName);
                Assert.Empty(level.Game.GenerationStats.ItemInstancesByName);
            }

            var chunks = source.Fill(level, branch);
            if (chunks == null || RejectedAttempt)
            {
                return chunks;
            }

            AbandonedEntityIds = level.Rooms.SelectMany(room => room.Actors.Values.Concat(room.Items.Values))
                .Select(entity => entity.Id).ToArray();
            RejectedAttempt = true;
            if (throwOnFailure)
            {
                throw new InvalidOperationException("Test failure after creature and item placement.");
            }

            return null;
        }
    }

    [Fact]
    public void Successful_generation_increments_attempt_counters_once()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;
        var dungeon = BranchData.RuinedStronghold.Instantiate(game);
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
        var dungeon = BranchData.RuinedStronghold.Instantiate(game);
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
            Name = BranchData.RuinedStronghold.Name,
            Length = BranchData.RuinedStronghold.Length,
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
            Name = BranchData.RuinedStronghold.Name,
            Length = BranchData.RuinedStronghold.Length,
            Layout = new FailNTimesLayout(failCount: 2, fallbackLayout: BranchData.RuinedStronghold.Layout)
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
