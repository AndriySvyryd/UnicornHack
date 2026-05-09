namespace UnicornHack.Primitives;

// Order matters
public enum Direction : byte
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
    Down
}

[Flags]
public enum DirectionFlags : byte
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
    NorthAndEast = North | East,
    NorthAndWest = North | West,
    SouthAndWest = South | West,
    SouthAndEast = South | East,
    Longitudinal = North | South,
    Latitudinal = West | East,
    Diagonal = Northeast | Southwest,
    AntiDiagonal = Northwest | Southeast,
    NorthEastWest = North | East | West,
    NorthEastSouth = North | East | South,
    NorthWestSouth = North | West | South,
    SouthEastWest = South | East | West,
    NorthwestCorner = West | Northwest | North,
    NortheastCorner = North | Northeast | East,
    SoutheastCorner = East | Southeast | South,
    SouthwestCorner = South | Southwest | West,
    Cross = Longitudinal | Latitudinal,
    DiagonalCross = Diagonal | AntiDiagonal,
    NorthSemicircle = NorthwestCorner | NortheastCorner,
    EastSemicircle = NortheastCorner | SoutheastCorner,
    SouthSemicircle = SoutheastCorner | SouthwestCorner,
    WestSemicircle = SouthwestCorner | NorthwestCorner,
    All = NorthSemicircle | SouthSemicircle
}

public static class DirectionExtensions
{
    public static Direction Rotate(this Direction direction, int octants)
    {
        var result = direction.AsOctants() + octants;
        if (result < 0)
        {
            result += 8;
        }
        else if (result > 7)
        {
            result -= 8;
        }

        return (Direction)result;
    }

    public static Vector AsVector(this Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                return new Vector(0, -1);
            case Direction.South:
                return new Vector(0, 1);
            case Direction.West:
                return new Vector(-1, 0);
            case Direction.East:
                return new Vector(1, 0);
            case Direction.Northwest:
                return new Vector(-1, -1);
            case Direction.Northeast:
                return new Vector(1, -1);
            case Direction.Southwest:
                return new Vector(-1, 1);
            case Direction.Southeast:
                return new Vector(1, 1);
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, message: null);
        }
    }

    public static int AsOctants(this Direction direction)
    {
        var result = (int)direction;
        if (result > 7)
        {
            throw new InvalidOperationException("Angleless direction");
        }

        return result;
    }

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

    /// <summary>
    ///     Returns whether the offset <c>(<paramref name="dx" />, <paramref name="dy" />)</c>
    ///     from a reference point lies in the half-plane (cardinal) or quadrant (ordinal)
    ///     corresponding to <paramref name="direction" />.
    ///     Tiles exactly on the axis (dx == 0 or dy == 0) are accepted by both adjacent
    ///     cardinal directions.
    /// </summary>
    public static bool IsInDirection(this Direction direction, int dx, int dy)
        => direction switch
        {
            Direction.East => dx >= 0,
            Direction.West => dx <= 0,
            Direction.North => dy <= 0,
            Direction.South => dy >= 0,
            Direction.Northeast => dx >= 0 && dy <= 0,
            Direction.Northwest => dx <= 0 && dy <= 0,
            Direction.Southeast => dx >= 0 && dy >= 0,
            Direction.Southwest => dx <= 0 && dy >= 0,
            _ => false
        };
}
