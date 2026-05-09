namespace UnicornHack.Utils.DataStructures;

/// <summary>
///     Maintains a covering of the empty space inside a bounding rectangle by axis-aligned
///     rectangles, given a set of inserted obstacle rectangles. Each obstacle insertion
///     splits every overlapping empty rectangle into up to four side strips and prunes any
///     resulting strip that is fully contained in another empty rectangle.
/// </summary>
/// <remarks>
///     <see cref="EmptyRectangles" /> is kept sorted by area in descending order so that
///     <see cref="GetLargest" /> is O(1) and consumers searching for the largest rectangle
///     that satisfies an extra constraint (e.g. minimum dimensions) can break out of the
///     scan as soon as they reach a candidate too small to ever satisfy it.
///     This is a pragmatic alternative to the <em>minimum rectilinear partition</em>
///     (Imai–Asano with Hopcroft–Karp matching). The set produced here is a valid cover
///     of the empty region but is not guaranteed to be of minimum cardinality and does
///     not enumerate every maximal empty rectangle (a maximal rectangle that spans gaps
///     between two obstacles in different splits will be missed). For typical roguelike
///     layouts (≤ ~32 obstacles) it is fast and good enough to pick a large empty
///     rectangle for the next lot.
/// </remarks>
public class RectanglePartition
{
    private readonly List<Rectangle> _emptyRectangles;

    public RectanglePartition(Rectangle bounds)
    {
        _emptyRectangles = [bounds];
    }

    /// <summary>
    ///     The current cover of the empty region, sorted by area in descending order.
    /// </summary>
    public IReadOnlyList<Rectangle> EmptyRectangles => _emptyRectangles;

    /// <summary>
    ///     Removes <paramref name="obstacle" /> from the empty region. Every empty rectangle
    ///     overlapping the obstacle is replaced by up to four side strips that lie outside
    ///     the obstacle but inside the original empty rectangle.
    /// </summary>
    public void Insert(Rectangle obstacle)
    {
        for (var i = _emptyRectangles.Count - 1; i >= 0; i--)
        {
            var empty = _emptyRectangles[i];
            if (!empty.Overlaps(obstacle))
            {
                continue;
            }

            _emptyRectangles.RemoveAt(i);

            // Left strip: portion of empty strictly left of the obstacle.
            if (empty.TopLeft.X < obstacle.TopLeft.X)
            {
                AddSorted(new Rectangle(
                    empty.TopLeft,
                    new Point((byte)(obstacle.TopLeft.X - 1), empty.BottomRight.Y)));
            }

            // Right strip.
            if (empty.BottomRight.X > obstacle.BottomRight.X)
            {
                AddSorted(new Rectangle(
                    new Point((byte)(obstacle.BottomRight.X + 1), empty.TopLeft.Y),
                    empty.BottomRight));
            }

            // Top strip (smaller Y).
            if (empty.TopLeft.Y < obstacle.TopLeft.Y)
            {
                AddSorted(new Rectangle(
                    empty.TopLeft,
                    new Point(empty.BottomRight.X, (byte)(obstacle.TopLeft.Y - 1))));
            }

            // Bottom strip.
            if (empty.BottomRight.Y > obstacle.BottomRight.Y)
            {
                AddSorted(new Rectangle(
                    new Point(empty.TopLeft.X, (byte)(obstacle.BottomRight.Y + 1)),
                    empty.BottomRight));
            }
        }

        Prune();
    }

