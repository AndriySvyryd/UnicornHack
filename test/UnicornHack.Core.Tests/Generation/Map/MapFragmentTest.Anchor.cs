namespace UnicornHack.Generation.Map;

/// <summary>
///     Exercises the placement-anchor / target-edge / preserved-edge propagation logic
///     added to <see cref="MapFragment" /> on top of the edge-condition multiplier system.
/// </summary>
public partial class MapFragmentTest
{
    [Fact]
    public void TargetEdge_North_forces_anchor_to_top_of_lot()
    {
        var (room, _) = BuildRoom(roomSize: 9, fill: MapFeature.StoneFloor);
        var fragment = InitializeFragment(@"
...
...
...");

        var lot = new Rectangle(new Point(2, 2), 3, 5);
        var ctx = OpenCtx(room, lot, target: Direction.North);
        Assert.True(fragment.TryComputePlacementAnchor(room, ctx, random: null, out var anchor));
        Assert.Equal(new Point(2, 2), anchor);
    }

    [Fact]
    public void TargetEdge_East_forces_anchor_flush_against_east_lot_edge()
    {
        var (room, _) = BuildRoom(roomSize: 9, fill: MapFeature.StoneFloor);
        var fragment = InitializeFragment(@"
...
...
...");

        var lot = new Rectangle(new Point(1, 2), 5, 3);
        var ctx = OpenCtx(room, lot, target: Direction.East);
        Assert.True(fragment.TryComputePlacementAnchor(room, ctx, random: null, out var anchor));
        // Lot.X = 1..5 (width 5); payload width 3 → flush east means anchor.X = 5-3+1 = 3.
        Assert.Equal(new Point(3, 2), anchor);
    }

    [Fact]
    public void Closed_north_side_forces_flush_north_placement()
    {
        var (room, _) = BuildRoom(roomSize: 9, fill: MapFeature.StoneFloor);
        // 4-corner '~' marks all four edges conditional so PayloadArea is 3x2; the
        // N edge demands Filled, the S/W/E edges resolve to StoneFloor against the
        // floor-filled room.
        var fragment = InitializeFragment(@"
~FFF~
.....
.....
~SSS~");

        // Lot at y=3..7, payload 3×2. Insert an obstacle directly north of the lot so the
        // strip just above is not contained in any single empty rectangle → side closed.
        var lot = new Rectangle(new Point(2, 3), 3, 5);
        var emptySpace = new RectanglePartition(room.BoundingRectangle);
        emptySpace.Insert(new Rectangle(new Point(2, 2), 3, 1));
        emptySpace.Insert(lot);
        var ctx = new LotPlacementContext(
            lot,
            TestHelper.CreateRoomParameters(room),
            TargetEdge: null,
            EmptySpace: emptySpace,
            MinLotSize: new Dimensions(1, 1),
            PlacedChunks: null,
            PreservedConditions: null);

        Assert.True(fragment.TryComputePlacementAnchor(room, ctx, random: null, out var anchor));
        Assert.Equal(new Point(2, 3), anchor);
    }

    [Fact]
    public void Opposing_closed_edges_with_mismatched_dim_find_anchor_when_boundary_relaxes()
    {
        var (room, _) = BuildRoom(roomSize: 9, fill: MapFeature.StoneFloor);
        var fragment = InitializeFragment(@"
~FFF~
.....
~FFF~");

        // Lot height 9, payload height 1. Both N&S sides are closed by the room
        // boundary -- but both conditional Filled edges cannot match out-of-room
        // cells, so the boundary-relaxation lifts both sticks. The anchor is free to
        // settle anywhere along the lot's height where the payload fits.
        var lot = new Rectangle(new Point(2, 0), 3, 9);
        var ctx = OpenCtx(room, lot);
        Assert.True(fragment.TryComputePlacementAnchor(room, ctx, random: null, out _));
    }
}
