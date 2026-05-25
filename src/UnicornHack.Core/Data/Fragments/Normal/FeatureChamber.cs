namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    // 7x7 chamber framing a central feature wildcard with two tiles of breathing room.
    public static readonly NormalMapFragment FeatureChamber = new NormalMapFragment
    {
        Name = "featureChamber",
        GenerationWeight = "10.0 / ((2 + $instances) >> 1)",
        NoTransform = true,
        NoRandomDoorways = true,
        Map = """
            ###+###
            #.....#
            #.....#
            +..$..+
            #.....#
            #.....#
            ###+###
            """
    };
}
