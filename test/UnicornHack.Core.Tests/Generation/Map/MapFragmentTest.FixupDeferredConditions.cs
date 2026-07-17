using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

/// <summary>
///     Direct tests for <see cref="MapFragment.FixupDeferredConditions" /> — previously
///     only exercised through full level generation. Covers the early-exit shortcut, the
///     simple Filled-fill path, the Unfilled/Edge "skip the cell" path, the wall
///     preference, the isolated-pocket connectivity-rescue pass, and the
///     background-sealing pass around walkable condition cells.
/// </summary>
public partial class MapFragmentTest
{
    [Fact]
    public void FixupDeferredConditions_returns_empty_when_no_preserved_conditions()
    {
        var (room, _) = BuildRoom(roomSize: 7, fill: MapFeature.Default);
        var preserved = new Dictionary<Point, EdgeConditionKind[]>();

        var chunks = MapFragment.FixupDeferredConditions(
            room, preserved, room.Level.GenerationRandom, TestRoomParameters(room));

        Assert.Empty(chunks);
    }

    [Fact]
    public void FixupDeferredConditions_fills_cell_with_satisfying_feature_and_emits_chunk()
    {
        var (room, _) = BuildRoom(roomSize: 7, fill: MapFeature.Default);
        var cell = new Point(3, 3);
        var preserved = new Dictionary<Point, EdgeConditionKind[]>
        {
            // StoneFloor is the only feature that satisfies a tile-specific StoneFloor
            // demand, so the fixup must pick it (the wall-preference path doesn't apply
            // because no wall is in the candidate intersection).
            [cell] = [EdgeConditionKind.StoneFloor]
        };
        // Adjacent real-chunk floor so the fixup cell isn't classified as isolated
        // and walled off by the connectivity-rescue pass.
        TerrainSystem.SetTerrain(MapFeature.StoneFloor, new Point(3, 4), room);
        var roomParameters = TestRoomParameters(room);
        roomParameters[3, 4] = new TestChunkStub(room);

        var chunks = MapFragment.FixupDeferredConditions(
            room, preserved, room.Level.GenerationRandom, roomParameters);

        Assert.Equal(MapFeature.StoneFloor, room.Terrain[room.PointToIndex![cell.X, cell.Y]]);
        // The fixup emits the walkable fill (plus separate perimeter chunks for the walls
        // it seals around it); the fill cell is owned by one of the emitted chunks.
        Assert.Contains(chunks, c => c.InsidePoints.Contains(cell));
    }

    [Fact]
    public void FixupDeferredConditions_skips_cell_when_Unfilled_condition_present()
    {
        var (room, _) = BuildRoom(roomSize: 7, fill: MapFeature.Default);
        var cell = new Point(3, 3);
        var preserved = new Dictionary<Point, EdgeConditionKind[]>
        {
            // Unfilled / Edge demand the cell stay Default. Fixup must skip it.
            [cell] = [EdgeConditionKind.Unfilled]
        };

        var chunks = MapFragment.FixupDeferredConditions(
            room, preserved, room.Level.GenerationRandom, TestRoomParameters(room));

        Assert.Equal(MapFeature.Default, room.Terrain[room.PointToIndex![cell.X, cell.Y]]);
        Assert.Empty(chunks);
    }

    [Fact]
    public void FixupDeferredConditions_prefers_wall_when_candidate_intersection_allows_it()
    {
        var (room, _) = BuildRoom(roomSize: 7, fill: MapFeature.Default);
        var cell = new Point(3, 3);
        var preserved = new Dictionary<Point, EdgeConditionKind[]>
        {
            // Filled accepts any non-Default feature; PickPreferredFixupFeature should
            // pick the branch's default wall (RockWall) over a neighbor feature.
            [cell] = [EdgeConditionKind.Filled]
        };

        var chunks = MapFragment.FixupDeferredConditions(
            room, preserved, room.Level.GenerationRandom, TestRoomParameters(room));

        Assert.Equal(MapFeature.RockWall, room.Terrain[room.PointToIndex![cell.X, cell.Y]]);
        // A pure-wall fixup cell is a perimeter cell, not inside, but it still produces
        // a chunk so the level-generation pipeline can attribute it.
        Assert.Single(chunks);
        Assert.Contains(cell, chunks[0].PerimeterPoints);
    }

    [Fact]
    public void FixupDeferredConditions_walls_off_isolated_walkable_pocket()
    {
        // A StoneFloor fixup cell with no adjacent real-chunk floor is unreachable
        // by the corridor system (deferred-fill chunks author no doorways). The
        // ConvertIsolatedFixupCellsToWalls rescue pass must demote it to the default wall.
        var (room, _) = BuildRoom(roomSize: 7, fill: MapFeature.Default);
        var cell = new Point(3, 3);
        var preserved = new Dictionary<Point, EdgeConditionKind[]>
        {
            [cell] = [EdgeConditionKind.StoneFloor]
        };

        var chunks = MapFragment.FixupDeferredConditions(
            room, preserved, room.Level.GenerationRandom, TestRoomParameters(room));

        Assert.Equal(MapFeature.RockWall, room.Terrain[room.PointToIndex![cell.X, cell.Y]]);
        // Chunk grouping still owns the cell; the wall demotion happens after fill but
        // before grouping cares which cells are inside vs perimeter.
        Assert.Single(chunks);
        Assert.Contains(cell, chunks[0].PerimeterPoints);
    }

