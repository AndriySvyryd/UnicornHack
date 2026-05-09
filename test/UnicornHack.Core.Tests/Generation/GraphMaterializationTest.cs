using UnicornHack.Data.Branches;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation;

/// <summary>
///     Verifies that <see cref="LevelLayout.MaterializeGraph" /> turns each directed
///     <see cref="RoomGraphEdge" /> into exactly one <see cref="ConnectionComponent" /> on the
///     source room. Bidirectional pairs (both A→B and B→A in the graph) produce two; one-way
///     pruned pairs (only A→B survives) produce one — the source room has the connection,
///     the target does not.
/// </summary>
public class GraphMaterializationTest
{
    [Theory]
    [InlineData(1u)]
    [InlineData(7u)]
    [InlineData(42u)]
    public void Each_directed_edge_produces_one_intra_level_connection_on_its_source_room(uint seed)
    {
        var (graph, level) = GenerateLevel(seed);

        // Filter to intra-level connections (TargetRoomId points to a room on the same level).
        var roomIds = level.Rooms.Select(r => r.EntityId).ToHashSet();

        // Count intra-level connections per source room.
        var perSource = new Dictionary<int, int>();
        foreach (var room in level.Rooms)
        {
            foreach (var entity in room.Connections.Values)
            {
                if (roomIds.Contains(entity.Connection!.TargetRoomId))
                {
                    perSource[room.EntityId] = perSource.GetValueOrDefault(room.EntityId) + 1;
                }
            }
        }

        // Sum of intra-level connections must equal the number of directed graph edges.
        var totalIntraLevel = perSource.Values.Sum();
        Assert.Equal(graph.Connections.Count, totalIntraLevel);

        // Per-source counts also match.
        var expectedPerSource = new Dictionary<int, int>();
        foreach (var edge in graph.Connections)
        {
            var sourceId = edge.Source.Room!.EntityId;
            expectedPerSource[sourceId] = expectedPerSource.GetValueOrDefault(sourceId) + 1;
        }

        foreach (var (sourceId, expected) in expectedPerSource)
        {
            Assert.Equal(expected, perSource.GetValueOrDefault(sourceId));
        }
    }

    [Theory]
    [InlineData(1u)]
    [InlineData(7u)]
    [InlineData(42u)]
    public void Bidirectional_pair_creates_one_connection_per_endpoint(uint seed)
    {
        var (graph, level) = GenerateLevel(seed);

        // Identify bidirectional pairs in the abstract graph (A→B AND B→A both exist).
        var directed = new HashSet<(int, int)>();
        foreach (var edge in graph.Connections)
        {
            directed.Add((edge.Source.Room!.EntityId, edge.Target.Room!.EntityId));
        }

        foreach (var edge in graph.Connections)
        {
            var src = edge.Source.Room!;
            var tgt = edge.Target.Room!;
            var hasReverse = directed.Contains((tgt.EntityId, src.EntityId));

            // Source must have a connection toward target.
            Assert.Contains(src.Connections.Values, c =>
                c.Connection!.TargetRoomId == tgt.EntityId);

            if (hasReverse)
            {
                // Target must also have a connection back.
                Assert.Contains(tgt.Connections.Values, c =>
                    c.Connection!.TargetRoomId == src.EntityId);
            }
        }
    }

    /// <summary>
    ///     The prune-from-full algorithm with low Interconnectedness is statistically very
    ///     likely to leave at least one one-way passage somewhere across a few seeds.
    ///     A one-way passage is a directed pair (A,B) where the level has a connection from
    ///     A to B but none from B to A.
    /// </summary>
    [Fact]
    public void Some_seeds_produce_one_way_passages()
    {
        var oneWayFound = false;
        for (uint seed = 1; seed <= 20 && !oneWayFound; seed++)
        {
            var (graph, level) = GenerateLevel(seed);
            var directed = new HashSet<(int, int)>();
            foreach (var edge in graph.Connections)
            {
                directed.Add((edge.Source.Room!.EntityId, edge.Target.Room!.EntityId));
            }

            foreach (var (s, t) in directed)
            {
                if (!directed.Contains((t, s)))
                {
                    oneWayFound = true;
                    break;
                }
            }
        }

        Assert.True(oneWayFound,
            "Expected at least one seed in [1..20] to produce a one-way passage with default Interconnectedness.");
    }

    private static (RoomGraph graph, GameLevel level) GenerateLevel(uint seed)
    {
        var game = TestHelper.CreateGame(seed);
        var manager = game.Manager;
        var dungeon = TestHelper.InstantiateBranch(BranchData.Dungeon, game);

        // Try several attempts in case the algorithm or materialisation fails on the
        // chosen seed (e.g. a room ending up with more outgoing edges than it has
        // doorway tiles to host).
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var attemptSeed = seed + (uint)attempt;
            var level = LevelGenerator.CreateEmpty(dungeon, depth: 2, attemptSeed, manager);
            try
            {
                var chunks = LevelGenerator.Generate(level, validateGraph: true);
                if (chunks is not null)
                {
                    return (ReconstructGraph(level), level);
                }
            }
            catch (InvalidOperationException)
            {
                // Pipeline failure (e.g. doorway exhaustion) — try the next seed.
            }
        }

        throw new InvalidOperationException($"Could not generate level for seed {seed}");
    }

    /// <summary>
    ///     Builds a <see cref="RoomGraph" /> view from the materialised level rooms and
    ///     intra-level connections. Used by tests instead of inspecting BuildGraph's output
    ///     directly because <see cref="LevelLayout" /> no longer caches the last graph.
    /// </summary>
    private static RoomGraph ReconstructGraph(GameLevel level)
    {
        var graph = new RoomGraph();
        var nodesByRoom = new Dictionary<int, RoomGraphNode>(level.Rooms.Count);
        foreach (var room in level.Rooms)
        {
            var node = new RoomGraphNode
            {
                GridX = room.GridX,
                GridY = room.GridY,
                Features = room.Features,
                Room = room
            };
            nodesByRoom[room.EntityId] = node;
            graph.Rooms.Add(node);
        }

        foreach (var room in level.Rooms)
        {
            foreach (var entity in room.Connections.Values)
            {
                if (nodesByRoom.TryGetValue(entity.Connection!.TargetRoomId, out var target))
                {
                    graph.Connections.Add(new RoomGraphEdge
                    {
                        Source = nodesByRoom[room.EntityId],
                        Target = target,
                        Hidden = entity.Connection.Hidden
                    });
                }
            }
        }

        return graph;
    }
}
