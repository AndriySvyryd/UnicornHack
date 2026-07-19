namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment FeatureChamber = new NormalMapFragment
    {
        Name = "featureChamber",
        GenerationWeight = "10.0 / ((2 + $instances) >> 1)",
        NoTransform = true,
        Map = @"###+###
#.....#
#.....#
+..$..+
#.....#
#.....#
###+###",
        NoRandomDoorways = true
    };
}
