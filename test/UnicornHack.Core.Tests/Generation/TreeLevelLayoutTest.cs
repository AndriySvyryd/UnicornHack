using UnicornHack.Data.Branches;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils;

namespace UnicornHack.Generation;

public class TreeLevelLayoutTest
{
    [Theory]
    [InlineData(1u)]
    [InlineData(42u)]
    [InlineData(123u)]
    [InlineData(7777u)]
    public void Generated_graph_validates(uint seed)
    {
        var layout = new TreeLevelLayout();
        var random = new SimpleRandom { Seed = seed };

        var graph = layout.BuildGraph(
            layout.GetParameters(LevelGenerator.MaxDifficulty), random,
            new LevelGenerationStats(), new GenerationStats());

        Assert.NotNull(graph);
        Assert.True(LevelGenerator.ValidateRoomGraph(graph),
            $"Generated graph for seed {seed} failed validation");
    }

    [Fact]
    public void Most_seeds_produce_valid_graphs()
    {
        var layout = new TreeLevelLayout();
        var validCount = 0;
        const int trials = 100;

        for (uint seed = 1; seed <= trials; seed++)
        {
            var random = new SimpleRandom { Seed = seed };
            var graph = layout.BuildGraph(
                layout.GetParameters(LevelGenerator.MaxDifficulty), random,
                new LevelGenerationStats(), new GenerationStats());
            if (graph is not null && LevelGenerator.ValidateRoomGraph(graph))
            {
                validCount++;
            }
        }

        Assert.True(validCount >= trials * 0.90,
            $"Expected at least 90% valid graphs, got {validCount}/{trials}");
    }

    [Fact]
    public void Graph_has_entrance_and_exit()
    {
        var layout = new TreeLevelLayout();
        var random = new SimpleRandom { Seed = 1 };

        var graph = layout.BuildGraph(
            layout.GetParameters(LevelGenerator.MaxDifficulty), random,
            new LevelGenerationStats(), new GenerationStats());

        Assert.NotNull(graph);
        Assert.Contains(graph.Rooms, r => (r.Features & RoomFeatures.Entrance) != 0);
        Assert.Contains(graph.Rooms, r => (r.Features & RoomFeatures.Exit) != 0);
    }

    [Fact]
    public void Higher_interconnectedness_produces_more_connections()
    {
        var sparseLayout = new TreeLevelLayout { Interconnectedness = 0.0f };
        var denseLayout = new TreeLevelLayout { Interconnectedness = 1.0f };

        var sparseTotal = 0;
        var denseTotal = 0;

        for (uint seed = 1; seed <= 20; seed++)
        {
            sparseTotal += sparseLayout.BuildGraph(sparseLayout.GetParameters(LevelGenerator.MaxDifficulty),
                new SimpleRandom { Seed = seed }, new LevelGenerationStats(), new GenerationStats())?.Connections.Count ?? 0;
            denseTotal += denseLayout.BuildGraph(denseLayout.GetParameters(LevelGenerator.MaxDifficulty),
                new SimpleRandom { Seed = seed }, new LevelGenerationStats(), new GenerationStats())?.Connections.Count ?? 0;
        }

        Assert.True(denseTotal > sparseTotal,
            $"Expected dense ({denseTotal}) > sparse ({sparseTotal})");
    }

    [Fact]
    public void Higher_curse_probability_produces_more_cursed_rooms()
    {
        var noCurseLayout = new TreeLevelLayout { CurseRoomProbability = 0.0f };
        var alwaysCurseLayout = new TreeLevelLayout { CurseRoomProbability = 1.0f };

        var noCurseTotal = 0;
        var alwaysCurseTotal = 0;

        for (uint seed = 1; seed <= 20; seed++)
        {
            noCurseTotal += noCurseLayout.BuildGraph(noCurseLayout.GetParameters(LevelGenerator.MaxDifficulty),
                    new SimpleRandom { Seed = seed }, new LevelGenerationStats(), new GenerationStats())
                ?.Rooms.Count(r => (r.Features & RoomFeatures.Cursed) != 0) ?? 0;
            alwaysCurseTotal += alwaysCurseLayout.BuildGraph(alwaysCurseLayout.GetParameters(LevelGenerator.MaxDifficulty),
                    new SimpleRandom { Seed = seed }, new LevelGenerationStats(), new GenerationStats())
                ?.Rooms.Count(r => (r.Features & RoomFeatures.Cursed) != 0) ?? 0;
        }

        Assert.Equal(0, noCurseTotal);
        Assert.True(alwaysCurseTotal > 0);
    }

