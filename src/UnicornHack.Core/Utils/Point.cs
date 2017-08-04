using System;
using System.Diagnostics;

namespace UnicornHack.Utils
{
    //[StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Point
    {
        [DebuggerStepThrough]
        public Point(byte x, byte y)
        {
            X = x;
            Y = y;
        }

        public readonly byte X;
        public readonly byte Y;

        public byte DistanceTo(Point target)
            => (byte)Math.Max(Math.Abs(target.X - X), Math.Abs(target.Y - Y));

        public byte OrthogonalDistanceTo(Point target)
            => (byte)(Math.Abs(target.X - X) + Math.Abs(target.Y - Y));

        public Vector DirectionTo(Point target)
            => new Vector((sbyte)(target.X - X), (sbyte)(target.Y - Y));

        public Point Translate(Vector direction)
            => new Point((byte)(X + direction.X), (byte)(Y + direction.Y));

        public override string ToString() => $"({X}, {Y})";

        public override bool Equals(object obj)
        {
            if (obj is Point otherPoint)
            {
                return Equals(otherPoint);
            }

            return false;
        }

        public bool Equals(Point other)
        {
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }
    }
}