namespace UnicornHack.Generation;

/// <summary>
///     Verifies that a branch's terminal level (Depth == Length) sources an inter-branch
///     connection from its exit room when <see cref="Branch.IsTerminal"/> is <c>false</c>,
///     and skips the connection when the branch is flagged terminal. The destination
///     branch is picked at random by <see cref="Branch.GetWeight"/>; <c>GetWeight</c>
///     returns 0 for the source branch automatically so authors don't need to repeat the
///     guard in their <see cref="Branch.GenerationWeight"/> expressions.
/// </summary>
public class TerminalBranchConnectionTest
{
    [Fact]
    public void Non_terminal_branch_terminal_level_sources_inter_branch_connection_to_random_next_branch()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;

        // Two synthetic branches: 'origin' (length 1, IsTerminal=false → continues) and
        // 'next' (length 1). Each branch's GenerationWeight stays at the default "1";
        // GetWeight automatically returns 0 when the candidate matches the source.
        var nextBranch = new Branch
        {
            Name = "test-next-branch",
            Length = 1,
            Difficulty = 1,
            Layout = new EmptyLevelLayout()
        };
        var originBranch = new Branch
        {
            Name = "test-origin-branch",
            Length = 1,
            Difficulty = 1,
            Layout = new EmptyLevelLayout()
        };

        var testProvider = (TestDataProvider)game.Services.Data;
        // Restrict the Branches loader to test-only entries so the inter-branch picker
        // can only pick between the two synthetic branches; this isolates the test
        // from the set of production branches and the order their per-source weights
        // resolve to under the (deterministic) gen RNG.
        testProvider.Branches.IncludeProduction = false;
        testProvider.Branches.Add(originBranch);
        testProvider.Branches.Add(nextBranch);

        var originDefining = new DefiningMapFragment
        {
            Name = "test-origin-defining",
            GenerationWeight = $"$branch == '{originBranch.Name}' ? 1 : 0",
            NoRandomDoorways = true
        };
        testProvider.DefiningMapFragments.Add(originDefining);

        // Defensive: defining fragment for the destination branch in case its placeholder
        // ever gets generated. The test never calls EnsureGenerated on it though.
        var nextDefining = new DefiningMapFragment
        {
            Name = "test-next-defining",
            GenerationWeight = $"$branch == '{nextBranch.Name}' ? 1 : 0",
            NoRandomDoorways = true
        };
        testProvider.DefiningMapFragments.Add(nextDefining);

        var originEntity = originBranch.Instantiate(game);
        nextBranch.Instantiate(game);

        var originLevel = LevelGenerator.CreateEmpty(originEntity, depth: 1, seed: 1u, manager);
        LevelGenerator.EnsureGenerated(originLevel);

        var exitRoom = originLevel.Rooms.Single(r => (r.Features & RoomFeatures.Exit) != 0);

        var outboundConnections = exitRoom.Connections.Values
            .Select(c => c.Connection!)
            .Where(c => c.TargetRoomEntity.Room is { } target
                        && target.Level.BranchName == nextBranch.Name)
            .ToList();
        Assert.Single(outboundConnections);
    }

    [Fact]
    public void Terminal_branch_terminal_level_skips_inter_branch_connection()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;

        var terminalBranch = new Branch
        {
            Name = "test-terminal-branch",
            Length = 1,
            Difficulty = 1,
            IsTerminal = true,
            Layout = new EmptyLevelLayout()
        };
        var testProvider = (TestDataProvider)game.Services.Data;
        testProvider.Branches.Add(terminalBranch);

        var defining = new DefiningMapFragment
        {
            Name = "test-terminal-defining",
            GenerationWeight = $"$branch == '{terminalBranch.Name}' ? 1 : 0",
            NoRandomDoorways = true
        };
        testProvider.DefiningMapFragments.Add(defining);

        var entity = terminalBranch.Instantiate(game);
        var level = LevelGenerator.CreateEmpty(entity, depth: 1, seed: 1u, manager);
        LevelGenerator.EnsureGenerated(level);

        var exitRoom = level.Rooms.Single(r => (r.Features & RoomFeatures.Exit) != 0);
        // No inter-level connection \u2014 the only Connections entries (if any) should be
        // intra-level, never spanning to a different GameLevel.
        var outboundInterLevel = exitRoom.Connections.Values
            .Select(c => c.Connection!)
            .Where(c => c.TargetRoomEntity.Room is { } target && target.Level != exitRoom.Level)
            .ToList();
        Assert.Empty(outboundInterLevel);
    }
}
