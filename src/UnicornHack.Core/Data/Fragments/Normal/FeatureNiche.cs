namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment FeatureNiche = new NormalMapFragment
    {
        Name = "featureNiche",
        GenerationWeight = "10.0 / ((2 + $instances) >> 1)",
        NoTransform = true,
        Map = @"##+##
#...#
+.$.+
#...#
##+##",
        NoRandomDoorways = true
    };
}
