using CSharpScriptSerialization;

namespace UnicornHack.Generation.Map;

/// <summary>
///     A <see cref="RoomLayout" /> that packs chunks tightly together, eliminating the
///     double-wall column that <see cref="UniformRoomLayout" /> leaves between adjacent
///     chunks. Produces denser, town-like rooms.
/// </summary>
/// <remarks>
///     <para>
///         Inherits the lot-iteration scheme from <see cref="UniformRoomLayout" /> but
///         tunes defaults toward higher coverage and smaller lots, and overrides
///         <see cref="UniformRoomLayout.CreatePlacementContext" /> to attempt a
///         <em>wall-merge shift</em> for every lot adjacent to a previously placed chunk.
///     </para>
///     <para>
///         <strong>Wall-merge shift.</strong> By default <see cref="UniformRoomLayout" />
///         leaves bounding rectangles touching (no gap) but with two perimeter wall columns
///         side-by-side (the neighbour's right wall at column <c>X</c> and the candidate's
///         left wall at column <c>X+1</c>). DenseRoomLayout shifts the candidate lot one
///         tile toward the neighbour so the candidate's perimeter wall column lands ON the
///         neighbour's perimeter wall column (column <c>X</c>). Both fragments write
///         <c>#</c> to that column, collapsing the double wall into a single shared wall.
///         The resulting chunk bounding rectangles overlap by exactly one column or row;
///         <see cref="Utils.DataStructures.RectangleIntervalTree" /> and
///         <see cref="Utils.DataStructures.RectanglePartition" /> both support overlap.
///     </para>
///     <para>
///         <strong>Safety rules.</strong> The shift is rejected when the neighbour
///         fragment carries <see cref="MapFragment.NoOverwrite" /> (authored statics that
///         depend on exact wall thickness opt in). When the neighbour's facing perimeter
///         contains important cells (doorways, features, etc.) the shift still proceeds —
///         <see cref="MapFragment.Write" /> preserves those cells by suppressing the
///         candidate's <c>#</c> write at any point where existing non-wall content lives.
///         The candidate's own <see cref="MapFragment.NoOverwrite" /> flag is irrelevant
///         to the shift decision: it protects against neighbours shifting onto <em>this</em>
///         fragment, not against <em>this</em> fragment shifting onto a (non-NoOverwrite)
///         neighbour.
///     </para>
/// </remarks>
public class DenseRoomLayout : UniformRoomLayout, ICSScriptSerializable
{
    protected static new Dimensions DefaultMinLotSize = new(4, 4);

    protected static float DefaultDenseCoverage = 0.85f;

    protected static byte DefaultDenseMaxChunkCount = 12;

    public DenseRoomLayout()
    {
        MinLotSize = DefaultMinLotSize;
        Coverage = DefaultDenseCoverage;
        MaxChunkCount = DefaultDenseMaxChunkCount;
    }

    protected override Rectangle MaybeShiftLot(
        Rectangle lot, Direction? targetEdge, IReadOnlyList<Chunk> placedChunks)
    {
        if (targetEdge.HasValue
            && TryShiftToMerge(lot, targetEdge.Value, placedChunks, out var shifted))
        {
            return shifted;
        }

        return lot;
    }

    /// <summary>
    ///     Attempts to shift <paramref name="lot" /> one tile toward an adjacent placed
    ///     chunk on <paramref name="targetEdge" /> so the resulting candidate's perimeter
    ///     wall column will overlap the neighbour's perimeter wall column, collapsing the
    ///     double wall into a single shared one.
    /// </summary>
    /// <returns>
    ///     <c>true</c> when a wall-merge shift is legal; <paramref name="shiftedLot" /> is
    ///     then the lot translated by one tile toward the neighbour. <c>false</c> when the
    ///     shift must be rejected (no adjacent neighbour found or neighbour has
    ///     <see cref="MapFragment.NoOverwrite" />).
    /// </returns>
    protected virtual bool TryShiftToMerge(
        Rectangle lot,
        Direction targetEdge,
        IReadOnlyList<Chunk> placedChunks,
        out Rectangle shiftedLot)
    {
        shiftedLot = lot;

        var neighbour = FindAdjacentNeighbour(lot, targetEdge, placedChunks);
        if (neighbour == null)
        {
            return false;
        }

        if (neighbour.Fragment.NoOverwrite)
        {
            return false;
        }

        // Overlap on the neighbour's facing perimeter is allowed even when that perimeter
        // carries important cells (doorways, features, etc.). The fragment-placing code
        // (MapFragment.Write) preserves existing non-wall content when its candidate
        // perimeter would write a plain wall over it.
        shiftedLot = ShiftLot(lot, targetEdge);
        return true;
    }

    /// <summary>
    ///     Finds the placed chunk whose bounding rectangle directly touches
    ///     <paramref name="lot" /> on <paramref name="targetEdge" /> (no gap between the
    ///     rectangles). When multiple chunks touch the same edge, the one with the longest
    ///     contact run is returned.
    /// </summary>
    private static Chunk? FindAdjacentNeighbour(
        Rectangle lot, Direction targetEdge, IReadOnlyList<Chunk> placedChunks)
    {
        Chunk? best = null;
        var bestContact = 0;
        for (var i = 0; i < placedChunks.Count; i++)
        {
            var chunk = placedChunks[i];
            var contact = ContactLength(lot, targetEdge, chunk.BoundingRectangle);
            if (contact > bestContact)
            {
                bestContact = contact;
                best = chunk;
            }
        }

        return best;
    }

