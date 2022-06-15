using System;
using UnicornHack.Primitives;

namespace UnicornHack.Utils.DataStructures;

public readonly struct Vector : IEquatable<Vector>
{
    public Vector(int x, int y)
    {
        X = x;
        Y = y;
    }

    public readonly int X;
    public readonly int Y;

    // Order matters, see Direction.cs

    public static readonly Vector[] MovementDirections =
    {
        new Vector(1, 0), new Vector(1, -1), new Vector(0, -1), new Vector(-1, -1), new Vector(-1, 0),
        new Vector(-1, 1), new Vector(0, 1), new Vector(1, 1)
    };

    public static readonly byte[] OppositeDirectionIndexes = { 4, 5, 6, 7, 0, 1, 2, 3 };

    public bool IsCardinalAligned() => X == 0 || Y == 0;

    public bool IsIntercardinalAligned() => X == Y || X == -Y;

    public Vector GetOrthogonal() => new Vector((sbyte)-Y, X);

    public Vector GetInverse() => new Vector((sbyte)-X, (sbyte)-Y);

    /// <summary>
    ///     Gets the closest grid vector of length 1
    /// </summary>
    public Vector GetUnit()
    {
        if (X == 0 && Y == 0)
        {
            return this;
        }

        switch (OctantsTo(Direction.East))
        {
            case 0:
                return new Vector(1, 0);
            case 1:
                return new Vector(1, 1);
            case 2:
                return new Vector(0, 1);
            case 3:
                return new Vector(-1, 1);
            case 4:
            case -4:
                return new Vector(-1, 0);
            case -3:
                return new Vector(-1, -1);
            case -2:
                return new Vector(0, -1);
            case -1:
                return new Vector(1, -1);
            default:
                throw new InvalidOperationException();
        }
    }

    public byte Length() => (byte)Math.Max(Math.Abs(X), Math.Abs(Y));

    public byte OrthogonalLength() => (byte)(Math.Abs(X) + Math.Abs(Y));

    public Direction AsDirection()
    {
        switch (X)
        {
            case -1:
                switch (Y)
                {
                    case -1:
                        return Direction.Northwest;
                    case 0:
                        return Direction.West;
                    case 1:
                        return Direction.Southwest;
                }

                break;
            case 0:
                switch (Y)
                {
                    case -1:
                        return Direction.North;
                    case 0:
                        throw new InvalidOperationException("Zero length vector");
                    case 1:
                        return Direction.South;
                }

                break;
            case 1:
                switch (Y)
                {
                    case -1:
                        return Direction.Northeast;
                    case 0:
                        return Direction.East;
                    case 1:
                        return Direction.Southeast;
                }

                break;
        }

        throw new InvalidOperationException("Not a cardinal direction: " + this);
    }

    /// <summary>
    ///     Get the vector octant (0-7)
    /// </summary>
    public int GetOctant()
    {
        if (Y < 0)
        {
            return X > 0
                ? X > -Y ? 0 : 1
                : -X < -Y
                    ? 2
                    : 3;
        }

        if (Y > 0)
        {
            return X < 0
                ? -X > Y ? 4 : 5
                : X < Y
                    ? 6
                    : 7;
        }

        if (X > 0)
        {
            return X > -Y ? 0 : 1;
        }

        if (X < 0)
        {
            return -X > Y ? 4 : 5;
        }

        throw new InvalidOperationException("Zero length vector");
    }

    public int OctantsTo(Direction direction)
    {
        var targetOctant = (int)direction;
        int octant;

        // Get the closest octant centered on a cardinal or intercardinal direction
        if (Y <= 0)
        {
            if (X > 0)
            {
                if (X >= -2 * Y)
                {
                    if (X != -2 * Y)
                    {
                        octant = 0;
                    }
                    else
                    {
                        octant = (targetOctant + 3) % 8 < 4 ? 0 : 1;
                    }
                }
                else if (2 * X >= -Y)
                {
                    if (2 * X != -Y)
                    {
                        octant = 1;
                    }
                    else
                    {
                        octant = (targetOctant + 2) % 8 < 4 ? 1 : 2;
                    }
                }
                else
                {
                    octant = 2;
                }
            }
            else
            {
                if (2 * X >= Y)
                {
                    if (2 * X != Y)
                    {
                        octant = 2;
                    }
                    else
                    {
                        octant = (targetOctant + 1) % 8 < 4 ? 2 : 3;
                    }
                }
                else if (X >= 2 * Y)
                {
                    if (X != 2 * Y)
                    {
                        octant = 3;
                    }
                    else
                    {
                        octant = targetOctant < 4 ? 3 : 4;
                    }
                }
                else
                {
                    octant = 4;
                }
            }
        }
        else
        {
            if (X < 0)
            {
                if (-X >= 2 * Y)
                {
                    if (-X != 2 * Y)
                    {
                        octant = 4;
                    }
                    else
                    {
                        octant = (targetOctant + 7) % 8 < 4 ? 4 : 5;
                    }
                }
                else if (-2 * X >= Y)
                {
                    if (-2 * X != Y)
                    {
                        octant = 5;
                    }
                    else
                    {
                        octant = (targetOctant + 6) % 8 < 4 ? 5 : 6;
                    }
                }
                else
                {
                    octant = 6;
                }
            }
            else
            {
                if (2 * X <= Y)
                {
                    if (2 * X != Y)
                    {
                        octant = 6;
                    }
                    else
                    {
                        octant = (targetOctant + 5) % 8 < 4 ? 6 : 7;
                    }
                }
                else if (X <= 2 * Y)
                {
                    if (X != 2 * Y)
                    {
                        octant = 7;
                    }
                    else
                    {
                        octant = (targetOctant + 4) % 8 < 4 ? 7 : 0;
                    }
                }
                else
                {
                    octant = 0;
                }
            }
        }

        var difference = targetOctant - octant;
        if (difference > 4)
        {
            difference -= 8;
        }
        else if (difference < -4)
        {
            difference += 8;
        }

        return difference;
    }

    public Vector ToOctant(int octant)
        => octant switch
        {
            0 => new Vector(X, -Y),
            1 => new Vector(-Y, X),
            2 => new Vector(-Y, -X),
            3 => new Vector(-X, -Y),
            4 => new Vector(-X, Y),
            5 => new Vector(Y, -X),
            6 => new Vector(Y, X),
            7 => new Vector(X, Y),
            _ => throw new InvalidOperationException()
        };

    public override string ToString() => $"<{X}, {Y}>";

    public override bool Equals(object obj)
    {
        if (obj is Vector otherVector)
        {
            return Equals(otherVector);
        }

        return false;
    }

    public bool Equals(Vector other) => X == other.X && Y == other.Y;

    public override int GetHashCode()
        => HashCode.Combine(X, Y);

    public static bool operator ==(Vector left, Vector right) => left.Equals(right);

    public static bool operator !=(Vector left, Vector right) => !(left == right);
}
