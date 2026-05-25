using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

/// <summary>
///     Tests for <see cref="DenseRoomLayout" /> covering tuned defaults and the
///     <see cref="DenseRoomLayout.TryShiftToMerge" /> hook that performs the
///     wall-merge shift. The hook is exercised directly through a test subclass; an
///     end-to-end test that drives a full layout pass and asserts on the resulting chunk
///     bounding rectangles will land alongside the static fragment set.
/// </summary>
public class DenseRoomLayoutTest
{
    [Fact]
    public void Defaults_pack_more_densely_than_uniform()
    {
        var dense = new DenseRoomLayout();
        var uniform = new UniformRoomLayout();

        Assert.True(dense.Coverage > uniform.Coverage,
            "DenseRoomLayout should target higher coverage than UniformRoomLayout");
        Assert.True(dense.MaxChunkCount > uniform.MaxChunkCount,
            "DenseRoomLayout should permit more chunks than UniformRoomLayout");
        Assert.True(
            dense.MinLotSize.Width <= uniform.MinLotSize.Width
            && dense.MinLotSize.Height <= uniform.MinLotSize.Height,
            "DenseRoomLayout should accept lots no larger than UniformRoomLayout");
    }

    [Fact]
    public void Is_assignable_as_a_branch_RoomLayout()
    {
        var branch = new Branch { RoomLayout = new DenseRoomLayout() };

        Assert.IsType<DenseRoomLayout>(branch.RoomLayout);
    }

    [Fact]
    public void TryShiftToMerge_shifts_lot_one_tile_west_toward_an_adjacent_neighbour()
    {
        var layout = new TestableDenseRoomLayout();
        var room = TestHelper.BuildRoom();
        var anchor = BuildChunk(
            room, new Rectangle(new Point(0, 0), 3, 3), new TestNormalMapFragment());

        // Lot abuts the anchor on its West edge (placed.BottomRight.X = 2, lot.TopLeft.X = 3).
        var lot = new Rectangle(new Point(3, 0), 3, 3);

        var shifted = layout.InvokeTryShiftToMerge(lot, Direction.West, [anchor], out var result);

        Assert.True(shifted);
        Assert.Equal(new Point(2, 0), result.TopLeft);
        Assert.Equal(new Point(4, 2), result.BottomRight);
    }

    [Fact]
    public void TryShiftToMerge_rejects_when_neighbour_has_NoOverwrite()
    {
        var layout = new TestableDenseRoomLayout();
        var room = TestHelper.BuildRoom();
        var anchor = BuildChunk(
            room,
            new Rectangle(new Point(0, 0), 3, 3),
            new TestNormalMapFragment { NoOverwrite = true });

        var lot = new Rectangle(new Point(3, 0), 3, 3);

        var shifted = layout.InvokeTryShiftToMerge(lot, Direction.West, [anchor], out _);

        Assert.False(shifted);
    }

    [Fact]
    public void TryShiftToMerge_rejects_when_lot_is_not_adjacent_to_any_chunk()
    {
        var layout = new TestableDenseRoomLayout();
        var room = TestHelper.BuildRoom();
        var anchor = BuildChunk(
            room, new Rectangle(new Point(0, 0), 3, 3), new TestNormalMapFragment());

        // Gap of one column between anchor.BottomRight.X = 2 and lot.TopLeft.X = 4.
        var lot = new Rectangle(new Point(4, 0), 3, 3);

        var shifted = layout.InvokeTryShiftToMerge(lot, Direction.West, [anchor], out _);

        Assert.False(shifted);
    }

    [Fact]
    public void TryShiftToMerge_allows_shift_when_neighbour_has_doorway_on_facing_perimeter()
    {
        var layout = new TestableDenseRoomLayout();
        var room = TestHelper.BuildRoom();
        // Doorway sits on the anchor's East perimeter — the column a westbound shift would overlap.
        // The shift is now allowed: the candidate's wall write will preserve the doorway.
        var anchor = BuildChunk(
            room,
            new Rectangle(new Point(0, 0), 3, 3),
            new TestNormalMapFragment(),
            doorwayPoints: [new Point(2, 1)]);

        var lot = new Rectangle(new Point(3, 0), 3, 3);

        var shifted = layout.InvokeTryShiftToMerge(lot, Direction.West, [anchor], out var result);

        Assert.True(shifted);
        Assert.Equal(new Point(2, 0), result.TopLeft);
    }

