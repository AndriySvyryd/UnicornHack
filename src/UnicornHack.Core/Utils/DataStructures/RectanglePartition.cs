using System.Buffers;

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
public sealed class RectanglePartition : IDisposable
{
    private readonly List<Rectangle> _emptyRectangles;

    // Per-cell empty-space raster covering the bounding rectangle, rented from
    // <see cref="ArrayPool{T}.Shared" /> and returned by <see cref="Dispose" />.
    // Row-major: index = (y - _originY) * _width + (x - _originX). <c>true</c> means
    // the cell is still part of the empty region. Initialised to all-true at
    // construction and flipped to <c>false</c> by <see cref="MarkOccupied" /> for
    // every cell inside an inserted obstacle. The partition has no Remove operation,
    // so a cell that becomes occupied stays occupied for the partition's lifetime
    // — that's what makes the raster sufficient (no per-cell ref-counting needed)
    // and what makes <see cref="Contains(Point)" /> an O(1) array lookup instead of
    // a tree query.
    private bool[] _emptyRaster;
    private readonly int _originX;
    private readonly int _originY;
    private readonly int _width;
    private readonly int _height;

    public RectanglePartition(Rectangle bounds)
    {
        _emptyRectangles = [bounds];
        _originX = bounds.TopLeft.X;
        _originY = bounds.TopLeft.Y;
        _width = bounds.Width;
        _height = bounds.Height;
        var length = _width * _height;
        _emptyRaster = ArrayPool<bool>.Shared.Rent(length);
        // Rent() may hand back a larger buffer; only the cells we'll index need to
        // start true. The pool doesn't clear on rent.
        Array.Fill(_emptyRaster, true, 0, length);
    }

    /// <summary>
    ///     The current cover of the empty region, sorted by area in descending order.
    /// </summary>
    public IReadOnlyList<Rectangle> EmptyRectangles => _emptyRectangles;

    /// <summary>
    ///     Returns <c>true</c> when <paramref name="point" /> lies in the empty region —
    ///     i.e. when no inserted obstacle has claimed the cell. O(1) raster lookup.
    /// </summary>
    public bool Contains(Point point)
    {
        // Single unsigned-cast bounds check: rolls the lower (>= 0) and upper (< _width)
        // comparisons into one branch. A negative `point.X - _originX` wraps to a huge
        // unsigned value that's guaranteed to fail the < _width comparison.
        var localX = (uint)(point.X - _originX);
        var localY = (uint)(point.Y - _originY);
        if (localX >= (uint)_width || localY >= (uint)_height)
        {
            return false;
        }
        return _emptyRaster[(int)((localY * (uint)_width) + localX)];
    }

    /// <summary>
    ///     Removes <paramref name="obstacle" /> from the empty region. Every empty rectangle
    ///     overlapping the obstacle is replaced by up to four side strips that lie outside
    ///     the obstacle but inside the original empty rectangle.
    /// </summary>
    public void Insert(Rectangle obstacle)
    {
        // Mark the obstacle's cells as occupied in the raster. Doing this once up-front
        // (instead of incrementally per split strip) is cheap and matches the
        // partition's "no Remove" contract — once occupied, always occupied.
        MarkOccupied(obstacle);

        for (var i = _emptyRectangles.Count - 1; i >= 0; i--)
        {
            var empty = _emptyRectangles[i];
            if (!empty.Overlaps(obstacle))
            {
                continue;
            }

            RemoveAt(i);

            // Left strip: portion of empty strictly left of the obstacle.
            if (empty.TopLeft.X < obstacle.TopLeft.X)
            {
                AddSortedAndPrune(new Rectangle(
                    empty.TopLeft,
                    new Point((byte)(obstacle.TopLeft.X - 1), empty.BottomRight.Y)));
            }

            // Right strip.
            if (empty.BottomRight.X > obstacle.BottomRight.X)
            {
                AddSortedAndPrune(new Rectangle(
                    new Point((byte)(obstacle.BottomRight.X + 1), empty.TopLeft.Y),
                    empty.BottomRight));
            }

            // Top strip (smaller Y).
            if (empty.TopLeft.Y < obstacle.TopLeft.Y)
            {
                AddSortedAndPrune(new Rectangle(
                    empty.TopLeft,
                    new Point(empty.BottomRight.X, (byte)(obstacle.TopLeft.Y - 1))));
            }

            // Bottom strip.
            if (empty.BottomRight.Y > obstacle.BottomRight.Y)
            {
                AddSortedAndPrune(new Rectangle(
                    new Point(empty.TopLeft.X, (byte)(obstacle.BottomRight.Y + 1)),
                    empty.BottomRight));
            }
        }
    }

