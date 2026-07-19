namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment TrappedHallway = new NormalMapFragment
    {
        Name = "trappedHallway",
        GenerationWeight = "5.0 / ((2 + $instances) >> 1)",
        NoMirror = true,
        Map = @"
##+##
#.1.#
#.1.#
#.1.#
#.1.#
#.1.#
##+##",
        Annotations = new Dictionary<char, string> { { '1', "...^" } },
        NoRandomDoorways = true
    };
}