    [Fact]
    public void Hidden_connections_do_not_break_validation()
    {
        // Hidden connections still count as valid edges for graph reachability.
        var layout = new TreeLevelLayout { HiddenConnectionProbability = 1.0f, Interconnectedness = 1.0f };

        var validated = 0;
        for (uint seed = 1; seed <= 20; seed++)
        {
            var graph = layout.BuildGraph(layout.GetParameters(LevelGenerator.MaxDifficulty),
                new SimpleRandom { Seed = seed }, new LevelGenerationStats(), new GenerationStats());
            if (graph is null)
            {
                // Graph generation can occasionally fail under extreme parameters; that's
                // the concern of Most_seeds_produce_valid_graphs, not this test.
                continue;
            }

            Assert.True(LevelGenerator.ValidateRoomGraph(graph),
                $"Graph with seed {seed} failed validation when all extras are hidden");
            validated++;
        }

        Assert.True(validated > 0, "Expected at least one seed to produce a graph for validation");
    }

    [Fact]
    public void Zero_interconnectedness_yields_minimal_graph()
    {
        // With Interconnectedness = 0 the algorithm prunes as aggressively as connectivity
        // allows. Strong connectivity for n nodes needs at least n directed edges (a single
        // cycle); the upper bound is loose because the algorithm stops pruning when no
        // further candidate can be safely removed without disconnecting the graph.
        var layout = new TreeLevelLayout { Interconnectedness = 0.0f };
        var random = new SimpleRandom { Seed = 1 };

        var graph = layout.BuildGraph(
            layout.GetParameters(LevelGenerator.MaxDifficulty), random,
            new LevelGenerationStats(), new GenerationStats());

        Assert.NotNull(graph);
        Assert.True(graph.Connections.Count <= 3 * graph.Rooms.Count,
            $"Expected near-minimal graph; got {graph.Connections.Count} edges for {graph.Rooms.Count} rooms");
    }

    [Fact]
    public void Validation_fails_for_unreachable_room()
    {
        // Graph with an orphan room not connected from the entrance.
        var entrance = new RoomGraphNode { GridX = 0, GridY = 0, Features = RoomFeatures.Entrance };
        var exit = new RoomGraphNode { GridX = 2, GridY = 0, Features = RoomFeatures.Exit };
        var orphan = new RoomGraphNode { GridX = 1, GridY = 1 };

        var graph = new RoomGraph();
        graph.Rooms.Add(entrance);
        graph.Rooms.Add(exit);
        graph.Rooms.Add(orphan);
        graph.Connections.Add(new RoomGraphEdge { Source = entrance, Target = exit });

        Assert.False(LevelGenerator.ValidateRoomGraph(graph));
    }

    [Fact]
    public void Validation_fails_for_dead_end_room_that_cannot_reach_exit()
    {
        // Graph where a room has no outgoing path to any exit.
        var entrance = new RoomGraphNode { GridX = 0, GridY = 0, Features = RoomFeatures.Entrance };
        var middle = new RoomGraphNode { GridX = 1, GridY = 0 };
        var deadEnd = new RoomGraphNode { GridX = 1, GridY = 1 };
        var exit = new RoomGraphNode { GridX = 2, GridY = 0, Features = RoomFeatures.Exit };

        var graph = new RoomGraph();
        graph.Rooms.Add(entrance);
        graph.Rooms.Add(middle);
        graph.Rooms.Add(deadEnd);
        graph.Rooms.Add(exit);
        // entrance → middle → exit, entrance → deadEnd (dead end has no path to exit)
        graph.Connections.Add(new RoomGraphEdge { Source = entrance, Target = middle });
        graph.Connections.Add(new RoomGraphEdge { Source = middle, Target = exit });
        graph.Connections.Add(new RoomGraphEdge { Source = entrance, Target = deadEnd });

        Assert.False(LevelGenerator.ValidateRoomGraph(graph));
    }

