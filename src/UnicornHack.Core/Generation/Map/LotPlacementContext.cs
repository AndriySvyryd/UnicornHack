using System.Numerics;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

/// <summary>
///     Per-lot inputs threaded through <see cref="MapFragment.GetEdgeMatchMultiplier" /> and
///     <see cref="MapFragment.TryPlace(RoomComponent, in LotPlacementContext, int)" />. Decouples
///     the fragment from the room layout strategy that is choosing the lot while still allowing
///     the fragment to drive its own intra-lot placement decisions.
/// </summary>
/// <param name="Lot">The candidate rectangle inside the room the fragment is being placed in.</param>
/// <param name="TargetEdge">
///     The lot edge the layout intends the fragment to be flush with, or <c>null</c> when the
///     layout has no preference. When non-null, the fragment will be placed flush against this
///     edge and any conditional edge in that direction is always evaluated.
/// </param>
/// <param name="EmptySpace">
///     Cover of the room's still-empty space (cells that no chunk has been placed in). Used to
///     decide whether a side of the lot is "open" — i.e. another lot of at least
///     <paramref name="MinLotSize" /> could still be carved on that side. <c>null</c> means the
///     caller has no partition (every side is treated as closed, like the defining-fragment
///     path).
/// </param>
/// <param name="MinLotSize">
///     The smallest lot the layout could still allocate. Used together with <see cref="EmptySpace" />
///     to decide whether a side of the lot is open.
/// </param>
/// <param name="PlacedChunks">
///     Interval tree of the bounding rectangles of chunks already committed in
///     <see cref="RoomComponent" />. Queried for fast adjacency tests when picking the target
///     edge. <c>null</c> means the caller has no tree (treated as no placed chunks).
/// </param>
/// <param name="PreservedConditions">
///     Edge conditions that previously placed chunks impose on the cells immediately outside
///     their bounding rectangles. Keyed by the constrained cell (the candidate fragment's
///     payload cell) and listing every condition contributed by an adjacent chunk's edge.
///     <c>null</c> means the caller doesn't track preserved conditions.
/// </param>
/// <param name="DemandedDoorways">
///     Cardinal directions on which the lot is flush with the room perimeter <em>and</em>
///     the corresponding intra-level connection demand is still unmet. The demand-aware
///     weight bonus (<see cref="MapFragment.GetDemandMultiplier" />) boosts fragments
///     whose authored <c>door-{n|s|e|w}</c> tags line up with these directions, and a
///     dynamic placer (<see cref="DynamicMap.GetRoomPoints" />) uses them to bias the
///     placed rectangle toward the demanded edges. <see cref="DirectionFlags.None" />
///     means no outstanding demand reaches this lot.
/// </param>
/// <param name="LotShifted">
///     <c>true</c> when the lot was translated by the room layout (e.g. <c>DenseRoomLayout</c>'s
///     wall-merge shift) onto cells already occupied by a previously placed chunk, so the
///     candidate fragment's payload can overlap that neighbour's terrain. The placement
///     pipeline gates the payload-overlap compatibility check on this flag — when the lot
///     was not shifted, overlap with placed terrain is impossible by construction.
/// </param>
public readonly record struct LotPlacementContext(
    Rectangle Lot,
    RoomParameters RoomParameters,
    Direction? TargetEdge = null,
    RectanglePartition? EmptySpace = null,
    Dimensions MinLotSize = default,
    RectangleIntervalTree? PlacedChunks = null,
    Dictionary<Point, List<EdgeConditionKind>>? PreservedConditions = null,
    DirectionFlags DemandedDoorways = DirectionFlags.None,
    bool LotShifted = false)
{
    /// <summary>
    ///     The room's background fill, taken from <see cref="RoomParameters" />.
    /// </summary>
    public MapFeature DefaultTerrain => RoomParameters.DefaultTerrain;

    /// <summary>
    ///     Per-cell ownership lookup against the room's <see cref="RoomParameters" />.
    ///     Returns the chunk that authored <paramref name="point" /> or <c>null</c>
    ///     when the cell still carries <see cref="DefaultTerrain" />.
    /// </summary>
    public Chunk? GetChunkAt(Point point)
        => RoomParameters[point];

    /// <summary>
    ///     Minimum number of doorways the placed fragment must supply: one per demanded
    ///     cardinal direction. The room layout encodes its own intra-room connectivity
    ///     baseline by OR-ing the target-edge direction into <see cref="DemandedDoorways" />
    ///     before constructing the context, so a lot that has a target edge is guaranteed
    ///     to demand at least one doorway.
    /// </summary>
    public int MinDoorways => BitOperations.PopCount((uint)DemandedDoorways);

    /// <summary>
    ///     <see cref="TargetEdge" /> rendered as a single-bit <see cref="DirectionFlags" />
    ///     value, or <see cref="DirectionFlags.None" /> when no target edge is set.
    ///     Dynamic placers pass this as the flush argument to
    ///     <see cref="DynamicMap.GetRoomPoints" /> so the carved shape lines up with the
    ///     adjacent placed chunk.
    /// </summary>
    public DirectionFlags TargetEdgeFlag => TargetEdge switch
    {
        Direction.North => DirectionFlags.North,
        Direction.South => DirectionFlags.South,
        Direction.East => DirectionFlags.East,
        Direction.West => DirectionFlags.West,
        _ => DirectionFlags.None
    };
}
