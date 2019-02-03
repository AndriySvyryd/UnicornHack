namespace UnicornHack.Primitives
{
    public enum TargetingShape
    {
        // 1 cell at any range
        Line,

        // 3 cell at any range
        ThreeLine,

        // Starts at 1 cell and increases by 2 each cell away
        OneOctant,

        // Starts at 3 cells and increases by 2 each cell away
        ThreeOctants,

        // Starts at 5 cells and increases by 4 each cell away
        FiveOctants,

        // Starts at 8 cells and increases by 8 each cell away
        Omnidirectional,

        // The shapes below can be applied in any direction

        // A single cell
        OneSquare,

        // A square with side of 3 cells centered on the target cell
        ThreeSquare,

        // A square with side of 5 cells centered on the target cell
        FiveSquare,

        // A square with side of 7 cells centered on the target cell
        SevenSquare
    }
}