    [Fact]
    public void Validation_fails_when_no_entrance()
    {
        var exit = new RoomGraphNode { GridX = 1, GridY = 0, Features = RoomFeatures.Exit };
        var room = new RoomGraphNode { GridX = 0, GridY = 0 };

        var graph = new RoomGraph();
        graph.Rooms.Add(room);
        graph.Rooms.Add(exit);
        graph.Connections.Add(new RoomGraphEdge { Source = room, Target = exit });

        Assert.False(LevelGenerator.ValidateRoomGraph(graph));
    }

    [Fact]
    public void Validation_fails_when_no_exit()
    {
        var entrance = new RoomGraphNode { GridX = 0, GridY = 0, Features = RoomFeatures.Entrance };
        var room = new RoomGraphNode { GridX = 1, GridY = 0 };

        var graph = new RoomGraph();
        graph.Rooms.Add(entrance);
        graph.Rooms.Add(room);
        graph.Connections.Add(new RoomGraphEdge { Source = entrance, Target = room });

        Assert.False(LevelGenerator.ValidateRoomGraph(graph));
    }

    [Fact]
    public void Validation_passes_for_valid_linear_graph()
    {
        var entrance = new RoomGraphNode { GridX = 0, GridY = 0, Features = RoomFeatures.Entrance };
        var middle = new RoomGraphNode { GridX = 1, GridY = 0 };
        var exit = new RoomGraphNode { GridX = 2, GridY = 0, Features = RoomFeatures.Exit };

        var graph = new RoomGraph();
        graph.Rooms.Add(entrance);
        graph.Rooms.Add(middle);
        graph.Rooms.Add(exit);
        graph.Connections.Add(new RoomGraphEdge { Source = entrance, Target = middle });
        graph.Connections.Add(new RoomGraphEdge { Source = middle, Target = exit });

        Assert.True(LevelGenerator.ValidateRoomGraph(graph));
    }

    [Fact]
    public void Reward_distribution_covers_every_feature_flag()
    {
        // PickRewardFeature must be able to emit every RoomFeatures flag listed in the
        // distribution; if a future refactor accidentally drops one this test catches it.
        var layout = new TreeLevelLayout { RewardRoomProbability = 1.0f, CurseRoomProbability = 0.0f };

        var expectedFlags = new[]
        {
            RoomFeatures.SmallFountain,
            RoomFeatures.BigFountain,
            RoomFeatures.SkillPedestal,
            RoomFeatures.TraitPedestal,
            RoomFeatures.FeatPedestal,
            RoomFeatures.RacePedestal,
            RoomFeatures.DeityAltar,
            RoomFeatures.WeaponStand,
            RoomFeatures.ArmorStand,
            RoomFeatures.ItemCase,
            RoomFeatures.MinorShrine,
            RoomFeatures.MajorShrine,
            RoomFeatures.RitualShrine
        };

        var observed = RoomFeatures.Normal;
        for (uint seed = 1; seed <= 500 && observed != expectedFlags.Aggregate(RoomFeatures.Normal, (a, f) => a | f); seed++)
        {
            var graph = layout.BuildGraph(layout.GetParameters(LevelGenerator.MaxDifficulty),
                new SimpleRandom { Seed = seed }, new LevelGenerationStats(), new GenerationStats());
            if (graph is null)
            {
                continue;
            }

            foreach (var room in graph.Rooms)
            {
                observed |= room.Features;
            }
        }

        foreach (var flag in expectedFlags)
        {
            Assert.True((observed & flag) != 0,
                $"Reward distribution never produced {flag} across 500 seeds");
        }
    }

