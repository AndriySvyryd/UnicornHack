using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using UnicornHack.Primitives;

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
            throw new ArgumentOutOfRangeException();
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

    public byte DistanceTo(Rectangle r) =>
        Math.Max(XProjection.DistanceTo(r.XProjection), YProjection.DistanceTo(YProjection));

    public byte DistanceTo(Point p) => Math.Max(XProjection.DistanceTo(p.X), YProjection.DistanceTo(p.Y));

    public byte OrthogonalDistanceTo(Rectangle r) =>
        (byte)(XProjection.DistanceTo(r.XProjection) + YProjection.DistanceTo(r.YProjection));

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

    public static (int[,], Point[]) GetPointIndex(IMemoryCache cache, byte width, byte height)
    {
        var key = nameof(GetPointIndex).GetHashCode() ^ (width << 8 + height);
        if (!cache.TryGetValue(key, out var result))
        {
            using var entry = cache.CreateEntry(key);
            var pointToIndex = new int[width, height];
            var indexToPoint = new Point[width * height];
            var i = 0;
            for (byte y = 0; y < height; y++)
            {
                for (byte x = 0; x < width; x++)
                {
                    pointToIndex[x, y] = i;
                    indexToPoint[i++] = new Point(x, y);
                }
            }

            result = (pointToIndex, indexToPoint);
            entry.SetValue(result);
        }

        return ((int[,], Point[]))result;
    }

    public override string ToString() => $"({TopLeft}, {BottomRight})";

    public bool Equals(Rectangle other)
        => TopLeft.Equals(other.TopLeft) && BottomRight.Equals(other.BottomRight);

    public override bool Equals(object obj)
        => obj is Rectangle other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(TopLeft, BottomRight);

    public static bool operator ==(Rectangle left, Rectangle right)
        => left.Equals(right);

    public static bool operator !=(Rectangle left, Rectangle right)
        => !left.Equals(right);
}