    /// <summary>
    ///     Looks for pairs of touching or overlapping empty rectangles whose union contains
    ///     a rectangle larger than either piece alone, and adds those merged rectangles to
    ///     the partition. Existing entries are not removed; <see cref="Prune" /> drops any that
    ///     end up fully contained in a newly merged rectangle. Returns the largest rectangle
    ///     added by this call, or <c>null</c> when no merge produced an improvement.
    /// </summary>
    public Rectangle? Recalculate()
    {
        Rectangle? largestAdded = null;
        var largestAddedArea = -1;

        // We iterate the original entries by index. Every merged rectangle has area
        // strictly greater than both inputs, so AddSorted always inserts it at a position
        // ≤ i (and ≤ j). Bumping i, j, and originalCount keeps the iteration anchored on
        // the same set of originals — the new entries sit at positions we've already moved
        // past, so we never revisit them. This avoids a snapshot allocation.
        var count = _emptyRectangles.Count;
        for (var i = 0; i < count; i++)
        {
            var a = _emptyRectangles[i];
            for (var j = i + 1; j < count; j++)
            {
                var b = _emptyRectangles[j];
                var minRequiredArea = a.Area > b.Area ? a.Area : b.Area;

                if (TryMergeAlong(a.XProjection, b.XProjection, a.YProjection, b.YProjection,
                        verticalUnion: true, minRequiredArea, ref largestAdded, ref largestAddedArea) >= 0)
                {
                    i++;
                    j++;
                    count++;
                }

                if (TryMergeAlong(a.YProjection, b.YProjection, a.XProjection, b.XProjection,
                        verticalUnion: false, minRequiredArea, ref largestAdded, ref largestAddedArea) >= 0)
                {
                    i++;
                    j++;
                    count++;
                }
            }
        }

        if (largestAdded != null)
        {
            Prune();
        }

        return largestAdded;
    }

    /// <summary>
    ///     Tries to find an axis-aligned rectangle inside <paramref name="intersectAxisA" /> ×
    ///     <paramref name="unionAxisA" /> ∪ <paramref name="intersectAxisB" /> ×
    ///     <paramref name="unionAxisB" /> whose area exceeds <paramref name="minRequiredArea" />,
    ///     adds it via <see cref="AddSorted" />, and returns the position it was inserted at.
    ///     Returns <c>-1</c> when no improvement was found.
    /// </summary>
    private int TryMergeAlong(
        Segment intersectAxisA, Segment intersectAxisB,
        Segment unionAxisA, Segment unionAxisB,
        bool verticalUnion,
        int minRequiredArea,
        ref Rectangle? largestAdded,
        ref int largestAddedArea)
    {
        var intersection = intersectAxisA.Intersection(intersectAxisB);
        if (intersection == null)
        {
            return -1;
        }

        // The union axes must touch or overlap to form a single contiguous segment.
        if (unionAxisA.End + 1 < unionAxisB.Beginning || unionAxisB.End + 1 < unionAxisA.Beginning)
        {
            return -1;
        }

        var unionBeginning = unionAxisA.Beginning < unionAxisB.Beginning ? unionAxisA.Beginning : unionAxisB.Beginning;
        var unionEnd = unionAxisA.End > unionAxisB.End ? unionAxisA.End : unionAxisB.End;
        var union = new Segment(unionBeginning, unionEnd);

        var merged = verticalUnion
            ? new Rectangle(intersection.Value, union)
            : new Rectangle(union, intersection.Value);

        var area = merged.Area;
        if (area <= minRequiredArea)
        {
            return -1;
        }

        var insertedAt = AddSorted(merged);
        if (area > largestAddedArea)
        {
            largestAdded = merged;
            largestAddedArea = area;
        }

        return insertedAt;
    }

    /// <summary>
    ///     Inserts <paramref name="rectangle" /> into <see cref="_emptyRectangles" /> at the
    ///     position that preserves the descending-area ordering. Returns the inserted
    ///     position so callers iterating by index can adjust their cursors.
    /// </summary>
    private int AddSorted(Rectangle rectangle)
    {
        var area = rectangle.Area;
        var lo = 0;
        var hi = _emptyRectangles.Count;
        while (lo < hi)
        {
            var mid = (lo + hi) >>> 1;
            if (_emptyRectangles[mid].Area > area)
            {
                lo = mid + 1;
            }
            else
            {
                hi = mid;
            }
        }

        _emptyRectangles.Insert(lo, rectangle);
        return lo;
    }

    private void Prune()
    {
        for (var i = _emptyRectangles.Count - 1; i >= 0; i--)
        {
            var ri = _emptyRectangles[i];
            for (var j = 0; j < _emptyRectangles.Count; j++)
            {
                if (i == j)
                {
                    continue;
                }

                if (_emptyRectangles[j].Contains(ri))
                {
                    _emptyRectangles.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
