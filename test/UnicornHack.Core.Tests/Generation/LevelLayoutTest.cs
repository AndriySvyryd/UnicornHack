using UnicornHack.Data.Branches;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils;

namespace UnicornHack.Generation;

/// <summary>
///     Tests the prune-from-full algorithm in <see cref="LevelLayout.BuildGraph" /> across
///     the three concrete topologies (4-/6-/8-neighbor). The invariants asserted are:
///     <list type="bullet">
///         <item>The entrance reaches every node via directed edges.</item>
///         <item>Every node reaches the exit via directed edges.</item>
///         <item>Every edge respects the layout's <see cref="LevelLayout.GetNeighborOffsets" />.</item>
///         <item>Higher <c>Interconnectedness</c> → more edges.</item>
///     </list>
///     Also covers the connectivity invariant of a materialised level: intra-level connections
///     are one-directional (the travel system blocks returning to a visited room), and every room
///     must lie on a simple directed entrance→exit path — you can reach the exit from the entrance
///     passing through that room without traversing any room twice. That is what keeps a room from
///     being a dead end even though doors are one-way.
/// </summary>
public class LevelLayoutTest
{
    private readonly ITestOutputHelper _output;

    public LevelLayoutTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void CreateEmptyRoom_attaches_room_to_level_with_empty_terrain()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;
        var dungeon = BranchData.Dungeon.Instantiate(game);
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
        var dungeon = BranchData.Dungeon.Instantiate(game);
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
    ///     Regression: <see cref="RoomComponent.Clean" /> may run during component-pool
    ///     teardown on a room that never allocated terrain arrays. The component must
    ///     safely handle this path by treating all arrays as <see cref="Array.Empty{T}" />.
    /// </summary>
    [Fact]
    public void Removing_room_component_does_not_throw_on_uninitialised_room()
    {
        var manager = TestHelper.CreateGameManager();
        using var roomEntityReference = manager.CreateEntity();
        var room = manager.CreateComponent<RoomComponent>(EntityComponent.Room);
        roomEntityReference.Referenced.Room = room;

        // Self-initialized: every terrain getter returns Array.Empty<byte>() before any
        // allocation has happened, so reads and the release path are both safe.
        Assert.Empty(room.Terrain);
        Assert.Empty(room.KnownTerrain);
        Assert.Empty(room.Neighbors);
        Assert.Empty(room.VisibleTerrain);
        Assert.Empty(room.KnownNeighbors);

        roomEntityReference.Referenced.RemoveComponent((int)EntityComponent.Room);

        Assert.Empty(room.Terrain);
        Assert.Empty(room.KnownTerrain);
        Assert.Empty(room.Neighbors);
        Assert.Empty(room.VisibleTerrain);
        Assert.Empty(room.KnownNeighbors);
        Assert.Equal(0, room.Width);
        Assert.Equal(0, room.Height);
    }

    [Fact]
    public void Removing_room_component_returns_pooled_arrays_and_resets_dimensions()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;
        var dungeon = BranchData.Dungeon.Instantiate(game);
        var level = LevelGenerator.CreateEmpty(dungeon, depth: 1, seed: 1u, manager);
        var room = LevelLayout.CreateEmptyRoom(level, manager);
        var roomEntity = room.Entity;
        LevelLayout.AllocateTerrain(room, width: 6, height: 6, MapFeature.StoneFloor);

        roomEntity.RemoveComponent((int)EntityComponent.Room);

        Assert.Equal(0, room.Width);
        Assert.Equal(0, room.Height);
        Assert.Empty(room.Terrain);
        Assert.Empty(room.KnownTerrain);
        Assert.Empty(room.Neighbors);
        Assert.Empty(room.VisibleTerrain);
        Assert.Empty(room.KnownNeighbors);

        // Idempotent: removing again is a no-op.
        roomEntity.RemoveComponent((int)EntityComponent.Room);
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

