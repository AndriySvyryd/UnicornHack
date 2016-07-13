using System;
using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Utils
{
    public struct Vector
    {
        public Vector(sbyte x, sbyte y, sbyte z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public sbyte X { get; }
        public sbyte Y { get; }
        public sbyte Z { get; }

        public static Vector Convert(Direction direction)
        {
            switch (direction)
            {
                case Direction.None:
                    return new Vector(x: 0, y: 0, z: 0);
                case Direction.North:
                    return new Vector(x: 0, y: -1, z: 0);
                case Direction.South:
                    return new Vector(x: 0, y: 1, z: 0);
                case Direction.West:
                    return new Vector(-1, y: 0, z: 0);
                case Direction.East:
                    return new Vector(x: 1, y: 0, z: 0);
                case Direction.Northwest:
                    return new Vector(-1, -1, z: 0);
                case Direction.Northeast:
                    return new Vector(x: 1, y: -1, z: 0);
                case Direction.Southwest:
                    return new Vector(-1, y: 1, z: 0);
                case Direction.Southeast:
                    return new Vector(x: 1, y: 1, z: 0);
                case Direction.Up:
                    return new Vector(x: 0, y: 0, z: -1);
                case Direction.Down:
                    return new Vector(x: 0, y: 0, z: 1);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, message: null);
            }
        }
    }
}