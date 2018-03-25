using System;
using System.Diagnostics;

// ReSharper disable ImpureMethodCallOnReadonlyValueField
namespace UnicornHack.Utils.DataStructures
{
    public struct Point
    {
        public readonly byte X;
        public readonly byte Y;

        [DebuggerStepThrough]
        public Point(byte x, byte y)
        {
            X = x;
            Y = y;
        }

        public static Point? Unpack(int? bits)
            => bits == null ? (Point?)null : Unpack(bits.Value);

        public static Point Unpack(int bits)
            => new Point((byte)((bits & 0xFF00) >> 8), (byte)(bits & 0xFF));

        public static Point Unpack(ushort bits)
            => new Point((byte)((bits & 0xFF00) >> 8), (byte)(bits & 0xFF));

        public int ToInt32() => X << 8 | Y;

        public ushort ToUInt16() => (ushort)(X << 8 | Y);

        public byte DistanceTo(Point target) => (byte)Math.Max(Math.Abs(target.X - X), Math.Abs(target.Y - Y));

        public byte OrthogonalDistanceTo(Point target) => (byte)(Math.Abs(target.X - X) + Math.Abs(target.Y - Y));

        public Vector DifferenceTo(Point target) => new Vector((sbyte)(target.X - X), (sbyte)(target.Y - Y));

        public Point Translate(Vector direction) => new Point((byte)(X + direction.X), (byte)(Y + direction.Y));

        public void Deconstruct(out byte x, out byte y)
        {
            x = X;
            y = Y;
        }

        public override string ToString() => $"({X}, {Y})";

        public override bool Equals(object obj)
        {
            if (obj is Point otherPoint)
            {
                return Equals(otherPoint);
            }

            return false;
        }

        public bool Equals(Point other) => X == other.X && Y == other.Y;

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        public static bool operator ==(Point left, Point right) => left.Equals(right);

        public static bool operator !=(Point left, Point right) => !(left == right);
    }
}
