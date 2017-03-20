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
}