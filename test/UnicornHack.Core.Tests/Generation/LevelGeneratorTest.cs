using UnicornHack.Data.Branches;
using UnicornHack.Data.Fragments;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils;

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
        var dungeon = BranchData.Dungeon.Instantiate(game);

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

    /// <summary>
    ///     Regression for the generation queue overflow: generating many levels back-to-back with a
    ///     single <see cref="GameManager" /> and no intervening game turn used to overflow the ECS
    ///     message queue (limit 16384) around the third level, because each generation enqueues
    ///     entity-lifecycle / property-change messages that only a turn would drain.
    ///     <see cref="LevelGenerator.EnsureGenerated(GameLevel)" /> now drains its own messages, so
    ///     a deep descent completes without any manual <c>ProcessQueue</c>.
    /// </summary>
    [Theory]
    [InlineData(1u)]
    [InlineData(42u)]
    public void Deep_descent_does_not_overflow_the_message_queue(uint seed)
    {
        var game = TestHelper.CreateGame(seed);
        var manager = game.Manager;

        var surfaceBranch = BranchData.Surface.Instantiate(game);
        BranchData.Entrance.Instantiate(game);
        BranchData.Dungeon.Instantiate(game);

        var surfaceLevel = LevelGenerator.CreateEmpty(surfaceBranch, depth: 1, seed, manager);
        LevelGenerator.EnsureGenerated(surfaceLevel);

        var room = surfaceLevel.Rooms.First().Connections.Single().Value.Connection!.TargetRoomEntity.Room!;
        var levelsGenerated = 0;

        // Deliberately never call ProcessQueue: the fix makes each EnsureGenerated self-draining.
        for (var i = 0; i < 8 && room != null; i++)
        {
            LevelGenerator.EnsureGenerated(room);
            levelsGenerated++;

            // After generation the queue is drained, so it must be empty.
            Assert.Equal(0, manager.Queue.QueuedCount);

            room = room.Level.Rooms
                .FirstOrDefault(r => (r.Features & RoomFeatures.Exit) != 0)?.Connections.Values
                .Select(c => c.Connection!)
                .FirstOrDefault(c => c.Orientation == Direction.Down)?.TargetRoomEntity.Room;
        }

        // Must have descended well past the ~3-level point where the queue used to overflow.
        Assert.True(levelsGenerated >= 5,
            $"Seed {seed}: only descended {levelsGenerated} levels; expected at least 5.");
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
        var dungeon = BranchData.Dungeon.Instantiate(game);
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
        var dungeon = BranchData.Dungeon.Instantiate(game);
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

    /// <summary>
    ///     Whatever rooms end up concealed (with <see cref="LevelLayout.HiddenConnectionProbability"/>
    ///     spread across the hideable rooms), concealment must stay off the shortest entrance→exit
    ///     path and must never strand a visible predecessor (the cumulative-hiding guard keeps each
    ///     visible room with at least one visible outgoing edge).
    /// </summary>
    [Fact]
    public void Hidden_rooms_stay_off_the_spine_and_never_strand_a_visible_predecessor()
    {
        // A high probability maximises the number of concealed rooms so the invariants below are
        // exercised across many hidden rooms; the exact count is spread by the hideable-room count.
        var layout = new TreeLevelLayout
        {
            HiddenConnectionProbability = 1.0f,
            CurseRoomProbability = 0.0f,
            Sinuosity = 0.0f
        };

        var checkedSeeds = 0;
        for (var seed = 1u; seed <= 20; seed++)
        {
            var graph = layout.BuildGraph(
                layout.GetParameters(LevelGenerator.MaxDifficulty),
                new SimpleRandom { Seed = seed }, new LevelGenerationStats(), new GenerationStats());
            if (graph is null)
            {
                continue;
            }

            checkedSeeds++;
            var entrance = graph.Rooms.Single(r => (r.Features & RoomFeatures.Entrance) != 0);
            var exit = graph.Rooms.Single(r => (r.Features & RoomFeatures.Exit) != 0);

            // A room is hidden iff every connection incident to it is hidden (and it has at
            // least one). Build the per-room incident-edge view.
            var incidentVisible = new Dictionary<RoomGraphNode, int>();
            var incidentTotal = new Dictionary<RoomGraphNode, int>();
            var visibleOutgoing = new Dictionary<RoomGraphNode, int>();
            foreach (var edge in graph.Connections)
            {
                incidentTotal[edge.Source] = incidentTotal.GetValueOrDefault(edge.Source) + 1;
                incidentTotal[edge.Target] = incidentTotal.GetValueOrDefault(edge.Target) + 1;
                if (!edge.Hidden)
                {
                    incidentVisible[edge.Source] = incidentVisible.GetValueOrDefault(edge.Source) + 1;
                    incidentVisible[edge.Target] = incidentVisible.GetValueOrDefault(edge.Target) + 1;
                    visibleOutgoing[edge.Source] = visibleOutgoing.GetValueOrDefault(edge.Source) + 1;
                }
            }

            bool IsHidden(RoomGraphNode r)
                => incidentTotal.GetValueOrDefault(r) > 0 && incidentVisible.GetValueOrDefault(r) == 0;

            var spine = ShortestDirectedPath(graph, entrance, exit).ToHashSet();

            // Entrance and exit are never hidden.
            Assert.False(IsHidden(entrance), "Entrance must never be hidden.");
            Assert.False(IsHidden(exit), "Exit must never be hidden.");

            foreach (var node in graph.Rooms)
            {
                if (IsHidden(node))
                {
                    Assert.DoesNotContain(node, spine);
                }

                // Dead-end invariant: a visible (non-hidden) room other than the exit must
                // retain at least one visible outgoing edge — the cumulative-hiding guard.
                if (!IsHidden(node) && node != exit)
                {
                    Assert.True(visibleOutgoing.GetValueOrDefault(node) >= 1,
                        $"Seed {seed}: visible room ({node.GridX},{node.GridY}) has no visible exit.");
                }
            }
        }

        Assert.True(checkedSeeds > 0, "Expected at least one seed to produce a graph.");
    }

    [Fact]
    public void Shortcut_is_a_hidden_only_reward_that_scales_with_difficulty()
    {
        var probe = new RewardWeightProbe();

        // Visible rooms can never receive the Shortcut reward.
        Assert.Equal(0f, probe.Weight(RoomFeatures.Shortcut, countSoFar: 0, globalCountSoFar: 0, hidden: false));

        // Hidden rooms can, and the weight grows with difficulty (before the clamp).
        var shallow = probe.WeightAtDifficulty(RoomFeatures.Shortcut, 0, 0, hidden: true, difficulty: 1);
        var deep = probe.WeightAtDifficulty(RoomFeatures.Shortcut, 0, 0, hidden: true, difficulty: 8);
        Assert.True(shallow > 0f, "Shortcut must be selectable in a hidden room.");
        Assert.True(deep > shallow, "Shortcut weight must grow with difficulty.");
    }

    [Fact]
    public void Hidden_rooms_bias_their_reward_draw_toward_premium_features()
    {
        var probe = new RewardWeightProbe();

        // A premium feature (RitualShrine) is weighted higher in a hidden room than the same
        // feature in a visible room (the quality multiplier only applies when hidden).
        var visible = probe.Weight(RoomFeatures.RitualShrine, 0, 0, hidden: false);
        var hidden = probe.Weight(RoomFeatures.RitualShrine, 0, 0, hidden: true);
        Assert.True(visible > 0f, "RitualShrine should be available at max difficulty.");
        Assert.True(hidden > visible, "A hidden room must bias toward premium rewards.");
    }

    [Fact]
    public void MaterializeGraph_drops_shortcut_when_combined_with_entrance_or_exit()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;

        var branch = new Branch
        {
            Name = "test-sanitize-branch",
            Length = 5,
            Difficulty = 1,
            Layout = new FixedShortcutGraphLayout(),
            RoomLayout = new UniformRoomLayout()
        };
        var testProvider = (TestDataProvider)game.Services.Data;
        testProvider.DefiningMapFragments.Add(new DefiningMapFragment
        {
            Name = "test-sanitize-defining",
            GenerationWeight = $"$branch == '{branch.Name}' ? 1 : 0",
            NoRandomDoorways = true
        });

        var branchEntity = branch.Instantiate(game);

        GameLevel? generated = null;
        for (var attempt = 0; attempt < 20 && generated == null; attempt++)
        {
            var level = LevelGenerator.CreateEmpty(branchEntity, depth: 1, seed: 1u + (uint)attempt, manager);
            if (branch.Layout.Fill(level, branch) != null)
            {
                generated = level;
            }
        }

        Assert.NotNull(generated);

        // The exit node was authored Exit | Shortcut; the sanitize drops the shortcut bit.
        var exitRoom = generated!.Rooms.Single(r => (r.Features & RoomFeatures.Exit) != 0);
        Assert.True((exitRoom.Features & RoomFeatures.Shortcut) == 0,
            "Exit | Shortcut must collapse to Exit.");

        // The standalone shortcut room keeps the feature (and isn't entrance/exit).
        var shortcutRoom = Assert.Single(generated.Rooms, r => (r.Features & RoomFeatures.Shortcut) != 0);
        Assert.True((shortcutRoom.Features & (RoomFeatures.Entrance | RoomFeatures.Exit)) == 0);
    }

    private static List<RoomGraphNode> ShortestDirectedPath(
        RoomGraph graph, RoomGraphNode source, RoomGraphNode target)
    {
        var adjacency = graph.Connections
            .GroupBy(e => e.Source)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Target).ToList());
        var parent = new Dictionary<RoomGraphNode, RoomGraphNode?> { [source] = null };
        var queue = new Queue<RoomGraphNode>();
        queue.Enqueue(source);
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            if (node == target)
            {
                break;
            }

            if (!adjacency.TryGetValue(node, out var neighbors))
            {
                continue;
            }

            foreach (var next in neighbors)
            {
                if (parent.TryAdd(next, node))
                {
                    queue.Enqueue(next);
                }
            }
        }

        var path = new List<RoomGraphNode>();
        for (RoomGraphNode? n = target; n != null; n = parent.GetValueOrDefault(n))
        {
            path.Add(n);
        }

        path.Reverse();
        return path;
    }

    /// <summary>
    ///     Exposes the protected <see cref="LevelLayout.GetRewardFeatureWeight"/> hook so the
    ///     hidden-room reward weighting can be asserted directly.
    /// </summary>
    private sealed class RewardWeightProbe : TreeLevelLayout
    {
        public float Weight(RoomFeatures feature, int countSoFar, int globalCountSoFar, bool hidden)
            => GetRewardFeatureWeight(feature, countSoFar, globalCountSoFar,
                GetParameters(LevelGenerator.MaxDifficulty), hidden);

        public float WeightAtDifficulty(
            RoomFeatures feature, int countSoFar, int globalCountSoFar, bool hidden, int difficulty)
            => GetRewardFeatureWeight(feature, countSoFar, globalCountSoFar,
                GetParameters(difficulty), hidden);
    }

    /// <summary>
    ///     Materialises a fixed 3-room graph (entrance → shortcut, entrance → exit). The exit
    ///     node is authored with the mutually-exclusive <c>Exit | Shortcut</c> combo to verify
    ///     the sanitize in <c>MaterializeGraph</c>.
    /// </summary>
    private sealed class FixedShortcutGraphLayout : LevelLayout
    {
        public override List<Chunk>? Fill(GameLevel level, Branch branch)
        {
            var entrance = new RoomGraphNode { GridX = 0, GridY = 0, Features = RoomFeatures.Entrance };
            var shortcut = new RoomGraphNode { GridX = 0, GridY = 1, Features = RoomFeatures.Shortcut };
            var exit = new RoomGraphNode
            {
                GridX = 1,
                GridY = 0,
                Features = RoomFeatures.Exit | RoomFeatures.Shortcut
            };

            var graph = new RoomGraph();
            graph.Rooms.AddRange([entrance, shortcut, exit]);
            graph.Connections.Add(new RoomGraphEdge { Source = entrance, Target = shortcut });
            graph.Connections.Add(new RoomGraphEdge { Source = entrance, Target = exit });

            return MaterializeGraph(level, graph, branch);
        }
    }
}
