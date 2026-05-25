namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    // 5x5 niche framing a central feature wildcard with a one-tile aisle on every side.
    public static readonly NormalMapFragment FeatureNiche = new NormalMapFragment
    {
        Name = "featureNiche",
        GenerationWeight = "10.0 / ((2 + $instances) >> 1)",
        NoTransform = true,
        NoRandomDoorways = true,
        Map = """
            ##+##
            #...#
            +.$.+
            #...#
            ##+##
            """
    };
}
