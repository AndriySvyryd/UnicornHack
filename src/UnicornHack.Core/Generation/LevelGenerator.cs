using UnicornHack.Systems.Levels;
using UnicornHack.Utils;

namespace UnicornHack.Generation;

public static class LevelGenerator
{
    public static readonly byte MaxDifficulty = 30;

    public static GameLevel CreateEmpty(GameBranch branch, byte depth, uint seed, GameManager manager)
    {
        if (branch.Length < depth)
        {
            throw new InvalidOperationException("Level created beyond branch length");
        }

        var difficulty = branch.Difficulty + depth;
        if (difficulty > MaxDifficulty)
        {
            throw new InvalidOperationException(
                $"Difficulty {difficulty} greater than max {MaxDifficulty}");
        }

        var level = new GameLevel
        {
            GameId = branch.GameId,
            Game = branch.Game,
            BranchName = branch.Name,
            Branch = branch,
            Depth = depth,
            Difficulty = difficulty,
            GenerationRandom = new SimpleRandom { Seed = seed }
        };
        branch.Levels.Add(level);

        return level;
    }

    public static bool EnsureGenerated(GameLevel level)
    {
        // If rooms already exist with terrain, generation has already run.
        if (level.Rooms.Any(r => r.Width != 0))
        {
            return false;
        }

        const int maxAttempts = 5;
        var stats = level.GenerationStats;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            stats.LevelAttempts++;
            var gameCheckpoint = level.Game.GenerationStats.CaptureInstanceCounts();
            var levelCheckpoint = stats.CaptureInstanceCounts();
            var committed = false;
            try
            {
                stats.RoomGraphAttempts++;
                if (Generate(level) is not null)
                {
                    // Generation enqueues entity-lifecycle and property-change messages for every
                    // room, connection, creature and item it creates. Nothing drains them until a
                    // game turn ticks, so generating several levels back-to-back (descending
                    // without acting, or a batch tool) would otherwise overflow the message queue.
                    // Process them now so each generated level leaves the queue clean and
                    // self-contained.
                    var manager = level.Game.Manager;
                    manager.Queue.ProcessQueue(manager);
                    committed = true;
                    return true;
                }

                stats.ValidationFailures++;
            }
            catch (Exception e)
            {
                // TODO: Log parameters if failed
                var msg = $@"Error while generating level '{level.BranchName}:{level.Depth}', initial seed {level.Game.Options.InitialSeed}:
";

                Console.WriteLine(msg);
                Console.WriteLine(e);
                throw new Exception(msg, e);
            }
            finally
            {
                if (!committed)
                {
                    try
                    {
                        ResetGeneratedState(level);
                        var manager = level.Game.Manager;
                        manager.Queue.ProcessQueue(manager);
                    }
                    finally
                    {
                        level.Game.GenerationStats.RestoreInstanceCounts(gameCheckpoint);
                        stats.RestoreInstanceCounts(levelCheckpoint);
                    }
                }
            }
        }

        throw new InvalidOperationException(
            $"Failed to generate a valid graph for '{level.BranchName}:{level.Depth}' after {maxAttempts} attempts. "
            + $"Unmet doorways: {stats.RoomLayoutInsufficientDoorways}; unbound features: {stats.RoomLayoutUnboundFeatures}; "
            + $"room failures: {string.Join(", ", stats.FragmentRoomFailures.Select(entry => $"{entry.Key}={entry.Value}"))}.");
    }

    /// <summary>
    ///     Backward-compatible overload: generates the <see cref="GameLevel" /> the room belongs to.
    /// </summary>
    public static bool EnsureGenerated(RoomComponent room) => EnsureGenerated(room.Level);

    /// <summary>
    ///     Picks a <see cref="DefiningMapFragment" /> for a room based on its
    ///     <see cref="RoomFeatures" />.
    /// </summary>
    public static DefiningMapFragment PickFragmentForRoom(
        GameLevel level,
        RoomFeatures roomFeatures)
    {
        var stats = level.GenerationStats;
        stats.FragmentSelectionAttempts++;

        var picked = level.GenerationRandom.WeightedOrder(level.Game.Services.Data.DefiningMapFragments.GetAsList(),
                fragment => fragment.GetWeight(level, roomFeatures)).FirstOrDefault()
            ?? throw new InvalidOperationException(
                $"No eligible defining fragment for '{level.BranchName}:{level.Depth}' with room features '{roomFeatures}'.");

        picked.IncrementInstanceCounts(level, roomFeatures);
        return picked;
    }

    /// <summary>
    ///     Removes all generated rooms from <paramref name="gameLevel" /> so a fresh
    ///     attempt can run from a clean slate. Placeholder rooms created by a
    ///     neighboring level's <see cref="Map.LevelConnection.CreateSourceConnection" />
    ///     to host a cross-level <see cref="ConnectionComponent" />'s target are
    ///     reset in place via <see cref="RoomComponent.Reset" />:
    ///     destroying their Room component would fire the
    ///     <c>IncomingConnectionsToRoomRelationship</c> cleanup which detaches the
    ///     neighbor's outbound connection (its <see cref="PositionComponent" />)
    ///     from its source room, breaking the cross-level link permanently. A
    ///     placeholder is identified by an incoming connection from another level;
    ///     attempt 1 may have
    ///     allocated terrain on it (so the <c>Width == 0</c> half of
    ///     <see cref="LevelLayout.GetEmptyEntranceRoom" />'s discriminator no
    ///     longer holds), and <c>ResetToPlaceholder</c> re-establishes the zero-width
    ///     state in place.
    /// </summary>
    private static void ResetGeneratedState(GameLevel gameLevel)
    {
        RollbackCreatureInstances(gameLevel);

        foreach (var room in gameLevel.Rooms.ToList())
        {
            foreach (var feature in room.Features.Values.ToArray())
            {
                feature.Position = null;
            }

            if (room.IncomingConnections.Any(connection =>
                connection.Position?.RoomEntity.Room is { } sourceRoom && sourceRoom.Level != gameLevel))
            {
                room.Reset();
                continue;
            }

            // Remove the Room component first. Removing from the collection first would
            // make EF's NavigationFixer sever the required RoomComponent -> GameLevel
            // relationship by nulling the non-nullable BranchName foreign key, which throws.
            room.Entity.RemoveComponent((int)EntityComponent.Room);
            gameLevel.Rooms.Remove(room);
        }
    }

    private static void RollbackCreatureInstances(GameLevel gameLevel)
    {
        gameLevel.Game.GenerationStats.RemoveCreatureInstances(gameLevel.GenerationStats);
        gameLevel.GenerationStats.ClearCreatureInstances();
    }

    /// <summary>
    ///     The level-scope <see cref="LevelLayout" /> lives on <see cref="Branch.Layout" />.
    ///     The branch definition is resolved by name from the data provider; tests that
    ///     need a synthetic branch must register it in their <c>TestDataProvider.Branches</c>
    ///     loader before calling <see cref="Generate" />.
    /// </summary>
    public static List<Chunk>? Generate(GameLevel level)
    {
        // Each LevelLayout owns its own creature / item generation pipeline; single-room
        // layouts call the generators on their lone room, multi-room layouts call them
        // per-room from MaterializeGraph. Both paths resolve effective per-room values via
        // DefiningMapFragment.GetEffective* helpers, falling back to the branch defaults.
        var branch = level.Game.Services.Data.Branches.Get(level.BranchName);
        return branch.Layout.Fill(level, branch);
    }
}