    /// <summary>
    ///     Returns the length of the contact run where <paramref name="placed" /> sits
    ///     immediately outside <paramref name="lot" /> on <paramref name="edge" />, i.e.
    ///     the chunk's facing side abuts the lot with a zero-cell gap. Returns 0 when the
    ///     chunk is not touching that edge.
    /// </summary>
    private static int ContactLength(Rectangle lot, Direction edge, Rectangle placed)
    {
        switch (edge)
        {
            case Direction.West:
                if (lot.TopLeft.X == 0 || placed.BottomRight.X != lot.TopLeft.X - 1)
                {
                    return 0;
                }

                return OverlapLength(
                    lot.TopLeft.Y, lot.BottomRight.Y, placed.TopLeft.Y, placed.BottomRight.Y);

            case Direction.East:
                if (placed.TopLeft.X != lot.BottomRight.X + 1)
                {
                    return 0;
                }

                return OverlapLength(
                    lot.TopLeft.Y, lot.BottomRight.Y, placed.TopLeft.Y, placed.BottomRight.Y);

            case Direction.North:
                if (lot.TopLeft.Y == 0 || placed.BottomRight.Y != lot.TopLeft.Y - 1)
                {
                    return 0;
                }

                return OverlapLength(
                    lot.TopLeft.X, lot.BottomRight.X, placed.TopLeft.X, placed.BottomRight.X);

            case Direction.South:
                if (placed.TopLeft.Y != lot.BottomRight.Y + 1)
                {
                    return 0;
                }

                return OverlapLength(
                    lot.TopLeft.X, lot.BottomRight.X, placed.TopLeft.X, placed.BottomRight.X);

            default:
                return 0;
        }
    }

    private static int OverlapLength(int aStart, int aEnd, int bStart, int bEnd)
    {
        var start = Math.Max(aStart, bStart);
        var end = Math.Min(aEnd, bEnd);
        return end >= start ? end - start + 1 : 0;
    }

    /// <summary>
    ///     Returns the lot translated by one tile toward <paramref name="edge" />. The
    ///     translated lot overlaps the placed neighbour's perimeter wall column/row by
    ///     exactly one cell — the shared wall after the merge.
    /// </summary>
    /// <remarks>
    ///     Precondition: <paramref name="lot" /> sits flush against a neighbour on
    ///     <paramref name="edge" /> with a non-zero <see cref="ContactLength" />. The
    ///     callers (<see cref="TryShiftToMerge" />) verify this first, so the
    ///     <c>(byte)(coord - 1)</c> on the West/North branches can never underflow and
    ///     the <c>+1</c> on East/South stays within the room's byte range. The
    ///     <see cref="Debug.Assert" />s make the contract explicit.
    /// </remarks>
    private static Rectangle ShiftLot(Rectangle lot, Direction edge)
    {
        switch (edge)
        {
            case Direction.West:
                Debug.Assert(lot.TopLeft.X >= 1, "West shift requires lot.TopLeft.X >= 1.");
                return new Rectangle(
                    new Point((byte)(lot.TopLeft.X - 1), lot.TopLeft.Y),
                    new Point((byte)(lot.BottomRight.X - 1), lot.BottomRight.Y));
            case Direction.East:
                Debug.Assert(lot.BottomRight.X < byte.MaxValue, "East shift requires lot.BottomRight.X < 255.");
                return new Rectangle(
                    new Point((byte)(lot.TopLeft.X + 1), lot.TopLeft.Y),
                    new Point((byte)(lot.BottomRight.X + 1), lot.BottomRight.Y));
            case Direction.North:
                Debug.Assert(lot.TopLeft.Y >= 1, "North shift requires lot.TopLeft.Y >= 1.");
                return new Rectangle(
                    new Point(lot.TopLeft.X, (byte)(lot.TopLeft.Y - 1)),
                    new Point(lot.BottomRight.X, (byte)(lot.BottomRight.Y - 1)));
            case Direction.South:
                Debug.Assert(lot.BottomRight.Y < byte.MaxValue, "South shift requires lot.BottomRight.Y < 255.");
                return new Rectangle(
                    new Point(lot.TopLeft.X, (byte)(lot.TopLeft.Y + 1)),
                    new Point(lot.BottomRight.X, (byte)(lot.BottomRight.Y + 1)));
            default:
                return lot;
        }
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<DenseRoomLayout>(GetPropertyConditions<DenseRoomLayout>());

    public new ICSScriptSerializer GetSerializer() => Serializer;

    protected static new Dictionary<string, Func<TDenseRoomLayout, object?, bool>> GetPropertyConditions<TDenseRoomLayout>()
        where TDenseRoomLayout : DenseRoomLayout => new()
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        { nameof(Coverage), (_, v) => (float)v! != DefaultDenseCoverage },
        { nameof(MaxChunkCount), (_, v) => (byte)v! != DefaultDenseMaxChunkCount },
        { nameof(MinLotSize), (_, v) => !DefaultMinLotSize.Equals(v) }
    };
}