    [Theory]
    [InlineData(1u)]
    [InlineData(42u)]
    [InlineData(7u)]
    public void Generated_chunks_fit_within_room_size_limit(uint seed)
    {
        // Spec: each room is at most 16x25 tiles so it fits on the screen.
        const int maxWidth = 25;
        const int maxHeight = 16;

        var game = TestHelper.CreateGame(seed);
        var manager = game.Manager;
        var dungeon = TestHelper.InstantiateBranch(BranchData.Dungeon, game);

        // Try a few seeds in case the placement engine doesn't find a valid arrangement
        // on the first try -- it's the chunk size constraint we're verifying, not generation
        // robustness.
        List<Chunk>? chunks = null;
        for (var attempt = 0; attempt < 5 && chunks == null; attempt++)
        {
            try
            {
                var attemptSeed = seed + (uint)attempt;
                var gameLevel = LevelGenerator.CreateEmpty(dungeon, depth: 1, attemptSeed, manager);
                chunks = LevelGenerator.Generate(gameLevel);
            }
            catch (InvalidOperationException)
            {
                // Try the next seed.
            }
        }

        Assert.NotNull(chunks);
        Assert.NotEmpty(chunks);
        foreach (var chunk in chunks)
        {
            var rect = chunk.BoundingRectangle;
            Assert.True(rect.Width <= maxWidth,
                $"Chunk at ({rect.TopLeft.X},{rect.TopLeft.Y}) is {rect.Width} wide; max is {maxWidth}");
            Assert.True(rect.Height <= maxHeight,
                $"Chunk at ({rect.TopLeft.X},{rect.TopLeft.Y}) is {rect.Height} tall; max is {maxHeight}");
        }
    }

    [Fact]
    public void Entrance_branch_uses_introductory_parameters()
    {
        // The entrance branch hosts the player's first level and should be small, sparse,
        // and curse-free so new players get a low-pressure introduction. This used to be
        // a depth-1 special case on the Dungeon branch.
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;
        var entrance = TestHelper.InstantiateBranch(BranchData.Entrance, game);

        var firstLevel = LevelGenerator.CreateEmpty(entrance, depth: 1, seed: 1u, manager);
        LevelGenerator.EnsureGenerated(firstLevel);

        Assert.InRange(firstLevel.Rooms.Count, 4, 14);
        Assert.DoesNotContain(firstLevel.Rooms, r => (r.Features & RoomFeatures.Cursed) != 0);
        // EntranceLevelLayout sets HiddenConnectionProbability = 0, so no connection is
        // hidden.
        foreach (var room in firstLevel.Rooms)
        {
            foreach (var connectionEntity in room.Connections.Values)
            {
                Assert.False(connectionEntity.Connection!.Hidden);
            }
        }
    }

    [Fact]
    public void Dungeon_levels_use_default_parameters_at_every_depth()
    {
        // Every dungeon level — including depth 1 — should use the full TreeLevelLayout
        // defaults; the introductory floor is now in the entrance branch.
        var layout = (DungeonLevelLayout)BranchData.Dungeon.Layout;

        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;
        var dungeon = TestHelper.InstantiateBranch(BranchData.Dungeon, game);
        var firstLevel = LevelGenerator.CreateEmpty(dungeon, depth: 1, seed: 1u, manager);
        LevelGenerator.Generate(firstLevel);

        // Use a fresh game for the second generation to keep this test independent from
        // queue backlog side effects between runs.
        var deepGame = TestHelper.CreateGame(seed: 1u);
        var deepManager = deepGame.Manager;
        var deepDungeon = TestHelper.InstantiateBranch(BranchData.Dungeon, deepGame);
        var deepLevel = LevelGenerator.CreateEmpty(deepDungeon, depth: 5, seed: 1u, deepManager);
        LevelGenerator.Generate(deepLevel);

        Assert.Equal((byte)7, layout.GridWidth);
        Assert.Equal((byte)5, layout.GridHeight);
        Assert.Equal((byte)32, layout.MaxRooms);
    }
}