    [Fact]
    public void Write_preserves_important_cell_when_wall_would_overlap()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(game: game);

        // Pre-write a Pool (a non-floor, non-wall feature) at (2, 1) to simulate the
        // important content a neighbour's perimeter contributed, and register the
        // owning chunk so the walkable-wins rule treats the cell as authored
        // content rather than freely-overwritable background fill.
        var poolPoint = new Point(2, 1);
        TerrainSystem.SetTerrain(MapFeature.Pool, poolPoint, room);
        var roomParameters = TestHelper.CreateRoomParameters(room);
        _ = new Chunk(
            room,
            new Rectangle(poolPoint, 1, 1),
            new TestNormalMapFragment { Name = "pool-owner" },
            doorwayPoints: null,
            insidePoints: new HashSet<Point> { poolPoint },
            perimeterPoints: null,
            roomParameters: roomParameters);

        // A 3x3 wall box with a doorway on the left so it satisfies fragment-placement
        // preconditions. Its right column lands on x=2 — the perimeter wall at (2, 1)
        // would overwrite the pre-written pool, but Write must preserve it.
        var fragment = new NormalMapFragment
        {
            Name = "wall-box",
            Map = """
                ###
                +.#
                ###
                """
        };
        fragment.EnsureInitialized();

        fragment.TryPlace(
            room,
            new LotPlacementContext(new Rectangle(new Point(0, 0), 3, 3), roomParameters));

