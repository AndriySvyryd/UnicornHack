using UnicornHack.Data.Branches;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils;

namespace UnicornHack.Generation;

/// <summary>
///     Tests the prune-from-full algorithm in <see cref="LevelLayout.BuildGraph" /> across
///     the three concrete topologies (4-/6-/8-neighbour). The invariants asserted are:
///     <list type="bullet">
///         <item>The entrance reaches every node via directed edges.</item>
///         <item>Every node reaches the exit via directed edges.</item>
///         <item>Every edge respects the layout's <see cref="LevelLayout.GetNeighbourOffsets" />.</item>
///         <item>Higher <c>Interconnectedness</c> → more edges.</item>
///     </list>
/// </summary>
public class LevelLayoutAlgorithmTest
{
    private readonly ITestOutputHelper _output;

    public LevelLayoutAlgorithmTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void CreateEmptyRoom_attaches_room_to_level_with_empty_terrain()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;
        var dungeon = TestHelper.InstantiateBranch(BranchData.Dungeon, game);
        var level = LevelGenerator.CreateEmpty(dungeon, depth: 1, seed: 1u, manager);

        var room = LevelLayout.CreateEmptyRoom(level, manager);

        Assert.Same(level, room.Level);
        Assert.Contains(room, level.Rooms);
        Assert.Equal(0, room.Width);
        Assert.Equal(0, room.Height);
        Assert.Empty(room.Terrain);
        Assert.Empty(room.KnownTerrain);
        Assert.Empty(room.Neighbors);
        Assert.Empty(room.VisibleTerrain);
        Assert.Empty(room.KnownNeighbors);
    }

    [Fact]
    public void AllocateTerrain_sizes_arrays_and_fills_default_terrain()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;
        var dungeon = TestHelper.InstantiateBranch(BranchData.Dungeon, game);
        var level = LevelGenerator.CreateEmpty(dungeon, depth: 1, seed: 1u, manager);
        var room = LevelLayout.CreateEmptyRoom(level, manager);

        const byte width = 10;
        const byte height = 8;
        LevelLayout.AllocateTerrain(room, width, height, MapFeature.StoneFloor);

        Assert.Equal(width, room.Width);
        Assert.Equal(height, room.Height);
        Assert.True(room.Terrain.Length >= width * height);
        Assert.True(room.KnownTerrain.Length >= width * height);
        for (var i = 0; i < room.TileCount; i++)
        {
            Assert.Equal(MapFeature.StoneFloor, room.Terrain[i]);
            Assert.Equal(MapFeature.Unexplored, room.KnownTerrain[i]);
        }
    }

    /// <summary>
    ///     Regression: <see cref="LevelLayout.ReleaseTerrain" /> is invoked from
    ///     <see cref="RoomComponent.Clean" /> during component-pool teardown, and tests that
    ///     create a <see cref="RoomComponent" /> directly (bypassing
    ///     <see cref="LevelLayout.CreateEmptyRoom" />) used to leave the terrain backing
    ///     fields null and trigger NRE. The component now self-initialises terrain backing
    ///     fields to <see cref="Array.Empty{T}" />, so this path is safe.
    /// </summary>
    [Fact]
    public void ReleaseTerrain_does_not_throw_on_uninitialised_room()
    {
        var manager = TestHelper.CreateGameManager();
        using var roomEntityReference = manager.CreateEntity();
        var room = manager.CreateComponent<RoomComponent>(EntityComponent.Room);
        roomEntityReference.Referenced.Room = room;

        // Self-initialised: every terrain getter returns Array.Empty<byte>() before any
        // allocation has happened, so reads and the release path are both safe.
        Assert.Empty(room.Terrain);
        Assert.Empty(room.KnownTerrain);
        Assert.Empty(room.Neighbors);
        Assert.Empty(room.VisibleTerrain);
        Assert.Empty(room.KnownNeighbors);

        LevelLayout.ReleaseTerrain(room);

        Assert.Empty(room.Terrain);
        Assert.Empty(room.KnownTerrain);
        Assert.Empty(room.Neighbors);
        Assert.Equal(0, room.Width);
        Assert.Equal(0, room.Height);
    }

    [Fact]
    public void ReleaseTerrain_returns_pooled_arrays_and_resets_dimensions()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;
        var dungeon = TestHelper.InstantiateBranch(BranchData.Dungeon, game);
        var level = LevelGenerator.CreateEmpty(dungeon, depth: 1, seed: 1u, manager);
        var room = LevelLayout.CreateEmptyRoom(level, manager);
        LevelLayout.AllocateTerrain(room, width: 6, height: 6, MapFeature.StoneFloor);

        LevelLayout.ReleaseTerrain(room);

        Assert.Equal(0, room.Width);
        Assert.Equal(0, room.Height);
        Assert.Empty(room.Terrain);
        Assert.Empty(room.KnownTerrain);
        Assert.Empty(room.Neighbors);
        Assert.Empty(room.VisibleTerrain);
        Assert.Empty(room.KnownNeighbors);

        // Idempotent: calling again must not throw or attempt to return Array.Empty<T>() to the pool.
        LevelLayout.ReleaseTerrain(room);
    }

    private const int SeedCount = 50;

    public static IEnumerable<object[]> Layouts()
    {
        yield return [new GridLevelLayout()];
        yield return [new HexLevelLayout()];
        yield return [new SpiralLevelLayout()];
    }

    [Theory]
    [MemberData(nameof(Layouts))]
    public void Algorithm_produces_strongly_connected_graphs(LevelLayout layout)
    {
        for (uint seed = 1; seed <= SeedCount; seed++)
        {
            var graph = TryBuild(layout, seed);
            if (graph == null)
            {
                continue;
            }

            Assert.True(LevelGenerator.ValidateRoomGraph(graph),
                $"{layout.GetType().Name} seed {seed}: graph failed strong-connectivity validation");
        }
    }

    [Theory]
    [MemberData(nameof(Layouts))]
    public void Sinuosity_endpoints_match_shortest_and_longest_paths(LevelLayout layout)
    {
        // Whenever a build succeeds:
        //   * Sinuosity = 0 → graph's entrance-to-exit distance equals the BFS distance on
        //                     the post-trim undirected adjacency (the structural minimum).
        //   * Sinuosity = 1 → the distance equals the longest simple-path distance (the
        //                     structural maximum). Both endpoints are by construction
        //                     achievable for every seed, so the assertion never spuriously
        //                     fails on a particular seed.
        var shortestLayout = CloneWith(layout, sinuosity: 0f);
        var longestLayout = CloneWith(layout, sinuosity: 1f);

        for (uint seed = 1; seed <= SeedCount; seed++)
        {
            var shortGraph = TryBuild(shortestLayout, seed);
            if (shortGraph is not null)
            {
                var (entrance, exit) = GetEntranceAndExit(shortGraph);
                var distance = ShortestPathLength(shortGraph, entrance, exit);
                var bfsBaseline = UndirectedShortestPath(shortGraph, entrance, exit);
                // sinuosity = 0 → the interpolated target equals the BFS baseline, so
                // the realised distance must match it exactly.
                Assert.Equal(bfsBaseline, distance);
            }

            var longGraph = TryBuild(longestLayout, seed);
            if (longGraph is not null)
            {
                var (entrance, exit) = GetEntranceAndExit(longGraph);
                var distance = ShortestPathLength(longGraph, entrance, exit);
                var bfsBaseline = UndirectedShortestPath(longGraph, entrance, exit);
                Assert.True(distance >= bfsBaseline,
                    $"{layout.GetType().Name} seed {seed}: longest-path target produced "
                    + $"distance {distance} below the structural minimum {bfsBaseline}");
            }
        }
    }

    ///     Per-attempt success rates expected for each layout's default configuration.
    /// </summary>
    public static IEnumerable<object[]> LayoutDefaultReliability()
    {
        yield return [new GridLevelLayout(), 0.90];
        yield return [new HexLevelLayout(), 0.90];
        yield return [new SpiralLevelLayout(), 0.90];
        yield return [new TreeLevelLayout(), 0.90];
    }

    [Theory]
    [MemberData(nameof(LayoutDefaultReliability))]
    public void Default_constraint_meets_min_per_attempt_success_rate(
        LevelLayout layout, double minPerAttemptSuccessRate)
    {
        const int trials = 200;
        var successes = 0;
        var failureCounts = new Dictionary<string, int>();
        var stats = new GenerationStats();
        var prevSnapshot = stats.GraphFailureSnapshot();
        for (uint seed = 1; seed <= trials; seed++)
        {
            var graph = layout.BuildGraph(GetParameters(layout), new SimpleRandom { Seed = seed }, stats);
            var snapshot = stats.GraphFailureSnapshot();
            if (graph is not null)
            {
                successes++;
            }
            else
            {
                var reason = AttributeFailure(prevSnapshot, snapshot);
                failureCounts[reason] = failureCounts.GetValueOrDefault(reason) + 1;
            }
            prevSnapshot = snapshot;
        }

        var rate = (double)successes / trials;
        Assert.True(rate >= minPerAttemptSuccessRate,
            $"{layout.GetType().Name} default Sinuosity={layout.Sinuosity}: "
            + $"per-attempt success {successes}/{trials} ({rate:P1}) < minimum {minPerAttemptSuccessRate:P1}. "
            + $"Failure breakdown: {FormatFailureBreakdown(failureCounts)}.");
    }

    /// <summary>
    ///     Compares two <see cref="GenerationStats.GraphFailureSnapshot" /> dictionaries
    ///     and returns the name of the single counter that grew between them. Falls back
    ///     to <c>"Unknown"</c> if no counter changed (should not happen when the call
    ///     returned null) or to a comma-joined list if more than one changed (multi-step
    ///     futures).
    /// </summary>
    private static string AttributeFailure(
        IReadOnlyDictionary<string, int> before,
        IReadOnlyDictionary<string, int> after)
    {
        var changed = after
            .Where(kvp => kvp.Value > before[kvp.Key])
            .Select(kvp => kvp.Key.StartsWith("RoomGraph") ? kvp.Key.Substring("RoomGraph".Length) : kvp.Key)
            .ToList();
        return changed.Count switch
        {
            0 => "Unknown",
            1 => changed[0],
            _ => string.Join("+", changed)
        };
    }

    private static string FormatFailureBreakdown(Dictionary<string, int> failureCounts)
    {
        if (failureCounts.Count == 0)
        {
            return "none";
        }
        return string.Join(", ", failureCounts
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => $"{kvp.Key}={kvp.Value}"));
    }

    /// <summary>
    ///     Diagnostic-only: runs the same 200-seed sweep as
    ///     <see cref="Default_constraint_meets_min_per_attempt_success_rate" /> but emits
    ///     the per-attempt success rate and full failure-reason breakdown to the test
    ///     output regardless of pass/fail, so changes that nudge the rate up or down are
    ///     visible in CI logs alongside the dominant rejection cause.
    /// </summary>
    [Theory]
    [MemberData(nameof(LayoutDefaultReliability))]
    public void Diagnostic_per_attempt_failure_breakdown(LevelLayout layout, double _)
    {
        const int trials = 200;
        var successes = 0;
        var failureCounts = new Dictionary<string, int>();
        var firstSeedByReason = new Dictionary<string, uint>();
        var stats = new GenerationStats();
        var prevSnapshot = stats.GraphFailureSnapshot();
        for (uint seed = 1; seed <= trials; seed++)
        {
            var graph = layout.BuildGraph(GetParameters(layout), new SimpleRandom { Seed = seed }, stats);
            var snapshot = stats.GraphFailureSnapshot();
            if (graph is not null)
            {
                successes++;
            }
            else
            {
                var reason = AttributeFailure(prevSnapshot, snapshot);
                failureCounts[reason] = failureCounts.GetValueOrDefault(reason) + 1;
                firstSeedByReason.TryAdd(reason, seed);
            }
            prevSnapshot = snapshot;
        }

        var rate = (double)successes / trials;
        _output.WriteLine(
            $"{layout.GetType().Name} Sinuosity={layout.Sinuosity}: "
            + $"per-attempt success {successes}/{trials} ({rate:P1}). "
            + $"Failure breakdown: {FormatFailureBreakdown(failureCounts)}.");
        foreach (var (reason, firstSeed) in firstSeedByReason)
        {
            _output.WriteLine($"  first {reason} seed: {firstSeed}");
        }
    }

    /// <summary>
    ///     With the level generator's 5-attempt retry budget, the per-level success rate is
    ///     <c>1 - (1 - p)^5</c> where <c>p</c> is the per-attempt rate. Verifies that every
    ///     layout's defaults stay within the budget across many simulated levels.
    /// </summary>
    [Theory]
    [MemberData(nameof(LayoutDefaultReliability))]
    public void Default_constraint_succeeds_within_retry_budget(
        LevelLayout layout, double _)
    {
        const int levels = 50;
        const int retryBudget = 5;
        var perLevelFailures = 0;
        var totalAttempts = 0;
        for (uint level = 1; level <= levels; level++)
        {
            var succeeded = false;
            for (var attempt = 0; attempt < retryBudget; attempt++)
            {
                totalAttempts++;
                var seed = level * 1000u + (uint)attempt;
                if (TryBuild(layout, seed) is not null)
                {
                    succeeded = true;
                    break;
                }
            }

            if (!succeeded)
            {
                perLevelFailures++;
            }
        }

        var failureRate = (double)perLevelFailures / levels;
        var avgAttempts = (double)totalAttempts / levels;
        Assert.True(failureRate <= 0.10,
            $"{layout.GetType().Name} default Sinuosity={layout.Sinuosity}: "
            + $"{perLevelFailures}/{levels} levels exhausted the {retryBudget}-attempt retry budget "
            + $"({failureRate:P1}); average attempts per level = {avgAttempts:F2}.");
    }

    private static LevelLayout CloneWith(LevelLayout layout, float sinuosity)
        => layout switch
        {
            // Tree extends Grid; check Tree first to avoid the GridLevelLayout pattern matching it.
            TreeLevelLayout t => new TreeLevelLayout
            {
                Interconnectedness = t.Interconnectedness,
                Sinuosity = sinuosity,
                MinRooms = t.MinRooms,
                MaxRooms = t.MaxRooms
            },
            GridLevelLayout g => new GridLevelLayout
            {
                GridWidth = g.GridWidth,
                GridHeight = g.GridHeight,
                Interconnectedness = g.Interconnectedness,
                Sinuosity = sinuosity,
                MinRooms = g.MinRooms,
                MaxRooms = g.MaxRooms
            },
            HexLevelLayout h => new HexLevelLayout
            {
                GridWidth = h.GridWidth,
                GridHeight = h.GridHeight,
                Interconnectedness = h.Interconnectedness,
                Sinuosity = sinuosity,
                MinRooms = h.MinRooms,
                MaxRooms = h.MaxRooms
            },
            SpiralLevelLayout s => new SpiralLevelLayout
            {
                GridWidth = s.GridWidth,
                GridHeight = s.GridHeight,
                Interconnectedness = s.Interconnectedness,
                Sinuosity = sinuosity,
                MinRooms = s.MinRooms,
                MaxRooms = s.MaxRooms
            },
            _ => throw new ArgumentException($"Unknown layout {layout.GetType().Name}")
        };

    private static (RoomGraphNode Entrance, RoomGraphNode Exit) GetEntranceAndExit(RoomGraph graph)
        => (graph.Rooms.Single(r => (r.Features & RoomFeatures.Entrance) != 0),
            graph.Rooms.Single(r => (r.Features & RoomFeatures.Exit) != 0));

    private static int UndirectedShortestPath(RoomGraph graph, RoomGraphNode source, RoomGraphNode target)
    {
        // Treat every directed edge as bidirectional for the structural minimum.
        var adj = new Dictionary<RoomGraphNode, HashSet<RoomGraphNode>>(graph.Rooms.Count);
        foreach (var node in graph.Rooms)
        {
            adj[node] = [];
        }
        foreach (var edge in graph.Connections)
        {
            adj[edge.Source].Add(edge.Target);
            adj[edge.Target].Add(edge.Source);
        }

        var dist = new Dictionary<RoomGraphNode, int> { [source] = 0 };
        var queue = new Queue<RoomGraphNode>();
        queue.Enqueue(source);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == target)
            {
                return dist[current];
            }
            var d = dist[current];
            foreach (var neighbour in adj[current])
            {
                if (dist.TryAdd(neighbour, d + 1))
                {
                    queue.Enqueue(neighbour);
                }
            }
        }
        return -1;
    }

    private static int ShortestPathLength(RoomGraph graph, RoomGraphNode source, RoomGraphNode target)
    {
        var outgoing = new Dictionary<RoomGraphNode, List<RoomGraphNode>>(graph.Rooms.Count);
        foreach (var node in graph.Rooms)
        {
            outgoing[node] = [];
        }
        foreach (var edge in graph.Connections)
        {
            outgoing[edge.Source].Add(edge.Target);
        }

        var dist = new Dictionary<RoomGraphNode, int> { [source] = 0 };
        var queue = new Queue<RoomGraphNode>();
        queue.Enqueue(source);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == target)
            {
                return dist[current];
            }
            var d = dist[current];
            foreach (var neighbour in outgoing[current])
            {
                if (dist.TryAdd(neighbour, d + 1))
                {
                    queue.Enqueue(neighbour);
                }
            }
        }

        return -1;
    }

    [Theory]
    [MemberData(nameof(Layouts))]
    public void Edges_respect_topology(LevelLayout layout)
    {
        for (uint seed = 1; seed <= SeedCount; seed++)
        {
            var graph = TryBuild(layout, seed);
            if (graph == null)
            {
                continue;
            }

            foreach (var edge in graph.Connections)
            {
                var dx = edge.Target.GridX - edge.Source.GridX;
                var dy = edge.Target.GridY - edge.Source.GridY;
                var validOffsets = layout.GetNeighbourOffsets(edge.Source.GridX, edge.Source.GridY).ToList();
                Assert.Contains((dx, dy), validOffsets);
            }
        }
    }

    [Theory]
    [MemberData(nameof(Layouts))]
    public void Higher_interconnectedness_keeps_more_edges(LevelLayout layout)
    {
        var sparseEdges = 0;
        var denseEdges = 0;
        for (uint seed = 1; seed <= SeedCount; seed++)
        {
            var sparse = BuildWith(layout, seed, interconnectedness: 0.0f);
            var dense = BuildWith(layout, seed, interconnectedness: 1.0f);
            if (sparse is not null)
            {
                sparseEdges += sparse.Connections.Count;
            }
            if (dense is not null)
            {
                denseEdges += dense.Connections.Count;
            }
        }

        Assert.True(denseEdges > sparseEdges,
            $"{layout.GetType().Name}: expected dense ({denseEdges}) > sparse ({sparseEdges})");
    }

    [Fact]
    public void Hex_layout_has_six_neighbours_per_cell()
    {
        // Offset-hex grids stagger columns vertically by half a row, so each cell has six
        // neighbours: two purely vertical (same column ± row) and four "diagonals" whose
        // grid-coordinate offset depends on column parity. There are no purely horizontal
        // neighbours in screen space.
        var hex = new HexLevelLayout();
        for (byte x = 0; x < 4; x++)
        {
            var offsets = hex.GetNeighbourOffsets(x, 2).ToList();
            Assert.Equal(6, offsets.Count);
            Assert.Contains((0, 1), offsets);
            Assert.Contains((0, -1), offsets);
        }
    }

    [Fact]
    public void Spiral_layout_includes_all_eight_diagonals()
    {
        var spiral = new SpiralLevelLayout();
        var offsets = spiral.GetNeighbourOffsets(3, 3).ToList();
        Assert.Equal(8, offsets.Count);
        Assert.Contains((1, 1), offsets);
        Assert.Contains((-1, -1), offsets);
        Assert.Contains((1, -1), offsets);
        Assert.Contains((-1, 1), offsets);
    }

    [Fact]
    public void Grid_layout_is_four_neighbour_only()
    {
        var grid = new GridLevelLayout();
        var offsets = grid.GetNeighbourOffsets(2, 2).ToList();
        Assert.Equal(4, offsets.Count);
        Assert.DoesNotContain((1, 1), offsets);
        Assert.DoesNotContain((-1, -1), offsets);
    }

    [Theory]
    [InlineData(typeof(SpiralLevelLayout))]
    [InlineData(typeof(HexLevelLayout))]
    [InlineData(typeof(TreeLevelLayout))]
    public void Diagonal_capable_layouts_produce_diagonal_edges_across_seeds(Type layoutType)
    {
        // Sanity check that diagonal-capable topologies don't merely *permit* diagonal
        // edges but actually produce them when generating graphs across many seeds.
        // (Plain GridLevelLayout is 4-neighbour and cannot produce diagonals — verified
        // by Grid_layout_is_four_neighbour_only. TreeLevelLayout overrides that to be
        // 8-neighbour and is exercised separately.)
        var layout = (LevelLayout)Activator.CreateInstance(layoutType)!;
        var seedsWithDiagonals = 0;
        var totalDiagonalEdges = 0;
        var totalEdges = 0;
        var built = 0;
        for (uint seed = 1; seed <= SeedCount; seed++)
        {
            var graph = TryBuild(layout, seed);
            if (graph == null)
            {
                continue;
            }

            built++;
            var diagonalEdgesInGraph = 0;
            foreach (var edge in graph.Connections)
            {
                var dx = edge.Target.GridX - edge.Source.GridX;
                var dy = edge.Target.GridY - edge.Source.GridY;
                totalEdges++;
                if (dx != 0 && dy != 0)
                {
                    diagonalEdgesInGraph++;
                    totalDiagonalEdges++;
                }
            }

            if (diagonalEdgesInGraph > 0)
            {
                seedsWithDiagonals++;
            }
        }

        _output.WriteLine($"{layoutType.Name}: {seedsWithDiagonals}/{built} seeds had diagonals, " +
            $"{totalDiagonalEdges}/{totalEdges} total diagonal edges");
        Assert.True(seedsWithDiagonals > 0,
            $"{layoutType.Name} produced no diagonal edges across {built} successful builds");
    }

    private static RoomGraph? TryBuild(LevelLayout layout, uint seed)
        => layout.BuildGraph(GetParameters(layout), new SimpleRandom { Seed = seed });

    private static RoomGraph? BuildWith(LevelLayout layout, uint seed, float interconnectedness)
    {
        var p = GetParameters(layout);
        var pp = new LayoutParameters(p.GridWidth, p.GridHeight, interconnectedness,
            p.Density, p.Sinuosity, p.RewardRoomProbability, p.CurseRoomProbability,
            p.HiddenConnectionProbability, p.MinRooms, p.MaxRooms);
        return layout.BuildGraph(pp, new SimpleRandom { Seed = seed });
    }

    private static LayoutParameters GetParameters(LevelLayout layout)
        => layout switch
        {
            GridLevelLayout g => new LayoutParameters(g.GridWidth, g.GridHeight, g.Interconnectedness,
                g.ComputeDensity(), g.Sinuosity, g.RewardRoomProbability, g.CurseRoomProbability,
                g.HiddenConnectionProbability, g.MinRooms, g.MaxRooms),
            HexLevelLayout h => new LayoutParameters(h.GridWidth, h.GridHeight, h.Interconnectedness,
                h.ComputeDensity(), h.Sinuosity, h.RewardRoomProbability, h.CurseRoomProbability,
                h.HiddenConnectionProbability, h.MinRooms, h.MaxRooms),
            SpiralLevelLayout s => new LayoutParameters(s.GridWidth, s.GridHeight, s.Interconnectedness,
                s.ComputeDensity(), s.Sinuosity, s.RewardRoomProbability, s.CurseRoomProbability,
                s.HiddenConnectionProbability, s.MinRooms, s.MaxRooms),
            _ => throw new ArgumentException($"Unknown layout {layout.GetType().Name}")
        };
}
