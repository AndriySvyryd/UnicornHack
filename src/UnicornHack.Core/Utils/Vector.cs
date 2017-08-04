using System;

namespace UnicornHack.Utils
{
    public struct Vector
    {
        public Vector(sbyte x, sbyte y)
        {
            X = x;
            Y = y;
        }

        public readonly sbyte X;
        public readonly sbyte Y;

        public Vector GetOrthogonal()
            => new Vector((sbyte)-Y, X);

        public Vector GetInverse()
            => new Vector((sbyte)-X, (sbyte)-Y);

        public Vector GetUnit()
            => new Vector(X == 0 ? X : (sbyte)(X / Math.Abs(X)), Y == 0 ? Y : (sbyte)(Y / Math.Abs(Y)));

        public byte Length()
            => (byte)Math.Max(Math.Abs(X), Math.Abs(Y));

        public byte OrthogonalLength()
            => (byte)(Math.Abs(X) + Math.Abs(Y));

        public override string ToString() => $"{{{X}, {Y}}}";

        public static Vector Convert(Direction direction)
        {
            switch (direction)
            {
                case Direction.None:
                    return new Vector(x: 0, y: 0);
                case Direction.North:
                    return new Vector(x: 0, y: -1);
                case Direction.South:
                    return new Vector(x: 0, y: 1);
                case Direction.West:
                    return new Vector(x: -1, y: 0);
                case Direction.East:
                    return new Vector(x: 1, y: 0);
                case Direction.Northwest:
                    return new Vector(x: -1, y: -1);
                case Direction.Northeast:
                    return new Vector(x: 1, y: -1);
                case Direction.Southwest:
                    return new Vector(x: -1, y: 1);
                case Direction.Southeast:
                    return new Vector(x: 1, y: 1);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, message: null);
            }
        }
    }
}