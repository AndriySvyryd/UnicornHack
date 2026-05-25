namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    // 7x7 shrine with a centred DeityAltar. Weight decays against `$globalInstances` so
    // later altars compete with generic chambers.
    public static readonly NormalMapFragment AltarShrine = new NormalMapFragment
    {
        Name = "altarShrine",
        GenerationWeight = "20.0 / ((2 + $globalInstances) >> 1)",
        NoTransform = true,
        NoRandomDoorways = true,
        Map = @"
###+###
#.....#
#.#.#.#
+.._..+
#.#.#.#
#.....#
###+###"
    };
}
