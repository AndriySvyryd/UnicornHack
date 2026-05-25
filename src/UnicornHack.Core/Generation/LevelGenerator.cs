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
            try
            {
                stats.RoomGraphAttempts++;
                if (Generate(level) is not null)
                {
                    return true;
                }

                stats.ValidationFailures++;
                ResetGeneratedState(level);
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
        }

        throw new InvalidOperationException(
            $"Failed to generate a valid graph for '{level.BranchName}:{level.Depth}' after {maxAttempts} attempts");
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
        RoomFeatures roomFeatures,
        LevelGenerationStats stats)
    {
        stats.FragmentSelectionAttempts++;

        var game = level.Game;
        var gameStats = game.GenerationStats;
        var picked = level.GenerationRandom.Pick(
            game.Services.Data.DefiningMapFragments.GetAsList(),
            f => f.GetWeight(
                level.BranchName, level.Depth, roomFeatures,
                fragmentInstancesByName: stats.FragmentInstancesByName,
                globalFragmentInstancesByName: gameStats.FragmentInstancesByName,
                tagInstances: stats.FragmentInstancesByTag,
                globalTagInstances: gameStats.FragmentInstancesByTag,
                globalFeatureInstances: gameStats.FragmentInstancesByFeature));

        stats.RecordFragmentInstance(picked.SourceName, picked.Tags, roomFeatures);
        gameStats.RecordFragmentInstance(picked.SourceName, picked.Tags, roomFeatures);
        stats.RecordFragmentVariant(picked.Name);
        return picked;
    }

    private static void ResetGeneratedState(GameLevel gameLevel)
    {
        foreach (var room in gameLevel.Rooms.ToList())
        {
            room.Entity.RemoveComponent((int)EntityComponent.Room);
        }

        gameLevel.Rooms.Clear();
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

    /// <summary>
    ///     Verifies that every room is reachable from the entrance and that an exit is reachable
    ///     from every room, following directed edges.
    /// </summary>
    public static bool ValidateRoomGraph(RoomGraph graph)
    {
        var entrances = graph.Rooms.Where(r => (r.Features & RoomFeatures.Entrance) != 0).ToList();
        var exits = graph.Rooms.Where(r => (r.Features & RoomFeatures.Exit) != 0).ToList();
        if (entrances.Count == 0 || exits.Count == 0)
        {
            return false;
        }

        var forward = graph.Connections
            .GroupBy(e => e.Source)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Target).ToList());

        var reachableFromEntrance = BreadthFirstReach(entrances, forward);
        if (reachableFromEntrance.Count != graph.Rooms.Count)
        {
            return false;
        }

        // To check "an exit is reachable from every room", reverse the edges and BFS from every exit.
        var backward = graph.Connections
            .GroupBy(e => e.Target)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Source).ToList());
        var canReachExit = BreadthFirstReach(exits, backward);
        return canReachExit.Count == graph.Rooms.Count;
    }

    private static HashSet<RoomGraphNode> BreadthFirstReach(
        IEnumerable<RoomGraphNode> sources,
        Dictionary<RoomGraphNode, List<RoomGraphNode>> adjacency)
    {
        var visited = new HashSet<RoomGraphNode>();
        var queue = new Queue<RoomGraphNode>();
        foreach (var source in sources)
        {
            if (visited.Add(source))
            {
                queue.Enqueue(source);
            }
        }

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            if (!adjacency.TryGetValue(node, out var neighbours))
            {
                continue;
            }

            foreach (var neighbour in neighbours)
            {
                if (visited.Add(neighbour))
                {
                    queue.Enqueue(neighbour);
                }
            }
        }

        return visited;
    }
}
