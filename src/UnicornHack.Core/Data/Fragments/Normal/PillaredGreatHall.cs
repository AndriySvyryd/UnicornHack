namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment PillaredGreatHall = new NormalMapFragment
    {
        Name = "pillaredGreatHall",
        GenerationWeight = "4.0 / ((2 + $instances) >> 1)",
        NoTransform = true,
        Map = @"
####+####
#.......#
#..#.#..#
#.......#
+...%...+
#.......#
#..#.#..#
#.......#
####+####",
        NoRandomDoorways = true
    };
}
