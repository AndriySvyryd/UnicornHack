namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    // 5x7 narrow passage with a column of ~25% trap candidates running between the two
    // end doorways; expected ~1.25 traps along the route.
    public static readonly NormalMapFragment TrappedHallway = new NormalMapFragment
    {
        Name = "trappedHallway",
        GenerationWeight = "5.0 / ((2 + $instances) >> 1)",
        NoMirror = true,
        NoRandomDoorways = true,
        Annotations = new Dictionary<char, string> { ['1'] = "...^" },
        Map = @"
##+##
#.1.#
#.1.#
#.1.#
#.1.#
#.1.#
##+##"
    };
}