        var preservedIndex = room.PointToIndex![2, 1];
        Assert.Equal(MapFeature.Pool, room.Terrain[preservedIndex]);
    }

    private static Chunk BuildChunk(
        RoomComponent room,
        Rectangle bounds,
        MapFragment fragment,
        HashSet<Point>? doorwayPoints = null)
        => new(room, bounds, fragment,
            doorwayPoints: doorwayPoints,
            insidePoints: null,
            perimeterPoints: null,
            roomParameters: TestHelper.CreateRoomParameters(room));

    [Fact]
    public void EnsureConnectivity_carves_corridor_between_disconnected_chunks()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(
            map: """
                ##########
                ##########
                ##########
                ##########
                ##########
                """,
            game: game);

        // Carve two isolated floor pockets separated by walls.
        var leftBounds = new Rectangle(new Point(1, 1), 3, 3);
        var rightBounds = new Rectangle(new Point(6, 1), 3, 3);
        var leftInside = new HashSet<Point>();
        var rightInside = new HashSet<Point>();
        for (var y = 1; y <= 3; y++)
        {
            for (var x = 1; x <= 3; x++)
            {
                var p = new Point((byte)x, (byte)y);
                TerrainSystem.SetTerrain(MapFeature.StoneFloor, p, room);
                leftInside.Add(p);
            }

            for (var x = 6; x <= 8; x++)
            {
                var p = new Point((byte)x, (byte)y);
                TerrainSystem.SetTerrain(MapFeature.StoneFloor, p, room);
                rightInside.Add(p);
            }
        }

        // Outline both pockets with perimeter walls so Connect's pathfinder can pierce them as archways.
        var leftDoorways = new HashSet<Point> { new(3, 2) };
        var rightDoorways = new HashSet<Point> { new(6, 2) };
        var leftFragment = new TestNormalMapFragment { Name = "left-pocket" };
        var rightFragment = new TestNormalMapFragment { Name = "right-pocket" };
        var roomParameters = TestHelper.CreateRoomParameters(room);
        var leftChunk = new Chunk(room, leftBounds, leftFragment, leftDoorways, leftInside, perimeterPoints: null,
            roomParameters: roomParameters);
        var rightChunk = new Chunk(room, rightBounds, rightFragment, rightDoorways, rightInside, perimeterPoints: null,
            roomParameters: roomParameters);

        // Sanity: before EnsureConnectivity the two pockets are flood-fill-isolated.
        Assert.False(IsReachable(room, leftInside.First(), rightInside.First()));

        var layout = new TestableDenseRoomLayout();
        layout.InvokeTryRepairConnectivity(room, [leftChunk, rightChunk]);

        // After the carve at least one walkable path exists between the pockets.
        Assert.True(IsReachable(room, leftInside.First(), rightInside.First()));
    }

    private static bool IsReachable(RoomComponent room, Point from, Point to)
    {
        var visited = new HashSet<Point> { from };
        var queue = new Queue<Point>();
        queue.Enqueue(from);
        var rect = room.BoundingRectangle;
        while (queue.Count > 0)
        {
            var p = queue.Dequeue();
            if (p.Equals(to))
            {
                return true;
            }

            foreach (var (dx, dy) in (ReadOnlySpan<(int, int)>)[(-1, 0), (1, 0), (0, -1), (0, 1)])
            {
                var nx = p.X + dx;
                var ny = p.Y + dy;
                if (nx < rect.TopLeft.X || nx > rect.BottomRight.X || ny < rect.TopLeft.Y || ny > rect.BottomRight.Y)
                {
                    continue;
                }

                var np = new Point((byte)nx, (byte)ny);
                if (!visited.Add(np))
                {
                    continue;
                }

                var idx = room.PointToIndex![nx, ny];
                if (room.Terrain[idx].CanMoveTo())
                {
                    queue.Enqueue(np);
                }
            }
        }

        return false;
    }

    [Fact]
    public void Fill_with_default_room_size_produces_chunks_that_share_a_wall_line()
    {
        // Drives a full DenseRoomLayout pass on a default-sized (25x16) empty room with a
        // dynamic defining fragment. The wall-merge shift in PlaceSurroundingFragments
        // pulls adjacent lots one tile toward an existing chunk so their bounding
        // rectangles share a wall column or row. Assert at least one pair of chunks
        // actually exhibits that 1-tile overlap end-to-end.
        var game = TestHelper.CreateGame(seed: 1u);
        var fragment = new DefiningMapFragment
        {
            Name = "dense-integration-defining",
            NoRandomDoorways = true,
            CreatureGenerator = new CreatureGenerator { ExpectedInitialCount = 0 },
            ItemGenerator = new ItemGenerator { ExpectedInitialCount = 0 }
        };

        var (_, chunks) = TestHelper.BuildRoom(fragment, game, roomLayout: new DenseRoomLayout());

        Assert.NotNull(chunks);
        Assert.True(chunks.Count >= 2,
            $"DenseRoomLayout should place at least 2 chunks in a default-sized room, got {chunks.Count}");

        var sharedWallPair = false;
        for (var i = 0; i < chunks.Count && !sharedWallPair; i++)
        {
            for (var j = i + 1; j < chunks.Count; j++)
            {
                var intersection = chunks[i].BoundingRectangle.Intersection(chunks[j].BoundingRectangle);
                if (intersection is null)
                {
                    continue;
                }

                // A wall-merge shift produces a 1-tile-wide column or 1-tile-tall row of overlap.
                if (intersection.Value.Width == 1 || intersection.Value.Height == 1)
                {
                    sharedWallPair = true;
                    break;
                }
            }
        }

        Assert.True(sharedWallPair,
            "Expected at least one pair of chunks to share a 1-tile wall line (wall-merge shift)");
    }

    private sealed class TestableDenseRoomLayout : DenseRoomLayout
    {
        public bool InvokeTryShiftToMerge(
            Rectangle lot,
            Direction targetEdge,
            IReadOnlyList<Chunk> placedChunks,
            out Rectangle shiftedLot)
            => TryShiftToMerge(lot, targetEdge, placedChunks, out shiftedLot);

        public void InvokeTryRepairConnectivity(RoomComponent room, List<Chunk> chunks)
            => TryRepairConnectivity(room, chunks);
    }
}
