namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    // 7x7 chamber with a cross-shaped pool blocking the centre; ranged advantage and
    // four small ordinal choke points around a central creature wildcard.
    public static readonly NormalMapFragment PoolGarden = new NormalMapFragment
    {
        Name = "poolGarden",
        GenerationWeight = "4.0 / ((2 + $instances) >> 1)",
        NoTransform = true,
        NoRandomDoorways = true,
        Map = @"
###+###
#.....#
#.~~~.#
+.~%~.+
#.~~~.#
#.....#
###+###"
    };
}
