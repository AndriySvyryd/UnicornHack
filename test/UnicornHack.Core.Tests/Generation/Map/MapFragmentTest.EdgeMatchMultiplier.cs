using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

public partial class MapFragmentTest
{
    [Fact]
    public void EdgeMultiplier_Fragment_with_no_conditional_edges_returns_one()
    {
        var (room, _) = BuildRoom(roomSize: 7);
        var fragment = InitializeFragment(@"
.....
.....
.....");

        var multiplier = fragment.GetEdgeMatchMultiplier(
            room, new LotPlacementContext(new Rectangle(new Point(1, 1), 5, 3), TestHelper.CreateRoomParameters(room)));

        Assert.Equal(1f, multiplier);
    }

    [Fact]
    public void EdgeMultiplier_Fragment_larger_than_lot_returns_zero()
    {
        var (room, _) = BuildRoom(roomSize: 7);
        var fragment = InitializeFragment(@"
.....
.....
.....");

        var multiplier = fragment.GetEdgeMatchMultiplier(
            room, new LotPlacementContext(new Rectangle(new Point(1, 1), 3, 3), TestHelper.CreateRoomParameters(room)));

        Assert.Equal(0f, multiplier);
    }

    [Fact]
    public void Unfilled_condition_matches_default_neighbor()
    {
        var (room, _) = BuildRoom(roomSize: 7, fill: MapFeature.Default);
        // Only the top edge is conditional under the both-corners rule; the U
        // constraint at every N slot matches the Default-filled neighbour row above
        // the lot.
        var fragment = InitializeFragment(@"
~UUU~
.....
.....");

        var lot = FlushNorthLot(room, fragment);
        var multiplier = fragment.GetEdgeMatchMultiplier(room, Ctx(lot));

        Assert.Equal(1f + 3 * MapFragment.EdgeMatchTieBreakerAlpha, multiplier);
    }

    [Fact]
    public void Unfilled_condition_against_placed_neighbor_returns_zero()
    {
        var (room, _) = BuildRoom(roomSize: 7, fill: MapFeature.StoneFloor);
        var fragment = InitializeFragment(@"
~UUU~
.....
.....");

        var lot = FlushNorthLot(room, fragment);
        Assert.Equal(0f, fragment.GetEdgeMatchMultiplier(room, Ctx(lot)));
    }

    [Fact]
    public void Filled_condition_against_placed_neighbor_yields_bonus()
    {
        var (room, _) = BuildRoom(roomSize: 7, fill: MapFeature.StoneFloor);
        var fragment = InitializeFragment(@"
~FFF~
.....
.....");

        // 3 N (F vs StoneFloor) matches; only the north edge is conditional under
        // the both-corners rule.
        var lot = FlushNorthLot(room, fragment);
        Assert.Equal(1f + 3 * MapFragment.EdgeMatchTieBreakerAlpha,
            fragment.GetEdgeMatchMultiplier(room, Ctx(lot)));
    }

    [Fact]
    public void Filled_condition_against_default_neighbor_returns_zero()
    {
        var (room, _) = BuildRoom(roomSize: 7, fill: MapFeature.Default);
        var fragment = InitializeFragment(@"
~FFF~
.....
.....");

        var lot = FlushNorthLot(room, fragment);
        Assert.Equal(0f, fragment.GetEdgeMatchMultiplier(room, Ctx(lot)));
    }

    [Fact]
    public void TileSpecific_condition_matches_exact_feature_only()
    {
        var (room, _) = BuildRoom(roomSize: 7, fill: MapFeature.StoneWall);
        var fragment = InitializeFragment(@"
~###~
.....
.....");

        // North #-cells match the StoneWall neighbour row above the lot; only the
        // north edge is conditional under the both-corners rule.
        var lot = FlushNorthLot(room, fragment);
        Assert.Equal(1f + 3 * MapFragment.EdgeMatchTieBreakerAlpha,
            fragment.GetEdgeMatchMultiplier(room, Ctx(lot)));

        var (floorRoom, _) = BuildRoom(roomSize: 7, fill: MapFeature.StoneFloor);
        Assert.Equal(0f, fragment.GetEdgeMatchMultiplier(floorRoom, Ctx(lot)));
    }

    [Fact]
    public void Same_condition_matches_when_neighbor_equals_fragment_edge_tile()
    {
        var (room, _) = BuildRoom(roomSize: 7, fill: MapFeature.StoneFloor);
        var fragment = InitializeFragment(@"
~SSS~
.....
.....");

        // 3 N (Same resolves to StoneFloor and matches the StoneFloor neighbour row);
        // only the north edge is conditional.
        var lot = FlushNorthLot(room, fragment);
        Assert.Equal(1f + 3 * MapFragment.EdgeMatchTieBreakerAlpha,
            fragment.GetEdgeMatchMultiplier(room, Ctx(lot)));
    }

    [Fact]
    public void Same_condition_against_mismatched_neighbor_returns_zero()
    {
        var (room, _) = BuildRoom(roomSize: 7, fill: MapFeature.RockFloor);
        var fragment = InitializeFragment(@"
~SSS~
.....
.....");

        var lot = FlushNorthLot(room, fragment);
        Assert.Equal(0f, fragment.GetEdgeMatchMultiplier(room, Ctx(lot)));
    }

    [Fact]
    public void Conditional_edge_is_evaluated_even_when_side_is_open_and_not_target()
    {
        // Conditional edges are always evaluated; the side being "open" no longer
        // suppresses them. 4-corner map so all four edges are conditional under the
        // both-corners rule; the N edge demands Filled (the test target), the other
        // three resolve to StoneFloor against the floor-filled room.
        var (room, _) = BuildRoom(roomSize: 9, fill: MapFeature.StoneFloor);
        var fragment = InitializeFragment(@"
~FFF~
.....
~SSS~");

        var lot = new Rectangle(new Point(2, 2), 3, 5);
        var ctx = OpenCtx(room, lot);
        // 3 N (F) + 3 S (S → StoneFloor) + 1 W + 1 E (StoneFloor) = 8 matches.
        Assert.Equal(1f + 8 * MapFragment.EdgeMatchTieBreakerAlpha,
            fragment.GetEdgeMatchMultiplier(room, ctx));
    }

    [Fact]
    public void Edge_is_evaluated_when_targetEdge_matches()
    {
        var (room, _) = BuildRoom(roomSize: 9, fill: MapFeature.StoneFloor);
        var fragment = InitializeFragment(@"
~FFF~
.....
~SSS~");

        // 4-corner map: 3 N (F) + 3 S (StoneFloor) + 1 W + 1 E = 8 matches when
        // targeted at the north lot edge.
        var lot = new Rectangle(new Point(2, 2), 3, 5);
        var ctx = OpenCtx(room, lot, target: Direction.North);
        Assert.Equal(1f + 8 * MapFragment.EdgeMatchTieBreakerAlpha,
            fragment.GetEdgeMatchMultiplier(room, ctx));
    }

    [Fact]
    public void Filled_edge_against_boundary_fails_but_anchor_slides_inward()
    {
        // A conditional Filled edge that lands flush against the room boundary fails
        // honestly (out-of-room neighbours don't satisfy Filled). The anchor sweep
        // slides the fragment one row inward where the F slots face in-room StoneFloor
        // terrain and match. 4-corner map: 3 N (F) + 3 S (StoneFloor) + 1 W + 1 E.
        var (room, _) = BuildRoom(roomSize: 9, fill: MapFeature.StoneFloor);
        var fragment = InitializeFragment(@"
~FFF~
.....
~SSS~");

        var lot = new Rectangle(new Point(2, 0), 3, 5);
        var ctx = OpenCtx(room, lot);
        Assert.Equal(1f + 8 * MapFragment.EdgeMatchTieBreakerAlpha,
            fragment.GetEdgeMatchMultiplier(room, ctx));
    }

    [Fact]
    public void Out_of_room_neighbor_counts_as_default()
    {
        var (room, _) = BuildRoom(roomSize: 5, fill: MapFeature.StoneFloor);
        // 4-corner map keeps the payload at 3×2 so the 3-wide lot can hold it.
        var fragment = InitializeFragment(@"
~UUU~
.....
.....
~SSS~");

        // 3 N (U vs out-of-room Default) + 3 S (StoneFloor) + 2 W + 2 E (StoneFloor) = 10.
        var lot = new Rectangle(new Point(1, 0), 3, 3);
        Assert.Equal(1f + 10 * MapFragment.EdgeMatchTieBreakerAlpha,
            fragment.GetEdgeMatchMultiplier(room, Ctx(lot)));
    }

    [Fact]
    public void Opposing_filled_edges_at_room_boundary_anchor_inward()
    {
        // Both N and S Filled edges land flush against the room boundary at the
        // extreme anchors. The anchor sweep finds an interior position where neither
        // F slot faces out-of-room, so both edges match in-room StoneFloor terrain.
        // 3 N + 3 S (F vs StoneFloor) + 1 W + 1 E (implicit StoneFloor vs StoneFloor).
        var (room, _) = BuildRoom(roomSize: 9, fill: MapFeature.StoneFloor);
        var fragment = InitializeFragment(@"
~FFF~
.....
~FFF~");

        var lot = new Rectangle(new Point(2, 0), 3, 9);
        var ctx = OpenCtx(room, lot);
        Assert.Equal(1f + 8 * MapFragment.EdgeMatchTieBreakerAlpha,
            fragment.GetEdgeMatchMultiplier(room, ctx));
    }

    [Fact]
    public void Multiple_edges_each_contribute_to_tie_breaker()
    {
        var (room, _) = BuildRoom(roomSize: 7, fill: MapFeature.StoneFloor);
        var fragment = InitializeFragment(@"
~FFF~
.....
~FFF~");

        // 3 N + 3 S (F vs StoneFloor) + 1 W + 1 E (implicit StoneFloor vs StoneFloor).
        var lot = new Rectangle(new Point(2, 2), 3, 1);
        Assert.Equal(1f + 8 * MapFragment.EdgeMatchTieBreakerAlpha,
            fragment.GetEdgeMatchMultiplier(room, Ctx(lot)));
    }

    [Fact]
    public void Edge_condition_succeeds_when_fragment_hugs_room_boundary()
    {
        var (room, _) = BuildRoom(roomSize: 7, fill: MapFeature.StoneFloor);
        // 4-corner map keeps the payload at 3×2; the N edge demands the fragment hug
        // the room boundary, the other three edges resolve to StoneFloor.
        var fragment = InitializeFragment(@"
~EEE~
.....
.....
~SSS~");

        // 3 N (Edge vs out-of-room) + 3 S (StoneFloor) + 2 W + 2 E (StoneFloor) = 10.
        var lot = new Rectangle(new Point(1, 0), 3, 3);
        Assert.Equal(1f + 10 * MapFragment.EdgeMatchTieBreakerAlpha,
            fragment.GetEdgeMatchMultiplier(room, Ctx(lot)));
    }

    [Fact]
    public void Edge_condition_fails_when_neighbor_is_in_room_even_if_default()
    {
        var (room, _) = BuildRoom(roomSize: 7, fill: MapFeature.Default);
        var fragment = InitializeFragment(@"
~EEE~
.....
.....");

        var lot = FlushNorthLot(room, fragment);
        Assert.Equal(0f, fragment.GetEdgeMatchMultiplier(room, Ctx(lot)));
    }

    // --- Regression coverage for placement bugs surfaced by the composition distribution probe ---

    [Fact]
    public void Oversized_payload_returns_null_instead_of_throwing()
    {
        // Bug #10 — a fragment whose payload is larger than the lot must not throw from
        // TryPlaceStaticMap's fallback path; it must return null so RoomLayout.TryPlace
        // can move on to the next weighted candidate.
        var (room, _) = BuildRoom(roomSize: 9, fill: MapFeature.Default);
        var fragment = InitializeFragment(@"
.....
.....
.....
.....
.....");

        // Lot is 3×9; payload is 5×5 → payload width 5 > lot width 3. TryPlace must
        // return null (placement infeasible) rather than throwing.
        var lot = new Rectangle(new Point(2, 0), 3, 9);
        var ctx = OpenCtx(room, lot);

        var chunk = fragment.TryPlace(room, ctx);

        Assert.Null(chunk);
    }

    [Fact]
    public void Oversized_payload_has_zero_weight()
    {
        var (room, _) = BuildRoom(roomSize: 9, fill: MapFeature.Default);
        var fragment = InitializeFragment(@"
.....
.....
.....
.....
.....");

        var lot = new Rectangle(new Point(2, 0), 3, 9);
        var weight = fragment.GetEdgeMatchMultiplier(room, OpenCtx(room, lot));

        Assert.Equal(0f, weight);
    }

    private static LotPlacementContext Ctx(Rectangle lot)
        => new(lot, TestRoomParameters());

    private static Rectangle FlushNorthLot(RoomComponent room, TestNormalMapFragment fragment)
    {
        var w = fragment.PayloadArea.Width;
        var h = fragment.PayloadArea.Height;
        return new Rectangle(new Point(1, 1), (byte)w, (byte)h);
    }
}
