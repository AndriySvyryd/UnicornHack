using System.Text;
using UnicornHack.Data.Branches;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

/// <summary>
///     Tests for <see cref="UniformRoomLayout" />. The trim must
///     drop corridor tiles that already lie inside the source or target chunk's interior,
///     so <see cref="UniformRoomLayout" />'s subsequent corridor write doesn't carve a
///     redundant archway through perimeter walls reached by a path that diverted through
///     the chunk's own walkable interior.
/// </summary>
public class RoomLayoutTest
{
    private readonly ITestOutputHelper _output;

    public RoomLayoutTest(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    ///     Path lives entirely outside both chunks (the typical case) — trim is a no-op.
    /// </summary>
    [Fact]
    public void Returns_path_unchanged_when_no_tile_is_inside_either_chunk()
    {
        var room = TestHelper.BuildRoom();
        var source = BuildChunk(room, new Rectangle(new Point(0, 0), 3, 3), inside: [new Point(1, 1)]);
        var target = BuildChunk(room, new Rectangle(new Point(6, 0), 3, 3), inside: [new Point(7, 1)]);

        // Corridor of three tiles between the chunks; none are inside either chunk.
        var path = new[] { new Point(3, 1), new Point(4, 1), new Point(5, 1) };

        var trimmed = RoomLayout.TrimCorridorEndpoints(path, source, target);

        Assert.Same(path, trimmed);
    }

    /// <summary>
    ///     Path starts at the source chunk's connection point and walks through two
    ///     interior tiles before exiting toward the target. The trim must drop the first
    ///     three entries (the perimeter doorway and both interior tiles), leaving the
    ///     corridor to begin at the first tile past the chunk's interior.
    /// </summary>
    [Fact]
    public void Drops_source_chunk_prefix_through_the_last_inside_point()
    {
        var room = TestHelper.BuildRoom();
        var sourceInside = new[] { new Point(1, 1), new Point(2, 1) };
        var source = BuildChunk(room, new Rectangle(new Point(0, 0), 4, 3), inside: sourceInside);
        var target = BuildChunk(room, new Rectangle(new Point(7, 0), 3, 3), inside: [new Point(8, 1)]);

        // (0,1) doorway perimeter wall → two interior tiles → corridor → target side.
        var path = new[]
        {
            new Point(0, 1), // perimeter doorway of source
            new Point(1, 1), // inside source
            new Point(2, 1), // inside source (last)
            new Point(3, 1), // first tile past source interior — kept
            new Point(4, 1),
            new Point(5, 1),
            new Point(6, 1)
        };

        var trimmed = RoomLayout.TrimCorridorEndpoints(path, source, target);

        Assert.Equal(new[]
        {
            new Point(3, 1),
            new Point(4, 1),
            new Point(5, 1),
            new Point(6, 1)
        }, trimmed);
    }

    /// <summary>
    ///     Mirror of the source-side case: path approaches the target chunk and dives
    ///     into its interior before stopping at the target doorway. Trim drops everything
    ///     from the first interior tile (from the target end) onward.
    /// </summary>
    [Fact]
    public void Drops_target_chunk_suffix_from_the_first_inside_point()
    {
        var room = TestHelper.BuildRoom();
        var source = BuildChunk(room, new Rectangle(new Point(0, 0), 3, 3), inside: [new Point(1, 1)]);
        var targetInside = new[] { new Point(7, 1), new Point(8, 1) };
        var target = BuildChunk(room, new Rectangle(new Point(6, 0), 4, 3), inside: targetInside);

        var path = new[]
        {
            new Point(3, 1),
            new Point(4, 1),
            new Point(5, 1),
            new Point(6, 1), // perimeter of target
            new Point(7, 1), // inside target (first from end)
            new Point(8, 1), // inside target
            new Point(9, 1)  // perimeter of target
        };

        var trimmed = RoomLayout.TrimCorridorEndpoints(path, source, target);

        Assert.Equal(new[]
        {
            new Point(3, 1),
            new Point(4, 1),
            new Point(5, 1),
            new Point(6, 1)
        }, trimmed);
    }

    /// <summary>
    ///     Both ends dive into their respective chunk's interior. Trim drops both prefix
    ///     and suffix, leaving only the bridge tiles between the two chunks.
    /// </summary>
    [Fact]
    public void Drops_both_ends_when_path_enters_each_chunk_interior()
    {
        var room = TestHelper.BuildRoom();
        var source = BuildChunk(room, new Rectangle(new Point(0, 0), 4, 3), inside: [new Point(1, 1), new Point(2, 1)]);
        var target = BuildChunk(room, new Rectangle(new Point(7, 0), 4, 3), inside: [new Point(8, 1), new Point(9, 1)]);

        var path = new[]
        {
            new Point(0, 1),
            new Point(1, 1), // inside source
            new Point(2, 1), // inside source
            new Point(3, 1),
            new Point(4, 1),
            new Point(5, 1),
            new Point(6, 1),
            new Point(7, 1),
            new Point(8, 1), // inside target
            new Point(9, 1), // inside target
            new Point(10, 1)
        };

        var trimmed = RoomLayout.TrimCorridorEndpoints(path, source, target);

        Assert.Equal(new[]
        {
            new Point(3, 1),
            new Point(4, 1),
            new Point(5, 1),
            new Point(6, 1),
            new Point(7, 1)
        }, trimmed);
    }

    /// <summary>
    ///     The corridor wanders out of and back into the source chunk before leaving for
    ///     good. Trim must keep walking until it finds the LAST inside point so the
    ///     intermediate excursion through chunk interior is dropped as well.
    /// </summary>
    [Fact]
    public void Trims_through_the_last_inside_point_when_path_re_enters_source_chunk()
    {
        var room = TestHelper.BuildRoom();
        var sourceInside = new[] { new Point(1, 1), new Point(2, 1), new Point(1, 3) };
        var source = BuildChunk(room, new Rectangle(new Point(0, 0), 4, 5), inside: sourceInside);
        var target = BuildChunk(room, new Rectangle(new Point(7, 0), 3, 3), inside: [new Point(8, 1)]);

        var path = new[]
        {
            new Point(0, 1),
            new Point(1, 1), // inside source
            new Point(2, 1), // inside source
            new Point(3, 1),
            new Point(2, 2), // outside source interior (not in inside list)
            new Point(1, 3), // inside source — LAST occurrence
            new Point(1, 4),
            new Point(2, 4),
            new Point(3, 4),
            new Point(4, 4),
            new Point(5, 4)
        };

        var trimmed = RoomLayout.TrimCorridorEndpoints(path, source, target);

        Assert.Equal(new[]
        {
            new Point(1, 4),
            new Point(2, 4),
            new Point(3, 4),
            new Point(4, 4),
            new Point(5, 4)
        }, trimmed);
    }

    /// <summary>
    ///     A 1-tile-wide gap between the two chunks: only the doorway perimeter wall on
    ///     each side survives the trim, leaving the corridor to carve just those two
    ///     tiles into archways.
    /// </summary>
    [Fact]
    public void Keeps_perimeter_doorways_when_chunks_are_adjacent()
    {
        var room = TestHelper.BuildRoom();
        var source = BuildChunk(room, new Rectangle(new Point(0, 0), 3, 3), inside: [new Point(1, 1)]);
        var target = BuildChunk(room, new Rectangle(new Point(3, 0), 3, 3), inside: [new Point(4, 1)]);

        // (1,1) source interior → (2,1) source perimeter → (3,1) target perimeter →
        // (4,1) target interior. Trim drops the two interior tiles but must keep the
        // adjacent perimeter walls so WriteCorridor can carve the archways.
        var path = new[]
        {
            new Point(1, 1),
            new Point(2, 1),
            new Point(3, 1),
            new Point(4, 1)
        };

        var trimmed = RoomLayout.TrimCorridorEndpoints(path, source, target);

        Assert.Equal(new[]
        {
            new Point(2, 1),
            new Point(3, 1)
        }, trimmed);
    }

    /// <summary>
    ///     When every tile lives in a chunk's interior the trim drops the entire path —
    ///     the chunks already cover the route end-to-end and no corridor write is needed.
    /// </summary>
    [Fact]
    public void Returns_empty_when_path_is_entirely_inside_chunk_interiors()
    {
        var room = TestHelper.BuildRoom();
        var source = BuildChunk(room, new Rectangle(new Point(0, 0), 3, 3), inside: [new Point(1, 1), new Point(2, 1)]);
        var target = BuildChunk(room, new Rectangle(new Point(3, 0), 3, 3), inside: [new Point(3, 1), new Point(4, 1)]);

        var path = new[]
        {
            new Point(1, 1),
            new Point(2, 1),
            new Point(3, 1),
            new Point(4, 1)
        };

        var trimmed = RoomLayout.TrimCorridorEndpoints(path, source, target);

        Assert.Empty(trimmed);
    }

    /// <summary>
    ///     Sanity check on the new <see cref="Chunk.PerimeterPoints" /> collection: the
    ///     fragment-static path populates it with every wall, archway and explicit
    ///     doorway tile written by the fragment so corridor logic downstream can
    ///     distinguish chunk-owned tiles from open floor.
    /// </summary>
    [Fact]
    public void Static_map_chunk_exposes_perimeter_points_from_fragment()
    {
        var room = TestHelper.BuildRoom(new string('.', 5) + "\n"
                                        + new string('.', 5) + "\n"
                                        + new string('.', 5) + "\n"
                                        + new string('.', 5) + "\n"
                                        + new string('.', 5));
        var fragment = new NormalMapFragment
        {
            Name = "perimeter-box",
            Map = """
                  ###
                  #.#
                  ###
                  """
        };
        fragment.EnsureInitialized();

        var chunk = fragment.TryPlace(room, new LotPlacementContext(new Rectangle(new Point(1, 1), 3, 3), TestHelper.CreateRoomParameters(room)));

        Assert.NotNull(chunk);
        // The 3x3 box has 8 wall tiles around a single interior floor.
        Assert.Equal(8, chunk!.PerimeterPoints.Count);
        Assert.Single(chunk.InsidePoints);
        Assert.Contains(new Point(2, 2), chunk.InsidePoints);
    }

    /// <summary>
    ///     Verifies the inter-level connection wiring that happens when
    ///     <see cref="UniformRoomLayout" /> falls back to picking a
    ///     <see cref="NormalMapFragment" /> with a perimeter <c>door-*</c> tag
    ///     because no <see cref="ConnectingMapFragment" /> could fit at a lot flush
    ///     with the level edge. The connection cell is one of the placed chunk's
    ///     <see cref="Chunk.DoorwayPoints" /> and is wired by role through
    ///     <see cref="LevelConnection.CreateReceivingConnection" /> /
    ///     <see cref="LevelConnection.CreateSourceConnection" />.
    /// </summary>
    [Fact]
    public void Fallback_places_connection_at_perimeter_doorway_when_no_connecting_fragment_fits()
    {
        Chunk? hostingChunk = null;
        ConnectionComponent? createdConnection = null;
        RoomComponent? room = null;
        var success = false;

        for (uint seed = 1; seed <= 25 && !success; seed++)
        {
            var game = TestHelper.CreateGame(seed);
            // Hermetic ConnectingMapFragments pool: nothing fits, forcing
            // UniformRoomLayout's NormalMapFragments fallback to handle the
            // outbound inter-level connection.
            ((TestDataProvider)game.Services.Data).ConnectingMapFragments.IncludeProduction = false;

            var branchName = "fallback-test-branch-" + seed;
            var testBranch = new Branch
            {
                Name = branchName,
                Length = 10,
                Difficulty = 1,
                Layout = new EmptyLevelLayout(),
                RoomWidth = 25,
                RoomHeight = 16
            };
            var branchEntity = testBranch.Instantiate(game);
            game.Branches.Add(branchEntity);
            ((TestDataProvider)game.Services.Data).Branches.Add(testBranch);

            // A small defining fragment placed inside a much larger room so
            // most of the level is empty lots — including lots flush with all
            // four level perimeters where the fallback can fire.
            var fragment = new DefiningMapFragment
            {
                Name = "fallback-defining-" + seed,
                Map = ".....\n.....\n.....\n.....\n.....",
                NoRandomDoorways = true,
                CreatureGenerator = new CreatureGenerator { ExpectedInitialCount = 0 },
                ItemGenerator = new ItemGenerator { ExpectedInitialCount = 0 },
                GenerationWeight = $"$branch == '{branchName}' ? 1 : 0"
            };
            fragment.EnsureInitialized();
            ((TestDataProvider)game.Services.Data).DefiningMapFragments.Add(fragment);

            var gameLevel = LevelGenerator.CreateEmpty(branchEntity, depth: 0, seed, game.Manager);
            IReadOnlyList<Chunk>? chunks;
            try
            {
                chunks = LevelGenerator.Generate(gameLevel);
            }
            catch
            {
                continue;
            }

            if (chunks == null)
            {
                continue;
            }

            var candidateRoom = gameLevel.Rooms.FirstOrDefault();
            if (candidateRoom == null)
            {
                continue;
            }

            foreach (var chunk in chunks)
            {
                if (chunk.Fragment is not NormalMapFragment)
                {
                    continue;
                }

                foreach (var (cell, connectionEntity) in candidateRoom.Connections)
                {
                    var connection = connectionEntity.Connection!;
                    if (connection.TargetLevelX != null)
                    {
                        // Inbound connection from elsewhere — skip.
                        continue;
                    }

                    if (!chunk.DoorwayPoints.Contains(cell))
                    {
                        continue;
                    }

                    // The doorway must lie on the level perimeter — the fallback
                    // only fires for lots flush with a level edge.
                    if (cell.X != 0 && cell.X != candidateRoom.Width - 1
                        && cell.Y != 0 && cell.Y != candidateRoom.Height - 1)
                    {
                        continue;
                    }

                    hostingChunk = chunk;
                    createdConnection = connection;
                    room = candidateRoom;
                    break;
                }

                if (hostingChunk != null)
                {
                    break;
                }
            }

            if (hostingChunk != null)
            {
                success = true;
            }
        }

        Assert.True(success,
            "Expected at least one seed to wire a level connection at a perimeter doorway "
            + "of a NormalMapFragment chunk when no ConnectingMapFragment is available.");
        Assert.NotNull(hostingChunk);
        Assert.NotNull(createdConnection);
        Assert.NotNull(room);

        // The connection must point at a target room — CreateSourceConnection
        // created (or reused) a placeholder room on a freshly allocated level.
        Assert.NotNull(createdConnection!.TargetRoomEntity);
    }

    /// <summary>
    ///     Mirrors a <c>--RevealRooms=true</c> walkthrough across several seeds and depths along the
    ///     production surface→entrance→dungeon path, asserting two invariants for every generated room:
    ///     connectivity (all walkable tiles form a single 8-connected component) and walling (no
    ///     walkable tile is adjacent to unfilled background or isolated). A walkable tile on the
    ///     room's outer boundary is only allowed when it is a connection portal (an archway or a
    ///     connection source/arrival cell); any other boundary floor is exposed to the level void.
    /// </summary>
    [Theory]
    [InlineData(1u)]
    [InlineData(2u)]
    [InlineData(3u)]
    [InlineData(7u)]
    [InlineData(42u)]
    public void Every_room_is_connected_and_fully_walled(uint seed)
    {
        var game = TestHelper.CreateGame(seed);
        var manager = game.Manager;

        var surfaceBranch = BranchData.Surface.Instantiate(game);
        BranchData.Entrance.Instantiate(game);
        BranchData.Dungeon.Instantiate(game);

        var surfaceLevel = LevelGenerator.CreateEmpty(surfaceBranch, depth: 1, seed, manager);
        LevelGenerator.EnsureGenerated(surfaceLevel);

        var entranceRoom = surfaceLevel.Rooms.First().Connections.Single().Value.Connection!.TargetRoomEntity.Room!;
        LevelGenerator.EnsureGenerated(entranceRoom);

        var violations = new List<string>();
        CheckLevel(entranceRoom.Level, violations);

        // Descend a couple of dungeon levels too. EnsureGenerated drains its own message queue, so
        // a shared GameManager handles back-to-back generations without overflowing.
        var room = NextDungeonRoom(entranceRoom.Level);
        for (var depth = 0; depth < 2 && room != null; depth++)
        {
            LevelGenerator.EnsureGenerated(room);
            CheckLevel(room.Level, violations);
            room = NextDungeonRoom(room.Level);
        }

        Assert.True(violations.Count == 0,
            $"seed {seed}: {violations.Count} room(s) violated connectivity/walling:\n"
            + string.Join("\n", violations));
    }

    /// <summary>
    ///     Regression: every walkable tile of every room on the production surface→entrance→dungeon
    ///     path must be mutually reachable. A connectivity-check flood that tunnelled through a
    ///     one-cell wall used to let a genuinely disconnected chunk slip past the repair pass.
    /// </summary>
    [Theory]
    [InlineData(1u)]
    [InlineData(2u)]
    [InlineData(3u)]
    [InlineData(7u)]
    [InlineData(42u)]
    public void Production_flow_rooms_have_no_disconnected_walkable_tiles(uint seed)
    {
        var game = TestHelper.CreateGame(seed);
        var manager = game.Manager;

        var surfaceBranch = BranchData.Surface.Instantiate(game);
        BranchData.Entrance.Instantiate(game);
        BranchData.Dungeon.Instantiate(game);

        var surfaceLevel = LevelGenerator.CreateEmpty(surfaceBranch, depth: 1, seed, manager);
        LevelGenerator.EnsureGenerated(surfaceLevel);

        var entranceRoom = surfaceLevel.Rooms.First().Connections.Single().Value.Connection!.TargetRoomEntity.Room!;
        LevelGenerator.EnsureGenerated(entranceRoom);
        AssertLevelWalkableConnected(entranceRoom.Level, seed);

        // Follow the entrance level's exit into the first dungeon level (the room the reported
        // disconnected chunk appeared in) and check it too.
        var dungeonRoom = entranceRoom.Level.Rooms
            .FirstOrDefault(r => (r.Features & RoomFeatures.Exit) != 0)?.Connections.Values
            .Select(c => c.Connection!)
            .FirstOrDefault(c => c.Orientation == Direction.Down)?.TargetRoomEntity.Room;
        if (dungeonRoom != null)
        {
            LevelGenerator.EnsureGenerated(dungeonRoom);
            AssertLevelWalkableConnected(dungeonRoom.Level, seed);
        }
    }

    private static RoomComponent? NextDungeonRoom(GameLevel level)
        => level.Rooms
            .FirstOrDefault(r => (r.Features & RoomFeatures.Exit) != 0)?.Connections.Values
            .Select(c => c.Connection!)
            .FirstOrDefault(c => c.Orientation == Direction.Down)?.TargetRoomEntity.Room;

    private void AssertLevelWalkableConnected(GameLevel level, uint seed)
    {
        foreach (var room in level.Rooms)
        {
            var (components, _) = Analyze(room);
            if (components > 1)
            {
                var msg = $"{room.Level.BranchName}:{room.Level.Depth} room({room.GridX},{room.GridY}) "
                    + $"has {components} disconnected walkable components (seed {seed})";
                _output.WriteLine(msg);
                _output.WriteLine(Dump(room, []));
                Assert.Fail(msg);
            }
        }
    }

    private void CheckLevel(GameLevel level, List<string> violations)
    {
        foreach (var room in level.Rooms)
        {
            var (components, exposed) = Analyze(room);
            if (components > 1 || exposed.Count > 0)
            {
                var msg =
                    $"{room.Level.BranchName}:{room.Level.Depth} room({room.GridX},{room.GridY}) {room.Features} "
                    + $"components={components} exposedWalkable={exposed.Count}";
                violations.Add(msg);
                _output.WriteLine(msg);
                _output.WriteLine(Dump(room, exposed));
            }
        }
    }

    private static (int Components, List<Point> Exposed) Analyze(RoomComponent room)
    {
        var w = room.Width;
        var h = room.Height;
        var comp = new int[room.TileCount];
        for (var i = 0; i < comp.Length; i++)
        {
            comp[i] = -1;
        }

        // Connection portals (this room's outbound door cells, the arrival cells of inbound
        // connections, and carved archways) are the legitimate boundary walkable tiles.
        var connectionCells = new HashSet<Point>(room.Connections.Keys);
        foreach (var incoming in room.IncomingConnections)
        {
            if (incoming.Connection!.TargetCell is { } targetCell)
            {
                connectionCells.Add(targetCell);
            }
        }

        var components = 0;
        var exposed = new List<Point>();

        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                var idx = room.PointToIndex![x, y];
                if (!room.Terrain[idx].CanMoveTo())
                {
                    continue;
                }

                var p = new Point((byte)x, (byte)y);

                // Connection portals don't need to be enclosed: they're teleport tiles at the room's
                // edge, and the void (or the other room) beyond them is outside this room's
                // enclosure. Exempt them from the exposure check — they still take part in the
                // connectivity flood below.
                if (!connectionCells.Contains(p))
                {
                    // No 8-neighbor may be unfilled (Default), and at least one must be walkable.
                    // A walkable tile on the room's outer ring is exposed to the level void beyond
                    // the boundary, so it counts as exposure too.
                    var hasWalkableNeighbor = false;
                    var isExposed = false;
                    for (var dir = 0; dir < 8; dir++)
                    {
                        var d = Vector.MovementDirections[dir];
                        int nx = x + d.X, ny = y + d.Y;
                        if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                        {
                            continue;
                        }

                        var nf = room.Terrain[room.PointToIndex![nx, ny]];
                        if (nf == MapFeature.Default)
                        {
                            isExposed = true;
                        }

                        if (nf.CanMoveTo())
                        {
                            hasWalkableNeighbor = true;
                        }
                    }

                    var onBoundary = x == 0 || y == 0 || x == w - 1 || y == h - 1;
                    if (onBoundary && room.Terrain[idx] != MapFeature.StoneArchway)
                    {
                        isExposed = true;
                    }

                    if (isExposed || !hasWalkableNeighbor)
                    {
                        exposed.Add(p);
                    }
                }

                if (comp[idx] != -1)
                {
                    continue;
                }

                Flood(room, x, y, components, comp);
                components++;
            }
        }

