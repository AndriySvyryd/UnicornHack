namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    // 9x9 pillared great hall: four symmetric pillars framing a central creature wildcard,
    // with a doorway centred on every side. Fully symmetric, so it collapses to a single
    // variant. A large size-large chunk offering cover around a guarded centre.
    public static readonly NormalMapFragment PillaredGreatHall = new NormalMapFragment
    {
        Name = "pillaredGreatHall",
        GenerationWeight = "4.0 / ((2 + $instances) >> 1)",
        NoTransform = true,
        NoRandomDoorways = true,
        Map = @"
####+####
#.......#
#..#.#..#
#.......#
+...%...+
#.......#
#..#.#..#
#.......#
####+####"
    };
}
