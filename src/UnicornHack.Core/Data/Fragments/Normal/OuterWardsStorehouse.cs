namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment OuterWardsStorehouse = new NormalMapFragment
    {
        Name = "outer wards storehouse",
        GenerationWeight = "$branch == 'outer wards' ? 3.0 / (1 + $instances) : 0",
        NoTransform = true,
        Map = @"
####+####
#.......#
#.#...#.#
+.......+
#.#...#.#
#.......#
####+####",
        Tags = new HashSet<string> { "!trapped", "!cursed" },
        NoRandomDoorways = true
    };
}
