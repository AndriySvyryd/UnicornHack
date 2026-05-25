namespace UnicornHack.Generation.Map;

/// <summary>
///     Verifies that <see cref="MapFragment.RegisterPreservedConditions" /> registers the
///     subset of a placed chunk's edge conditions that still face empty space, keyed by the
///     outside cell coordinate, and that a subsequent fragment placed in an adjacent lot is
///     constrained by those preserved conditions.
/// </summary>
public partial class MapFragmentTest
{
    [Fact]
    public void Placed_chunk_preserves_north_edge_when_neighbor_row_is_empty()
    {
        var (room, _) = BuildRoom(roomSize: 9, fill: MapFeature.Default);
        // 4-corner '~' marks all four edges conditional so PayloadArea is 3x2; the
        // N edge demands Filled (the assertion target), the S/W/E edges use U so
        // they remain trivially satisfied against the Default-filled room.
        var fragment = InitializeFragment(@"
~FFF~
.....
.....
~UUU~");

        var lot = new Rectangle(new Point(2, 3), 3, 2);
        var emptySpace = OpenPartition(room, lot);
        var preserved = new Dictionary<Point, List<EdgeConditionKind>>();
        var ctx = new LotPlacementContext(
            lot,
            TestRoomParameters(room),
            TargetEdge: Direction.North,
            EmptySpace: emptySpace,
            MinLotSize: new Dimensions(1, 1),
            PlacedChunks: null,
            PreservedConditions: preserved);
        var chunk = fragment.TryPlace(room, ctx);
        Assert.NotNull(chunk);

        MapFragment.RegisterPreservedConditions(chunk!, room, emptySpace, preserved, TestRoomParameters(room));
        for (var x = chunk!.BoundingRectangle.TopLeft.X; x <= chunk.BoundingRectangle.BottomRight.X; x++)
        {
            var outsidePoint = new Point((byte)x, (byte)(chunk.BoundingRectangle.TopLeft.Y - 1));
            Assert.True(preserved.TryGetValue(outsidePoint, out var list),
                $"Expected preserved condition at {outsidePoint}.");
            Assert.Single(list!);
            Assert.Equal(EdgeConditionKind.Filled, list[0]);
        }
    }

    [Fact]
    public void Default_edge_yields_empty_preserved_dictionary()
    {
        var (room, _) = BuildRoom(roomSize: 9, fill: MapFeature.Default);
        var fragment = InitializeFragment(@"
...
...
...");

        var lot = new Rectangle(new Point(2, 3), 3, 3);
        var emptySpace = OpenPartition(room, lot);
        var preserved = new Dictionary<Point, List<EdgeConditionKind>>();
        var ctx = new LotPlacementContext(
            lot,
            TestRoomParameters(room),
            TargetEdge: Direction.North,
            EmptySpace: emptySpace,
            MinLotSize: new Dimensions(1, 1),
            PlacedChunks: null,
            PreservedConditions: preserved);
        var chunk = fragment.TryPlace(room, ctx);
        Assert.NotNull(chunk);

        MapFragment.RegisterPreservedConditions(chunk!, room, emptySpace, preserved, TestRoomParameters(room));
        Assert.Empty(preserved);
    }

    [Fact]
    public void Preserved_edge_constrains_adjacent_fragment_weight()
    {
        // Place anchor whose north edge demands Filled above its top row; other
        // edges use U so they don't interfere with the placement under the
        // both-corners rule (which makes all four edges conditional).
        var (room, _) = BuildRoom(roomSize: 9, fill: MapFeature.Default);
        var anchor = InitializeFragment(@"
~FFF~
.....
.....
~UUU~");

        var anchorLot = new Rectangle(new Point(2, 3), 3, 2);
        var emptySpace = OpenPartition(room, anchorLot);
        var preserved = new Dictionary<Point, List<EdgeConditionKind>>();
        var anchorCtx = new LotPlacementContext(
            anchorLot,
            TestRoomParameters(room),
            TargetEdge: Direction.North,
            EmptySpace: emptySpace,
            MinLotSize: new Dimensions(1, 1),
            PlacedChunks: null,
            PreservedConditions: preserved);
        var anchorChunk = anchor.TryPlace(room, anchorCtx);
        Assert.NotNull(anchorChunk);

        emptySpace.Insert(anchorChunk!.BoundingRectangle);
        MapFragment.RegisterPreservedConditions(anchorChunk, room, emptySpace, preserved, TestRoomParameters(room));
        Assert.NotEmpty(preserved);

        // Candidate: 3×2 payload, no edges. Placed flush south against anchor's top edge.
        // Its bottom payload row is StoneFloor — satisfies preserved Filled → bonus.
        var candidate = InitializeFragment(@"
...
...");

        var adjacentLot = new Rectangle(new Point(2, 1), 3, 2);
        var placedChunks = new RectangleIntervalTree(room.BoundingRectangle);
        placedChunks.Insert(anchorChunk.BoundingRectangle);
        var ctxAdjacent = new LotPlacementContext(
            adjacentLot,
            TestRoomParameters(room),
            TargetEdge: Direction.South,
            EmptySpace: emptySpace,
            MinLotSize: new Dimensions(1, 1),
            PlacedChunks: placedChunks,
            PreservedConditions: preserved);

        var multiplier = candidate.GetEdgeMatchMultiplier(room, ctxAdjacent);
        Assert.True(multiplier > 1f,
            $"Expected preserved Filled contribution, got {multiplier}.");
    }

    [Fact]
    public void Preserved_edge_vetoes_adjacent_fragment_that_violates_it()
    {
        var (room, _) = BuildRoom(roomSize: 9, fill: MapFeature.Default);
        // Anchor declares Unfilled across its north edge; S/W/E use U to remain
        // trivially satisfied against the Default-filled room under the
        // both-corners rule.
        var anchor = InitializeFragment(@"
~UUU~
.....
.....
~UUU~");

        var anchorLot = new Rectangle(new Point(2, 3), 3, 2);
        var emptySpace = OpenPartition(room, anchorLot);
        var preserved = new Dictionary<Point, List<EdgeConditionKind>>();
        var anchorCtx = new LotPlacementContext(
            anchorLot,
            TestRoomParameters(room),
            TargetEdge: Direction.North,
            EmptySpace: emptySpace,
            MinLotSize: new Dimensions(1, 1),
            PlacedChunks: null,
            PreservedConditions: preserved);
        var anchorChunk = anchor.TryPlace(room, anchorCtx);
        Assert.NotNull(anchorChunk);

        emptySpace.Insert(anchorChunk!.BoundingRectangle);
        MapFragment.RegisterPreservedConditions(anchorChunk, room, emptySpace, preserved, TestRoomParameters(room));
        Assert.NotEmpty(preserved);

        // Candidate is solid StoneFloor — violates the Unfilled preserved condition.
        var candidate = InitializeFragment(@"
...
...");

        var adjacentLot = new Rectangle(new Point(2, 1), 3, 2);
        var placedChunks = new RectangleIntervalTree(room.BoundingRectangle);
        placedChunks.Insert(anchorChunk.BoundingRectangle);
        var ctxAdjacent = new LotPlacementContext(
            adjacentLot,
            TestRoomParameters(room),
            TargetEdge: Direction.South,
            EmptySpace: emptySpace,
            MinLotSize: new Dimensions(1, 1),
            PlacedChunks: placedChunks,
            PreservedConditions: preserved);

        Assert.Equal(0f, candidate.GetEdgeMatchMultiplier(room, ctxAdjacent));
    }
}