    [Fact]
    public void FixupDeferredConditions_skips_cell_already_overwritten_by_later_chunk()
    {
        var (room, _) = BuildRoom(roomSize: 7, fill: MapFeature.Default);
        var cell = new Point(3, 3);
        // Simulate a later chunk having landed on the cell (terrain no longer = Default).
        TerrainSystem.SetTerrain(MapFeature.StoneArchway, cell, room);
        var preserved = new Dictionary<Point, EdgeConditionKind[]>
        {
            [cell] = [EdgeConditionKind.StoneFloor]
        };

        var chunks = MapFragment.FixupDeferredConditions(
            room, preserved, room.Level.GenerationRandom, TestRoomParameters(room));

        // The fixup must leave the archway alone (the placement-time check already
        // validated it against the preserved condition).
        Assert.Equal(MapFeature.StoneArchway, room.Terrain[room.PointToIndex![cell.X, cell.Y]]);
        Assert.Empty(chunks);
    }

    [Fact]
    public void FixupDeferredConditions_groups_contiguous_cells_into_one_chunk()
    {
        var (room, _) = BuildRoom(roomSize: 8, fill: MapFeature.Default);
        // Two cells side-by-side, both adjacent to a real-chunk floor so neither is
        // demoted by the connectivity-rescue pass.
        TerrainSystem.SetTerrain(MapFeature.StoneFloor, new Point(2, 4), room);
        var roomParameters = TestRoomParameters(room);
        roomParameters[2, 4] = new TestChunkStub(room);
        var preserved = new Dictionary<Point, EdgeConditionKind[]>
        {
            [new Point(2, 3)] = [EdgeConditionKind.StoneFloor],
            [new Point(3, 3)] = [EdgeConditionKind.StoneFloor]
        };

        var chunks = MapFragment.FixupDeferredConditions(
            room, preserved, room.Level.GenerationRandom, roomParameters);

        // The two contiguous floor cells are grouped into a single chunk; the background
        // cells the fixup seals around them form their own perimeter chunks.
        var floorChunk = Assert.Single(chunks, c => c.InsidePoints.Contains(new Point(2, 3)));
        Assert.Contains(new Point(3, 3), floorChunk.InsidePoints);
        Assert.Equal(2, floorChunk.InsidePoints.Count);
    }

    /// <summary>
    ///     A walkable deferred-fill cell exposes floor to the unrendered background on the
    ///     sides its edge conditions didn't reach — typically near the chunk corners. The
    ///     fixup must seal every surrounding background cell (default terrain, no condition
    ///     of its own) with a wall, while leaving real-chunk floor untouched.
    /// </summary>
    [Fact]
    public void FixupDeferredConditions_walls_background_around_a_walkable_condition_cell()
    {
        var (room, _) = BuildRoom(roomSize: 8, fill: MapFeature.Default);
        var cell = new Point(3, 3);
        var preserved = new Dictionary<Point, EdgeConditionKind[]>
        {
            // StoneFloor can't be a wall, so the fixup fills this cell with floor.
            [cell] = [EdgeConditionKind.StoneFloor]
        };
        // Adjacent real-chunk floor to the west so the fill cell isn't classified as
        // isolated and demoted by the connectivity-rescue pass.
        TerrainSystem.SetTerrain(MapFeature.StoneFloor, new Point(2, 3), room);
        var roomParameters = TestRoomParameters(room);
        roomParameters[2, 3] = new TestChunkStub(room);

        var chunks = MapFragment.FixupDeferredConditions(
            room, preserved, room.Level.GenerationRandom, roomParameters);

        Assert.Equal(MapFeature.StoneFloor, room.Terrain[room.PointToIndex![cell.X, cell.Y]]);
        // The background cells around the floor cell are sealed with walls...
        Assert.Equal(MapFeature.RockWall, room.Terrain[room.PointToIndex![3, 2]]);
        Assert.Equal(MapFeature.RockWall, room.Terrain[room.PointToIndex![3, 4]]);
        Assert.Equal(MapFeature.RockWall, room.Terrain[room.PointToIndex![4, 3]]);
        // ...but the adjacent real-chunk floor (not background) is left intact.
        Assert.Equal(MapFeature.StoneFloor, room.Terrain[room.PointToIndex![2, 3]]);
        // A sealing wall is owned by a fixup chunk so the grouping accounts for it.
        Assert.Contains(chunks, c => c.PerimeterPoints.Contains(new Point(4, 3)));
    }

    /// <summary>
    ///     Minimal Chunk subclass for ownership-map slots — only the reference identity
    ///     matters to <see cref="MapFragment.ConvertIsolatedFixupCellsToWalls" />'s seed
    ///     pass, which checks <c>roomParameters[x, y] is not null</c>.
    /// </summary>
    private sealed class TestChunkStub : Chunk
    {
        public TestChunkStub(RoomComponent room)
            : base(
                room,
                boundingRectangle: new Rectangle(new Point(0, 0), 1, 1),
                fragment: null!,
                doorwayPoints: null,
                insidePoints: new HashSet<Point>(),
                perimeterPoints: new HashSet<Point>(),
                roomParameters: TestHelper.CreateRoomParameters(room))
        {
        }
    }
}