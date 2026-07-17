using System.Collections;
using System.Collections.Concurrent;

namespace UnicornHack.Utils.DataStructures;

/// <summary>
///     Represents an inclusive axis-aligned rectangle
/// </summary>
public readonly struct Rectangle : IEnumerable<Point>, IEquatable<Rectangle>
{
    public Rectangle(Point topLeft, Point bottomRight)
    {
        if (topLeft.X > bottomRight.X || topLeft.Y > bottomRight.Y)
        {
            throw new ArgumentOutOfRangeException(topLeft + " " + bottomRight);
        }

        TopLeft = topLeft;
        BottomRight = bottomRight;
    }

    public Rectangle(Point topLeft, byte width, byte height)
    {
        if (width == 0 || height == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }

        TopLeft = topLeft;
        BottomRight = new Point((byte)(topLeft.X + width - 1), (byte)(topLeft.Y + height - 1));
    }

    public Rectangle(Point topLeft, Dimensions size) : this(topLeft, size.Width, size.Height)
    {
    }

    public Rectangle(Segment xProjection, Segment yProjection)
    {
        TopLeft = new Point(xProjection.Beginning, yProjection.Beginning);
        BottomRight = new Point(xProjection.End, yProjection.End);
    }

    public readonly Point TopLeft;
    public readonly Point BottomRight;
    public Point TopRight => new(BottomRight.X, TopLeft.Y);
    public Point BottomLeft => new(TopLeft.X, BottomRight.Y);
    public byte Width => (byte)(BottomRight.X - TopLeft.X + 1);
    public byte Height => (byte)(BottomRight.Y - TopLeft.Y + 1);
    public Dimensions Dimensions => new(Width, Height);
    public Segment XProjection => new(TopLeft.X, BottomRight.X);
    public Segment YProjection => new(TopLeft.Y, BottomRight.Y);
    public int Area => Width * Height;

    public Rectangle? Intersection(Rectangle r)
    {
        var xIntersection = XProjection.Intersection(r.XProjection);
        if (xIntersection == null)
        {
            return null;
        }

        var yIntersection = YProjection.Intersection(r.YProjection);
        if (yIntersection == null)
        {
            return null;
        }

        return new Rectangle(xIntersection.Value, yIntersection.Value);
    }

    public bool Overlaps(Rectangle r) => Intersection(r) != null;

    public bool Encloses(Rectangle r) => XProjection.Encloses(r.XProjection) && YProjection.Encloses(r.YProjection);

    public bool Contains(Rectangle r) => XProjection.Contains(r.XProjection) && YProjection.Contains(r.YProjection);

    public bool Contains(Point p) => XProjection.Contains(p.X) && YProjection.Contains(p.Y);

    public bool Contains(Dimensions d) => Dimensions.Contains(d);

    public byte DistanceTo(Rectangle r)
        => Math.Max(XProjection.DistanceTo(r.XProjection), YProjection.DistanceTo(r.YProjection));

    public byte DistanceTo(Point p) => Math.Max(XProjection.DistanceTo(p.X), YProjection.DistanceTo(p.Y));

    public byte OrthogonalDistanceTo(Rectangle r)
        => (byte)(XProjection.DistanceTo(r.XProjection) + YProjection.DistanceTo(r.YProjection));

    public byte OrthogonalDistanceTo(Point p) => (byte)(XProjection.DistanceTo(p.X) + YProjection.DistanceTo(p.Y));

    public IEnumerator<Point> GetEnumerator()
    {
        for (var y = TopLeft.Y; y <= BottomRight.Y; y++)
        {
            for (var x = TopLeft.X; x <= BottomRight.X; x++)
            {
                yield return new Point(x, y);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<Point> GetInside()
    {
        if (Width < 3 || Height < 3)
        {
            yield break;
        }

        for (var y = (byte)(TopLeft.Y + 1); y < BottomRight.Y; y++)
        {
            for (var x = (byte)(TopLeft.X + 1); x < BottomRight.X; x++)
            {
                yield return new Point(x, y);
            }
        }
    }

    public IEnumerable<Point> GetPerimeter()
    {
        var currentX = TopLeft.X;
        var currentY = TopLeft.Y;
        var direction = Direction.East;
        do
        {
            switch (direction)
            {
                case Direction.East:
                    if (currentX == BottomRight.X)
                    {
                        if (currentY == BottomRight.Y)
                        {
                            // 1-height rectangle
                            yield return new Point(currentX, currentY);
                            yield break;
                        }

                        direction = Direction.South;
                        break;
                    }

                    yield return new Point(currentX++, currentY);
                    break;
                case Direction.South:
                    if (currentY == BottomRight.Y)
                    {
                        if (currentX == TopLeft.X)
                        {
                            yield return new Point(currentX, currentY);
                            yield break;
                        }

                        direction = Direction.West;
                        break;
                    }

                    yield return new Point(currentX, currentY++);
                    break;
                case Direction.West:
                    if (currentX == TopLeft.X)
                    {
                        direction = Direction.North;
                        break;
                    }

                    yield return new Point(currentX--, currentY);
                    break;
                case Direction.North:
                    if (currentY == TopLeft.Y)
                    {
                        yield break;
                    }

                    yield return new Point(currentX, currentY--);
                    break;
            }
        } while (true);
    }

    public Point? PlaceInside(Rectangle rectangleToPlace, SimpleRandom random)
    {
        if (rectangleToPlace.Width > Width || rectangleToPlace.Height > Height)
        {
            return null;
        }

        var xOffset = random.Next(Width - rectangleToPlace.Width + 1);
        var yOffset = random.Next(Height - rectangleToPlace.Height + 1);

        return new Point((byte)(TopLeft.X + xOffset), (byte)(TopLeft.Y + yOffset));
    }

    public static Rectangle CreateRandom(SimpleRandom random, Rectangle boundingRectangle) =>
        CreateRandom(random, boundingRectangle, new Dimensions(1, 1));

    public static Rectangle CreateRandom(SimpleRandom random, Rectangle boundingRectangle, Dimensions minSize)
    {
        var width = (byte)random.Next(minSize.Width, boundingRectangle.Width + 1);
        var x1 = (byte)random.Next(boundingRectangle.TopLeft.X, boundingRectangle.BottomRight.X + 2 - width);

        var height = (byte)random.Next(minSize.Height, boundingRectangle.Height + 1);
        var y1 = (byte)random.Next(boundingRectangle.TopLeft.Y, boundingRectangle.BottomRight.Y + 2 - height);

        return new Rectangle(new Point(x1, y1), width, height);
    }

    /// <summary>
    ///     Returns a random rectangle inside <paramref name="boundingRectangle" /> whose
    ///     position is biased toward one of the four edges supplied in
    ///     <paramref name="preferredEdges" /> instead of picking a side randomly. When
    ///     both edges of an axis are set the side is still chosen randomly (the result is
    ///     equivalent to the no-preference call for that axis); when neither is set the
    ///     axis behaves like the random-bias overload. <paramref name="flushEdges" /> is
    ///     stricter than bias: a cardinal set in <paramref name="flushEdges" /> forces the
    ///     carved rectangle to be flush against that edge of
    ///     <paramref name="boundingRectangle" /> (i.e. the corresponding coordinate is
    ///     pinned, not just biased). Used by dynamic placers
    ///     (<see cref="DynamicMap.GetRoomPoints" />) when the layout has committed the
    ///     fragment to overlap an existing chunk on that edge.
    /// </summary>
    public static Rectangle CreateRandomNearEdge(
        SimpleRandom random, Rectangle boundingRectangle, Dimensions minSize,
        DirectionFlags preferredEdges, DirectionFlags flushEdges)
    {
        var width = (byte)random.Next(minSize.Width, boundingRectangle.Width + 1);
        var height = (byte)random.Next(minSize.Height, boundingRectangle.Height + 1);

        var preferWest = (preferredEdges & DirectionFlags.West) == DirectionFlags.West;
        var preferEast = (preferredEdges & DirectionFlags.East) == DirectionFlags.East;
        var preferNorth = (preferredEdges & DirectionFlags.North) == DirectionFlags.North;
        var preferSouth = (preferredEdges & DirectionFlags.South) == DirectionFlags.South;

        var flushWest = (flushEdges & DirectionFlags.West) == DirectionFlags.West;
        var flushEast = (flushEdges & DirectionFlags.East) == DirectionFlags.East;
        var flushNorth = (flushEdges & DirectionFlags.North) == DirectionFlags.North;
        var flushSouth = (flushEdges & DirectionFlags.South) == DirectionFlags.South;

        byte x1;
        if (flushWest)
        {
            x1 = boundingRectangle.TopLeft.X;
        }
        else if (flushEast)
        {
            x1 = (byte)(boundingRectangle.BottomRight.X + 1 - width);
        }
        else
        {
            x1 = (byte)PickBiasedTowardEdge(
                random, boundingRectangle.TopLeft.X, boundingRectangle.BottomRight.X + 1 - width,
                preferMin: preferWest, preferMax: preferEast);
        }

        byte y1;
        if (flushNorth)
        {
            y1 = boundingRectangle.TopLeft.Y;
        }
        else if (flushSouth)
        {
            y1 = (byte)(boundingRectangle.BottomRight.Y + 1 - height);
        }
        else
        {
            y1 = (byte)PickBiasedTowardEdge(
                random, boundingRectangle.TopLeft.Y, boundingRectangle.BottomRight.Y + 1 - height,
                preferMin: preferNorth, preferMax: preferSouth);
        }

        return new Rectangle(new Point(x1, y1), width, height);
    }

    // Picks an integer in [min, max] biased toward either min or max. When both
    // preferMin and preferMax are false the side is chosen at random (legacy random-bias
    // behaviour); when exactly one is true the bias is forced toward that side; when both
    // are true the side is also random (callers can't favour both ends of one axis at once).
    private static int PickBiasedTowardEdge(SimpleRandom random, int min, int max,
        bool preferMin = false, bool preferMax = false)
    {
        if (min >= max)
        {
            return min;
        }

        var a = random.Next(min, max + 1);
        var b = random.Next(min, max + 1);
        var biasToMin = preferMin == preferMax
            ? random.NextSingle(0f, 1f) < 0.5f
            : preferMin;
        return biasToMin ? Math.Min(a, b) : Math.Max(a, b);
    }

    /// <summary>
    ///     Returns memoized <c>(pointToIndex, indexToPoint)</c> tables for a rectangle of
    ///     the given dimensions. Tables are cached in a process-wide
    ///     <see cref="ConcurrentDictionary{TKey,TValue}" /> keyed by <c>(width &lt;&lt; 8) | height</c>,
    ///     so callers can rely on reference equality for the same dimensions across the lifetime of
    ///     the process. The cached tables are immutable after construction, so sharing them across
    ///     threads is safe; <see cref="ConcurrentDictionary{TKey,TValue}.GetOrAdd(TKey,System.Func{TKey,TValue})" />
    ///     keeps lookups lock-free on this hot generation path. A benign race may build the same
    ///     tables twice on first touch, but only one instance is ever published.
    /// </summary>
    public static (short[,], Point[]) GetPointIndex(byte width, byte height)
    {
        var key = ((uint)width << 8) | height;
        return _staticPointIndexCache.GetOrAdd(key, static k => BuildPointIndex((byte)(k >> 8), (byte)k));
    }

    private static readonly ConcurrentDictionary<uint, (short[,], Point[])> _staticPointIndexCache = new();

    private static (short[,], Point[]) BuildPointIndex(byte width, byte height)
    {
        var pointToIndex = new short[width, height];
        var indexToPoint = new Point[width * height];
        short i = 0;
        for (byte y = 0; y < height; y++)
        {
            for (byte x = 0; x < width; x++)
            {
                pointToIndex[x, y] = i;
                indexToPoint[i++] = new Point(x, y);
            }
        }

        return (pointToIndex, indexToPoint);
    }

    public override string ToString() => $"({TopLeft}, {BottomRight})";

    public bool Equals(Rectangle other)
        => TopLeft.Equals(other.TopLeft) && BottomRight.Equals(other.BottomRight);

    public override bool Equals(object? obj)
        => obj is Rectangle other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(TopLeft, BottomRight);

    public static bool operator ==(Rectangle left, Rectangle right)
        => left.Equals(right);

    public static bool operator !=(Rectangle left, Rectangle right)
        => !left.Equals(right);
}