    private void MarkOccupied(Rectangle obstacle)
    {
        var xEndBound = _originX + _width - 1;
        var yEndBound = _originY + _height - 1;
        var obstacleTopLeft = obstacle.TopLeft;
        var obstacleBottomRight = obstacle.BottomRight;
        var xStart = obstacleTopLeft.X > _originX ? obstacleTopLeft.X : _originX;
        var xEnd = obstacleBottomRight.X < xEndBound ? obstacleBottomRight.X : xEndBound;
        var yStart = obstacleTopLeft.Y > _originY ? obstacleTopLeft.Y : _originY;
        var yEnd = obstacleBottomRight.Y < yEndBound ? obstacleBottomRight.Y : yEndBound;
        var raster = _emptyRaster;
        var width = _width;
        for (var y = yStart; y <= yEnd; y++)
        {
            var rowOffset = (y - _originY) * width;
            for (var x = xStart; x <= xEnd; x++)
            {
                raster[rowOffset + (x - _originX)] = false;
            }
        }
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
        var lo = BinarySearchInsertPosition(rectangle.Area);
        _emptyRectangles.Insert(lo, rectangle);
        return lo;
    }

    /// <summary>
    ///     <see cref="AddSorted" /> with single-pass containment pruning folded in: if
    ///     <paramref name="rectangle" /> is already covered by some existing empty,
    ///     skips the insertion and returns <c>-1</c>; otherwise drops any existing
    ///     rectangle that <paramref name="rectangle" /> fully covers before inserting.
    ///     Used by <see cref="Insert" /> so each strip add re-establishes the
    ///     "no rect contains another" invariant locally and we don't need a separate
    ///     <see cref="Prune" /> sweep over the whole partition afterwards.
    /// </summary>
    /// <remarks>
    ///     Safe to call from <see cref="Insert" />'s top-down loop: the new strip is a
    ///     subset of the empty being split, so the invariant guarantees the strip can
    ///     only ever subsume strips added earlier in the same <see cref="Insert" /> call
    ///     — those sit at indices &gt; <c>i</c>, leaving the outer cursor undisturbed.
    ///     Not used by <see cref="Recalculate" /> because a merged rect there can subsume
    ///     <c>a</c> or <c>b</c> at the cursor positions; <see cref="Recalculate" />
    ///     keeps the trailing <see cref="Prune" /> pass instead.
    /// </remarks>
    private int AddSortedAndPrune(Rectangle rectangle)
    {
        // Single top-down scan: bail if some existing entry already covers `rectangle`;
        // otherwise drop any entry `rectangle` covers. The two outcomes are mutually
        // exclusive (a strict-set containment relation), so the loop's effect is either
        // "skip insert" or "insert after pruning".
        for (var i = _emptyRectangles.Count - 1; i >= 0; i--)
        {
            var existing = _emptyRectangles[i];
            if (existing.Contains(rectangle))
            {
                return -1;
            }
            if (rectangle.Contains(existing))
            {
                _emptyRectangles.RemoveAt(i);
            }
        }

        var lo = BinarySearchInsertPosition(rectangle.Area);
        _emptyRectangles.Insert(lo, rectangle);
        return lo;
    }

    private int BinarySearchInsertPosition(int area)
    {
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
        return lo;
    }

    private void RemoveAt(int index)
    {
        _emptyRectangles.RemoveAt(index);
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
                    RemoveAt(i);
                    break;
                }
            }
        }
    }

    /// <summary>
    ///     Returns the rented <see cref="_emptyRaster" /> buffer to
    ///     <see cref="ArrayPool{T}.Shared" />. Idempotent: a second call is a no-op.
    ///     Calling any other instance method after <see cref="Dispose" /> is undefined
    ///     (the raster lookups will throw <see cref="NullReferenceException" />).
    /// </summary>
    public void Dispose()
    {
        var buffer = _emptyRaster;
        if (buffer is null)
        {
            return;
        }
        _emptyRaster = null!;
        ArrayPool<bool>.Shared.Return(buffer, clearArray: false);
    }
}