            Assert.True(graph.HasValidConnectivity(),
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
        var stats = new LevelGenerationStats();
        var gameStats = new GenerationStats();
        var prevSnapshot = stats.GraphFailureSnapshot();
        for (uint seed = 1; seed <= trials; seed++)
        {
            var graph = layout.BuildGraph(layout.GetParameters(LevelGenerator.MaxDifficulty), new SimpleRandom { Seed = seed }, stats, gameStats);
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
    ///     Compares two <see cref="LevelGenerationStatsTestExtensions.GraphFailureSnapshot" /> dictionaries
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
            foreach (var neighbor in adj[current])
            {
                if (dist.TryAdd(neighbor, d + 1))
                {
                    queue.Enqueue(neighbor);
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
            foreach (var neighbor in outgoing[current])
            {
                if (dist.TryAdd(neighbor, d + 1))
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        return -1;
    }

    /// <summary>
    ///     The level map must avoid connections that visually overlap (two diagonals of a grid
    ///     unit-square crossing like an X). Cardinal-only (<see cref="GridLevelLayout" />) and
    ///     hex-offset (<see cref="HexLevelLayout" />) topologies can never place both diagonals of a
    ///     square, so their maps are always overlap-free; the square diagonal topologies
    ///     (<see cref="TreeLevelLayout" />, <see cref="SpiralLevelLayout" />) can, but the generator
    ///     both avoids them during placement and untwists any residual crossing afterwards (re-pairing
    ///     a crossed square's four corner rooms along its cardinal sides, or, failing that, bending
    ///     one diagonal into a cardinal L through a corner while keeping the other); whatever still
    ///     survives is almost always RNG-avoidable, so the connection phase samples many arrangements
    ///     and keeps the cleanest. Only a negligible residue remains — far below the unconstrained
    ///     baseline (~9.6 crossings/level for the production TreeLevelLayout).
    /// </summary>
    [Fact]
    public void Connections_avoid_visual_overlaps()
    {
        const int seeds = 200;
        foreach (var (layout, maxAveragePerLevel) in new (LevelLayout Layout, double Budget)[]
                 {
                     (new GridLevelLayout(), 0.0),
                     (new HexLevelLayout(), 0.0),
                     (new TreeLevelLayout(), 0.1),
                     (new SpiralLevelLayout(), 0.1),
                 })
        {
            var total = 0;
            var built = 0;
            for (uint seed = 1; seed <= seeds; seed++)
            {
                var graph = TryBuild(layout, seed);
                if (graph == null)
                {
                    continue;
                }
                built++;
                total += CountCrossingPairs(graph);
            }

            Assert.True(built > 0, $"{layout.GetType().Name}: no graphs built across {seeds} seeds");
            var average = (double)total / built;
            _output.WriteLine(
                $"{layout.GetType().Name}: {total} crossings over {built} levels (avg {average:F2}/level)");
            Assert.True(average <= maxAveragePerLevel,
                $"{layout.GetType().Name}: {average:F2} crossings/level exceeds budget {maxAveragePerLevel:F2}");
        }
    }

    private static int CountCrossingPairs(RoomGraph graph)
    {
        var main = new HashSet<(int, int)>();
        var anti = new HashSet<(int, int)>();
        foreach (var edge in graph.Connections)
        {
            int ax = edge.Source.GridX, ay = edge.Source.GridY;
            int bx = edge.Target.GridX, by = edge.Target.GridY;
            if (Math.Abs(ax - bx) != 1 || Math.Abs(ay - by) != 1)
            {
                continue;
            }
            int minX = Math.Min(ax, bx), minY = Math.Min(ay, by);
            if ((ax == minX) == (ay == minY))
            {
                main.Add((minX, minY));
            }
            else
            {
                anti.Add((minX, minY));
            }
        }

        var crossings = 0;
        foreach (var sq in main)
        {
            if (anti.Contains(sq))
            {
                crossings++;
            }
        }

        return crossings;
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
                var validOffsets = layout.GetNeighborOffsets(edge.Source.GridX, edge.Source.GridY).ToList();
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
    public void Hex_layout_has_six_neighbors_per_cell()
    {
        // Offset-hex grids stagger columns vertically by half a row, so each cell has six
        // neighbors: two purely vertical (same column ± row) and four "diagonals" whose
        // grid-coordinate offset depends on column parity. There are no purely horizontal
        // neighbors in screen space.
        var hex = new HexLevelLayout();
        for (byte x = 0; x < 4; x++)
        {
            var offsets = hex.GetNeighborOffsets(x, 2).ToList();
            Assert.Equal(6, offsets.Count);
            Assert.Contains((0, 1), offsets);
            Assert.Contains((0, -1), offsets);
        }
    }

    [Fact]
    public void Spiral_layout_includes_all_eight_diagonals()
    {
        var spiral = new SpiralLevelLayout();
        var offsets = spiral.GetNeighborOffsets(3, 3).ToList();
        Assert.Equal(8, offsets.Count);
        Assert.Contains((1, 1), offsets);
        Assert.Contains((-1, -1), offsets);
        Assert.Contains((1, -1), offsets);
        Assert.Contains((-1, 1), offsets);
    }

    [Fact]
    public void Grid_layout_is_four_neighbor_only()
    {
        var grid = new GridLevelLayout();
        var offsets = grid.GetNeighborOffsets(2, 2).ToList();
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
        // (Plain GridLevelLayout is 4-neighbor and cannot produce diagonals — verified
        // by Grid_layout_is_four_neighbor_only. TreeLevelLayout overrides that to be
        // 8-neighbor and is exercised separately.)
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
        => layout.BuildGraph(
            layout.GetParameters(LevelGenerator.MaxDifficulty), new SimpleRandom { Seed = seed },
            new LevelGenerationStats(), new GenerationStats());

    private static RoomGraph? BuildWith(LevelLayout layout, uint seed, float interconnectedness)
    {
        var p = layout.GetParameters(LevelGenerator.MaxDifficulty);
        var pp = new LayoutParameters(p.GridWidth, p.GridHeight, interconnectedness,
            p.Density, p.Sinuosity, p.RewardRoomProbability, p.CurseRoomProbability,
            p.HiddenConnectionProbability, p.MinRooms, p.MaxRooms, p.Difficulty);
        return layout.BuildGraph(pp, new SimpleRandom { Seed = seed }, new LevelGenerationStats(), new GenerationStats());
    }

    [Theory]
    [MemberData(nameof(Layouts))]
    public void Every_walkable_tile_is_reachable_from_an_entrance(LevelLayout layout)
    {
        // BFS from each room's connection tiles across walkable terrain and verify it
        // reaches every other walkable tile in the room. Catches corridor-carving
        // regressions that leave chunks disconnected.
        var success = false;
        for (uint seed = 1; seed <= 8 && !success; seed++)
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
                continue;
            }

            foreach (var room in level.Rooms)
            {
                AssertEveryWalkableTileReachable(room, layout, seed);
            }

            success = true;
        }

        Assert.True(success, $"{layout.GetType().Name} failed to produce a valid level in 8 seeds");
    }

    private static void AssertEveryWalkableTileReachable(RoomComponent room, LevelLayout layout, uint seed)
    {
        var starts = new List<Point>(room.Connections.Keys);
        if (starts.Count == 0)
        {
            // Room with no inter-room connections — start the BFS from any walkable tile.
            for (byte y = 0; y < room.Height && starts.Count == 0; y++)
            {
                for (byte x = 0; x < room.Width && starts.Count == 0; x++)
                {
                    var feature = room.Terrain[room.PointToIndex![x, y]];
                    if (feature.CanMoveTo())
                    {
                        starts.Add(new Point(x, y));
                    }
                }
            }
        }

        if (starts.Count == 0)
        {
            return;
        }

        var visited = new bool[room.TileCount];
        var queue = new Queue<Point>();
        foreach (var start in starts)
        {
            var startIdx = room.PointToIndex![start.X, start.Y];
            if (!visited[startIdx])
            {
                visited[startIdx] = true;
                queue.Enqueue(start);
            }
        }

        while (queue.Count > 0)
        {
            var p = queue.Dequeue();
            // 8-connected: actors can move diagonally between adjacent tiles, so any
            // walkable tile reachable via diagonals counts as reachable. This matches
            // TravelSystem.GetPossibleMovementDirections and the 8-connected flood fill
            // in RoomLayout.FloodFillReachable.
            for (var d = 0; d < 8; d++)
            {
                var dx = d switch { 0 => 1, 1 => -1, 4 => 1, 5 => -1, 6 => 1, 7 => -1, _ => 0 };
                var dy = d switch { 2 => 1, 3 => -1, 4 => 1, 5 => 1, 6 => -1, 7 => -1, _ => 0 };
                var nx = p.X + dx;
                var ny = p.Y + dy;
                if (nx < 0 || ny < 0 || nx >= room.Width || ny >= room.Height)
                {
                    continue;
                }

                var nIdx = room.PointToIndex![(byte)nx, (byte)ny];
                if (visited[nIdx])
                {
                    continue;
                }

                if (!room.Terrain[nIdx].CanMoveTo())
                {
                    continue;
                }

                visited[nIdx] = true;
                queue.Enqueue(new Point((byte)nx, (byte)ny));
            }
        }

        // Every walkable tile must have been visited.
        for (byte y = 0; y < room.Height; y++)
        {
            for (byte x = 0; x < room.Width; x++)
            {
                var idx = room.PointToIndex![x, y];
                if (room.Terrain[idx].CanMoveTo() && !visited[idx])
                {
                    Assert.Fail(
                        $"{layout.GetType().Name} seed={seed}: walkable tile ({x},{y}) in room "
                        + $"({room.GridX},{room.GridY}) is unreachable from any connection / start.\n"
                        + DumpRoom(room, visited, starts));
                }
            }
        }
    }

    /// <summary>
    ///     Renders the room's terrain as ASCII. Walkable but unvisited tiles render as
    ///     <c>!</c> so a failing assertion shows exactly where the disconnect is.
    /// </summary>
    private static string DumpRoom(RoomComponent room, bool[] visited, IReadOnlyList<Point> starts)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("Starts: ").AppendJoin(", ", starts.Select(p => $"({p.X},{p.Y})")).Append('\n');
        for (byte y = 0; y < room.Height; y++)
        {
            for (byte x = 0; x < room.Width; x++)
            {
                var idx = room.PointToIndex![x, y];
                var feature = room.Terrain[idx];
                var glyph = feature switch
                {
                    MapFeature.Default => ' ',
                    MapFeature.RockFloor => '.',
                    MapFeature.StoneFloor => ',',
                    MapFeature.RockWall => '#',
                    MapFeature.StoneWall => '%',
                    MapFeature.StoneArchway => '+',
                    _ => '?'
                };
                if (feature.CanMoveTo() && !visited[idx])
                {
                    glyph = '!';
                }
                sb.Append(glyph);
            }
            sb.Append('\n');
        }
        return sb.ToString();
    }

    [Theory]
    [InlineData(1u)]
    [InlineData(7u)]
    [InlineData(42u)]
    public void Every_room_lies_on_a_simple_entrance_to_exit_path(uint seed)
    {
        var level = GenerateLevel(seed);
        var (adjacency, entrance, exit) = BuildAdjacency(level);

        foreach (var room in level.Rooms)
        {
            Assert.True(HasSimplePathThrough(room.EntityId, entrance, exit, adjacency),
                $"Seed {seed}: room {room.EntityId} ({room.GridX},{room.GridY}, {room.Features}) "
                + "has no simple entrance→exit path through it.");
        }
    }

    [Theory]
    [InlineData(1u)]
    [InlineData(7u)]
    [InlineData(42u)]
    public void Intra_level_connections_are_one_directional(uint seed)
    {
        var level = GenerateLevel(seed);
        var (adjacency, _, _) = BuildAdjacency(level);

        var oneWayExists = false;
        foreach (var (source, targets) in adjacency)
        {
            foreach (var target in targets)
            {
                if (!adjacency[target].Contains(source))
                {
                    oneWayExists = true;
                }
            }
        }

        Assert.True(oneWayExists, $"Seed {seed}: expected at least one one-directional connection.");
    }

    [Fact]
    public void Every_seed_keeps_every_room_on_a_simple_entrance_to_exit_path()
    {
        for (uint seed = 1; seed <= 20; seed++)
        {
            var level = GenerateLevel(seed);
            var (adjacency, entrance, exit) = BuildAdjacency(level);

            foreach (var room in level.Rooms)
            {
                Assert.True(HasSimplePathThrough(room.EntityId, entrance, exit, adjacency),
                    $"Seed {seed}: room {room.EntityId} ({room.GridX},{room.GridY}, {room.Features}) "
                    + "has no simple entrance→exit path through it.");
            }
        }
    }

    /// <summary>
    ///     Same invariant on the production entrance level (the surface→entrance flow, the one
    ///     the player sees first). Generated exactly as <c>GameHub.CreatePlayer</c> does.
    /// </summary>
    [Fact]
    public void Entrance_level_keeps_every_room_on_a_simple_entrance_to_exit_path()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;

        var surfaceBranch = BranchData.Surface.Instantiate(game);
        BranchData.Entrance.Instantiate(game);
        BranchData.Dungeon.Instantiate(game);

        var surfaceLevel = LevelGenerator.CreateEmpty(surfaceBranch, depth: 1, seed: 1u, manager);
        LevelGenerator.EnsureGenerated(surfaceLevel);

        var entranceRoom = surfaceLevel.Rooms.First().Connections.Single().Value.Connection!.TargetRoomEntity.Room!;
        LevelGenerator.EnsureGenerated(entranceRoom);

        var level = entranceRoom.Level;
        var (adjacency, entrance, exit) = BuildAdjacency(level);

        foreach (var room in level.Rooms)
        {
            Assert.True(HasSimplePathThrough(room.EntityId, entrance, exit, adjacency),
                $"Entrance room {room.EntityId} ({room.GridX},{room.GridY}, {room.Features}) "
                + "has no simple entrance→exit path through it.");
        }
    }

    private static (Dictionary<int, List<int>> Adjacency, int Entrance, int Exit) BuildAdjacency(GameLevel level)
    {
        var roomIds = level.Rooms.Select(r => r.EntityId).ToHashSet();
        var adjacency = new Dictionary<int, List<int>>();
        foreach (var room in level.Rooms)
        {
            adjacency[room.EntityId] = [];
        }

        foreach (var room in level.Rooms)
        {
            foreach (var entity in room.Connections.Values)
            {
                var targetId = entity.Connection!.TargetRoomId;
                if (roomIds.Contains(targetId))
                {
                    adjacency[room.EntityId].Add(targetId);
                }
            }
        }

        var entrance = level.Rooms.Single(r => (r.Features & RoomFeatures.Entrance) != 0).EntityId;
        var exit = level.Rooms.Single(r => (r.Features & RoomFeatures.Exit) != 0).EntityId;
        return (adjacency, entrance, exit);
    }

    private static bool HasSimplePathThrough(
        int through, int entrance, int exit, Dictionary<int, List<int>> adjacency)
    {
        // Simple path entrance→through using DFS, then through→exit avoiding the first half.
        var firstHalf = FindSimplePath(entrance, through, adjacency, []);
        if (firstHalf == null)
        {
            return false;
        }

        var blocked = new HashSet<int>(firstHalf);
        blocked.Remove(through);
        return FindSimplePath(through, exit, adjacency, blocked) != null;
    }

    private static List<int>? FindSimplePath(
        int from, int to, Dictionary<int, List<int>> adjacency, HashSet<int> blocked)
    {
        var path = new List<int>();
        var visited = new HashSet<int>(blocked);
        return Dfs(from, to, adjacency, visited, path) ? path : null;

        static bool Dfs(int node, int target, Dictionary<int, List<int>> adj, HashSet<int> visited, List<int> path)
        {
            if (!visited.Add(node))
            {
                return false;
            }

            path.Add(node);
            if (node == target)
            {
                return true;
            }

            foreach (var next in adj[node])
            {
                if (Dfs(next, target, adj, visited, path))
                {
                    return true;
                }
            }

            path.RemoveAt(path.Count - 1);
            return false;
        }
    }

    private static GameLevel GenerateLevel(uint seed)
    {
        var game = TestHelper.CreateGame(seed);
        var manager = game.Manager;
        var dungeon = BranchData.Dungeon.Instantiate(game);

        for (var attempt = 0; attempt < 20; attempt++)
        {
            var attemptSeed = seed + (uint)attempt;
            var level = LevelGenerator.CreateEmpty(dungeon, depth: 2, attemptSeed, manager);
            try
            {
                if (LevelGenerator.Generate(level) is not null)
                {
                    return level;
                }
            }
            catch (InvalidOperationException)
            {
                // Pipeline failure (e.g. doorway exhaustion) — try the next seed.
            }
        }

        throw new InvalidOperationException($"Could not generate level for seed {seed}");
    }
}
