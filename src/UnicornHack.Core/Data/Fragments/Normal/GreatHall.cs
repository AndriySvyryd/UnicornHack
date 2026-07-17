namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    // 9x9 open great hall with a doorway centred on every side. Fully symmetric, so it
    // collapses to a single variant. A large size-large chunk that fills most of a big
    // lot on its own.
    public static readonly NormalMapFragment GreatHall = new NormalMapFragment
    {
        Name = "greatHall",
        GenerationWeight = "4.0 / ((2 + $instances) >> 1)",
        NoTransform = true,
        NoRandomDoorways = true,
        Map = @"
####+####
#.......#
#.......#
#.......#
+.......+
#.......#
#.......#
#.......#
####+####"
    };
}
