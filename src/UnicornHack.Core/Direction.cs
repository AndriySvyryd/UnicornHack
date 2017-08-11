using System;

namespace UnicornHack
{
    // Order matters
    public enum Direction
    {
        East = 0,
        Northeast,
        North,
        Northwest,
        West,
        Southwest,
        South,
        Southeast,
        Up,
        Down,
        None
    }

    [Flags]
    public enum DirectionFlags
    {
        None = 0,
        East = 1 << 0,
        Northeast = 1 << 1,
        North = 1 << 2,
        Northwest = 1 << 3,
        West = 1 << 4,
        Southwest = 1 << 5,
        South = 1 << 6,
        Southeast = 1 << 7,
        Up = 1 << 8,
        Down = 1 << 9,
        Center = 1 << 10,
        NorthAndEast = North | East,
        NorthAndWest = North | West,
        SouthAndWest = South | West,
        SouthAndEast = South | East,
        Longitudinal = North | South,
        Latitudinal = West | East,
        Diagonal = Northeast | Southwest,
        BackwardsDiagonal = Northwest | Southeast,
        NorthEastWest = North | East | West,
        NorthEastSouth = North | East | South,
        NorthWestSouth = North | West | South,
        SouthEastWest = South | East | West,
        NorthwestCorner = West | Northwest | North,
        NortheastCorner = North | Northeast | East,
        SoutheastCorner = East | Southeast | South,
        SouthwestCorner = South | Southwest | West,
        Cross = Longitudinal | Latitudinal,
        DiagonalCross = Diagonal | BackwardsDiagonal,
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
            var difference = (int)otherDirection - (int)direction;
            return difference < 0 ? 8 + difference : difference;
        }

        public static int ClosestOctantsTo(this Direction direction, Direction otherDirection)
        {
            var difference = Math.Abs((int)otherDirection - (int)direction);
            return difference > 4 ? 8 - difference : difference;
        }
    }
}