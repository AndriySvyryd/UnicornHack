namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    // 7x7 hall with four pillars at the inner-square corners and a ~25% trap chance on
    // the centre tile (classic DCSS / Brogue layout).
    public static readonly NormalMapFragment PillaredHall = new NormalMapFragment
    {
        Name = "pillaredHall",
        GenerationWeight = "10.0 / ((2 + $instances) >> 1)",
        NoTransform = true,
        NoRandomDoorways = true,
        Annotations = new Dictionary<char, string> { ['1'] = "...^" },
        Map = @"
###+###
#.....#
#.#.#.#
+..1..+
#.#.#.#
#.....#
###+###"
    };
}