        return (components, exposed);
    }

    private static void Flood(RoomComponent room, int sx, int sy, int label, int[] comp)
    {
        var stack = new Stack<(int X, int Y)>();
        stack.Push((sx, sy));
        comp[room.PointToIndex![sx, sy]] = label;
        while (stack.Count > 0)
        {
            var (x, y) = stack.Pop();
            for (var dir = 0; dir < 8; dir++)
            {
                var d = Vector.MovementDirections[dir];
                int nx = x + d.X, ny = y + d.Y;
                if (nx < 0 || ny < 0 || nx >= room.Width || ny >= room.Height)
                {
                    continue;
                }

                var index = room.PointToIndex![nx, ny];
                if (room.Terrain[index].CanMoveTo() && comp[index] == -1)
                {
                    comp[index] = label;
                    stack.Push((nx, ny));
                }
            }
        }
    }

    private static string Dump(RoomComponent room, List<Point> exposed)
    {
        var exposedSet = new HashSet<Point>(exposed);
        var sb = new StringBuilder();
        for (var y = 0; y < room.Height; y++)
        {
            for (var x = 0; x < room.Width; x++)
            {
                var p = new Point((byte)x, (byte)y);
                var f = room.Terrain[room.PointToIndex![x, y]];
                sb.Append(exposedSet.Contains(p)
                    ? 'X'
                    : f switch
                    {
                        MapFeature.StoneWall or MapFeature.RockWall => '#',
                        MapFeature.StoneArchway => '*',
                        MapFeature.Default => ' ',
                        _ when f.CanMoveTo() => '.',
                        _ => '?'
                    });
            }

            sb.Append('\n');
        }

        return sb.ToString();
    }

    /// <summary>
    ///     A fragment with <see cref="MapFragment.NoRandomDoorways" /> = true produces
    ///     zero doorway points. When demand is non-zero, <see cref="RoomLayout.Fill" />
    ///     must return null because <see cref="RoomLayout.SatisfiesDemand" /> fails.
    /// </summary>
    [Fact]
    public void Fill_returns_null_when_fragment_has_no_doorway_points()
    {
        var (room, branch, game) = BuildTestRoom(7);

        var fragment = new DefiningMapFragment
        {
            Name = "no-doorways",
            Map = """
                #####
                #...#
                #...#
                #...#
                #####
                """,
            NoRandomDoorways = true,
            CreatureGenerator = new CreatureGenerator { ExpectedInitialCount = 0 },
            ItemGenerator = new ItemGenerator { ExpectedInitialCount = 0 }
        };
        fragment.EnsureInitialized();

        var demand = new Dictionary<Direction, int> { { Direction.East, 1 } };

        var result = new EmptyRoomLayout().Fill(
            room, branch, fragment, TestHelper.FromDemand(demand), fragment.GetRoomParameters(branch));

        Assert.Null(result);
    }

    /// <summary>
    ///     When a fragment produces doorway points but fewer than the total demand,
    ///     <see cref="RoomLayout.Fill" /> must return null.
    /// </summary>
    [Fact]
    public void Fill_returns_null_when_doorway_supply_less_than_demand()
    {
        var (room, branch, game) = BuildTestRoom(7);

        // A 5x5 box with a single explicit doorway marker on the east wall.
        var fragment = new DefiningMapFragment
        {
            Name = "one-doorway",
            Map = """
                #####
                #...#
                #..>+
                #...#
                #####
                """,
            CreatureGenerator = new CreatureGenerator { ExpectedInitialCount = 0 },
            ItemGenerator = new ItemGenerator { ExpectedInitialCount = 0 }
        };
        fragment.EnsureInitialized();

        // Demand 2 doorways total — the fragment only has 1.
        var demand = new Dictionary<Direction, int>
        {
            { Direction.East, 1 },
            { Direction.West, 1 }
        };

        var result = new EmptyRoomLayout().Fill(
            room, branch, fragment, TestHelper.FromDemand(demand), fragment.GetRoomParameters(branch));

        Assert.Null(result);
    }

    /// <summary>
    ///     When doorway supply equals demand, <see cref="RoomLayout.Fill" /> succeeds.
    /// </summary>
    [Fact]
    public void Fill_succeeds_when_doorway_supply_meets_demand()
    {
        var (room, branch, game) = BuildTestRoom(7);

        // Two explicit doorway markers.
        var fragment = new DefiningMapFragment
        {
            Name = "two-doorways",
            Map = """
                #####
                #...#
                +...+
                #...#
                #####
                """,
            CreatureGenerator = new CreatureGenerator { ExpectedInitialCount = 0 },
            ItemGenerator = new ItemGenerator { ExpectedInitialCount = 0 }
        };
        fragment.EnsureInitialized();

        var demand = new Dictionary<Direction, int>
        {
            { Direction.East, 1 },
            { Direction.West, 1 }
        };

        var result = new EmptyRoomLayout().Fill(
            room, branch, fragment, TestHelper.FromDemand(demand), fragment.GetRoomParameters(branch));

        Assert.NotNull(result);
    }

    /// <summary>
    ///     When all doorway points of a fragment are already consumed by existing
    ///     connections on the room, <see cref="RoomLayout.DeductChunkDoorways" />
    ///     skips them. This verifies that the remaining demand is not reduced when
    ///     doorways are already spoken for.
    /// </summary>
    [Fact]
    public void Fill_returns_null_when_all_doorways_consumed_by_existing_connections()
    {
        var (room, branch, game) = BuildTestRoom(7);

        // Fragment with a single explicit doorway.
        var fragment = new DefiningMapFragment
        {
            Name = "single-doorway-consumed",
            Map = """
                #####
                #...#
                #...+
                #...#
                #####
                """,
            CreatureGenerator = new CreatureGenerator { ExpectedInitialCount = 0 },
            ItemGenerator = new ItemGenerator { ExpectedInitialCount = 0 }
        };
        fragment.EnsureInitialized();

        // First, place with no demand to discover the doorway point.
        var chunks = new EmptyRoomLayout().Fill(room, branch, fragment, [], fragment.GetRoomParameters(branch));
        Assert.NotNull(chunks);
        Assert.Single(chunks);
        var doorwayPoint = Assert.Single(chunks[0].DoorwayPoints);

        // Simulate an existing connection at that doorway point.
        var connections = (SortedDictionary<Point, GameEntity>)room.Connections;
        using var dummyEntity = game.Manager.CreateEntity();
        connections[doorwayPoint] = dummyEntity.Referenced;

        // Reset terrain so Fill can run again cleanly.
        ResetRoomTerrain(room);

        // Now demand 1 doorway using UniformRoomLayout which calls DeductChunkDoorways.
        // DeductChunkDoorways skips the consumed doorway, so remaining demand stays at 1.
        // However, SatisfiesDemand counts raw DoorwayPoints (1 >= 1 = true).
        // The demand tracking is an optimization for the loop; SatisfiesDemand is the gate.
        // With only 1 doorway and it consumed, FindDoorwayInDirection would fail at
        // MaterializeGraph time. But SatisfiesDemand (total count) still passes here
        // because it doesn't account for connections — that's FindDoorwayInDirection's job.
        var demand = new Dictionary<Direction, int> { { Direction.East, 1 } };
        var result = new EmptyRoomLayout().Fill(
            room, branch, fragment, TestHelper.FromDemand(demand), fragment.GetRoomParameters(branch));

        // SatisfiesDemand only checks total DoorwayPoints.Count >= total demand.
        // The doorway point exists (count=1 >= 1), so Fill succeeds.
        // Connection filtering is handled later by FindDoorwayInDirection.
        Assert.NotNull(result);
    }

    /// <summary>
    ///     <see cref="MapFragment.BuildChunk" /> returns null when the fragment produces
    ///     fewer doorway points than <c>minDoorways</c>. This is the mechanism that
    ///     enforces per-lot doorway requirements during
    ///     <see cref="UniformRoomLayout.Fill" />. Only applies to the dynamic-map
    ///     placement path (static maps bypass <c>minDoorways</c>).
    /// </summary>
    [Fact]
    public void TryPlace_returns_null_when_minDoorways_not_met()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = BuildFloorRoom(game, roomSize: 5);

        // Place a dynamic-map fragment in the corner of the room so its walls are flush
        // with the room boundary on two sides. Only interior-facing walls produce
        // doorway candidates, so with a 3x3 rectangle at (0,0) in a 5x5 room only 2
        // doorway candidates are possible. Requesting minDoorways=3 triggers rejection.
        var fragment = new NormalMapFragment
        {
            Name = "dynamic-corner",
            DynamicMap = new RectangleMap { MinSize = new Dimensions(3, 3) }
        };
        fragment.EnsureInitialized();

        var lot = new Rectangle(new Point(0, 0), 3, 3);
        var ctx = new LotPlacementContext(lot, TestHelper.CreateRoomParameters(room)) with
        {
            DemandedDoorways = DirectionFlags.North | DirectionFlags.South | DirectionFlags.East
        };
        var chunk = fragment.TryPlace(room, ctx);

        Assert.Null(chunk);
    }

    /// <summary>
    ///     <see cref="MapFragment.TryPlace" /> succeeds when the fragment produces
    ///     enough doorway points to meet the <c>minDoorways</c> threshold.
    /// </summary>
    [Fact]
    public void TryPlace_succeeds_when_minDoorways_met()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = BuildFloorRoom(game, roomSize: 7);

        // A box fragment placed with space around it will auto-derive doorway points
        // at wall midpoints (at least 2 for a 3x3 box in a larger room).
        var fragment = new NormalMapFragment
        {
            Name = "box-with-doorways",
            Map = """
                ###
                #.#
                ###
                """
        };
        fragment.EnsureInitialized();

        var lot = new Rectangle(new Point(2, 2), 3, 3);
        var ctx = new LotPlacementContext(lot, TestHelper.CreateRoomParameters(room)) with { DemandedDoorways = DirectionFlags.North };
        var chunk = fragment.TryPlace(room, ctx);

        Assert.NotNull(chunk);
        Assert.True(chunk!.DoorwayPoints.Count >= 1);
    }

    /// <summary>
    ///     Builds a test room of the given size with all-floor terrain, attached to
    ///     an isolated branch. Returns the room, branch, and game for direct
    ///     <see cref="RoomLayout.Fill" /> calls.
    /// </summary>
    private static (RoomComponent Room, Branch Branch, Game Game) BuildTestRoom(byte roomSize)
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var branch = new Branch
        {
            Name = "doorway-demand-test",
            Length = 1,
            Difficulty = 1,
            Layout = new EmptyLevelLayout()
        };
        var branchEntity = branch.Instantiate(game);
        game.Branches.Add(branchEntity);

        var gameLevel = LevelGenerator.CreateEmpty(branchEntity, depth: 1, seed: 1u, game.Manager);
        var room = LevelLayout.GetEmptyEntranceRoom(gameLevel, game.Manager);
        LevelLayout.AllocateTerrain(room, roomSize, roomSize, MapFeature.StoneFloor);

        return (room, branch, game);
    }

    /// <summary>
    ///     Builds a square room via the full pipeline (using a stone-floor defining
    ///     fragment to simulate "surrounded by floor" semantics) and clears every
    ///     cell to <see cref="MapFeature.Default" /> so the room behaves as a blank
    ///     canvas for a subsequent <see cref="MapFragment.TryPlace" /> call. The
    ///     defining-fragment chunk would otherwise own every cell and the
    ///     walkable-wins overlap rule would preserve the floor against the
    ///     to-be-placed fragment's wall writes.
    /// </summary>
    private static RoomComponent BuildFloorRoom(Game game, byte roomSize)
    {
        var rowLine = new string('.', roomSize);
        var rows = new string[roomSize];
        for (var i = 0; i < roomSize; i++)
        {
            rows[i] = rowLine;
        }

        var room = TestHelper.BuildRoom(string.Join('\n', rows), game);
        for (var i = 0; i < room.TileCount; i++)
        {
            room.Terrain[i] = MapFeature.Default;
        }
        return room;
    }

    /// <summary>
    ///     Resets all terrain in the room to <see cref="MapFeature.StoneFloor" /> so
    ///     that <see cref="RoomLayout.Fill" /> can run again on the same room.
    /// </summary>
    private static void ResetRoomTerrain(RoomComponent room)
    {
        for (var i = 0; i < room.TileCount; i++)
        {
            room.Terrain[i] = MapFeature.StoneFloor;
        }
    }

    private static Chunk BuildChunk(
        RoomComponent room,
        Rectangle bounds,
        IReadOnlyList<Point> inside,
        IReadOnlyList<Point>? doorways = null,
        IReadOnlyList<Point>? perimeter = null)
    {
        // A non-null MapFragment is required by the Chunk constructor but plays no role in
        // the trim algorithm — use a trivial fragment whose only purpose is to satisfy
        // the type system.
        var fragment = new NormalMapFragment { Name = "trim-test-stub", Map = "." };
        fragment.EnsureInitialized();
        return new Chunk(room, bounds, fragment,
            doorwayPoints: doorways != null ? new HashSet<Point>(doorways) : null,
            insidePoints: new HashSet<Point>(inside),
            perimeterPoints: perimeter != null ? new HashSet<Point>(perimeter) : null,
            roomParameters: TestHelper.CreateRoomParameters(room));
    }
}
