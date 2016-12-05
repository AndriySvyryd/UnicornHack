using System;

namespace UnicornHack.Utils
{
    //[StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Point
    {
        public Point(byte x, byte y)
        {
            X = x;
            Y = y;
        }

        public byte X;
        public byte Y;

        // Using 8-directional movement
        public byte DistanceTo(Point target)
            => (byte)Math.Max(Math.Abs(target.X - X), Math.Abs(target.Y - Y));

        public Vector DirectionTo(Point target)
            => new Vector((sbyte)(target.X - X), (sbyte)(target.Y - Y));
    }
}