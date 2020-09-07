namespace UnicornHack.Primitives
{
    public enum TargetingShape
    {
        // Ends on target, width size*2 - 1.
        Line,

        // Goes through the target, length is determined by size.
        Beam,

        // Orientation is determined by the closest cardinal direction.

        // 2 1-width lines with lengths determined by size.
        DoubleBeam,

        // Size between 1-4 determines the number of quadrants covered.
        Cone,

        // 2 1-width lines with lengths determined by size. Excludes center cell.
        PerpendicularBeamNoCenter,

        // 2 1-width lines with lengths determined by size.
        PerpendicularBeam,

        // 3 1-width lines with lengths determined by size. Excludes center cell.
        TeeNoCenter,

        // 3 1-width lines with lengths determined by size.
        Tee,

        // 4 1-width lines with lengths determined by size. Excludes center cell.
        PlusNoCenter,

        // 4 1-width lines with lengths determined by size.
        Plus,

        // A square with side of size*2 - 1, centered on the target cell. Excludes center cell.
        SquareNoCenter,

        // A square with side of size*2 - 1, centered on the target cell.
        Square
    }
}
