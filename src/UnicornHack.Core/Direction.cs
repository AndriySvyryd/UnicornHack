using System;

namespace UnicornHack
{
    // Order matters
    public enum Direction
    {
        North = 0,
        East,
        South,
        West,
        Northwest,
        Northeast,
        Southeast,
        Southwest,
        Up,
        Down,
        None
    }

    [Flags]
    public enum DirectionFlags
    {
        None = 0,
        North = 1 << 0,
        East = 1 << 1,
        South = 1 << 2,
        West = 1 << 3,
        Northwest = 1 << 4,
        Northeast = 1 << 5,
        Southeast = 1 << 6,
        Southwest = 1 << 7,
        Up = 1 << 8,
        Down = 1 << 9,
        Center = 1 << 10,
        NorthAndWest = North | West,
        NorthAndEast = North | East,
        SouthAndEast = South | East,
        SouthAndWest = South | West,
        NorthwestCorner = West | Northwest | North,
        NortheastCorner = North | Northeast | East,
        SoutheastCorner = East | Southeast | South,
        SouthwestCorner = South | Southwest | West,
        NorthSemicircle = NorthwestCorner | NortheastCorner,
        EastSemicircle = NortheastCorner | SoutheastCorner,
        SouthSemicircle = SoutheastCorner | SouthwestCorner,
        WestSemicircle = SouthwestCorner | NorthwestCorner,
        Circle = NorthSemicircle | SouthSemicircle
    }

    public static class DirectionExtentions
    {
        public static int OctantsTo(this Direction direction, Direction otherDirection)
        {
            var firstDegrees = ToHorizontalDegrees(direction);
            if (firstDegrees == -1)
            {
                return -1;
            }

            var secondDegrees = ToHorizontalDegrees(otherDirection);
            if (secondDegrees == -1)
            {
                return -1;
            }

            var difference = firstDegrees - secondDegrees;
            if (difference < 0)
            {
                difference = 360 + difference;
            }
            return difference / 45;
        }

        public static int ClosestOctantsTo(this Direction direction, Direction otherDirection)
        {
            var octants = direction.OctantsTo(otherDirection);
            if (octants == -1)
            {
                return -1;
            }

            return octants > 4 ? 8 - octants : octants;
        }

        public static int ToHorizontalDegrees(this Direction direction)
        {
            switch (direction)
            {
                case Direction.East:
                    return 0;
                case Direction.Southeast:
                    return 45;
                case Direction.South:
                    return 90;
                case Direction.Southwest:
                    return 135;
                case Direction.West:
                    return 180;
                case Direction.Northwest:
                    return 225;
                case Direction.North:
                    return 270;
                case Direction.Northeast:
                    return 315;
                case Direction.Up:
                case Direction.Down:
                case Direction.None:
                default:
                    return -1;
            }
        }
    }
}