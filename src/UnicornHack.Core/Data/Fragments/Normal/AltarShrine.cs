namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment AltarShrine = new NormalMapFragment
    {
        Name = "altarShrine",
        GenerationWeight = "20.0 / ((2 + $globalInstances) >> 1)",
        NoTransform = true,
        Map = @"
###+###
#.....#
#.#.#.#
+.._..+
#.#.#.#
#.....#
###+###",
        NoRandomDoorways = true
    };
}
