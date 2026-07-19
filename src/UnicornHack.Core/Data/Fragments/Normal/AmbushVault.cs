namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment AmbushVault = new NormalMapFragment
    {
        Name = "ambushVault",
        GenerationWeight = "5.0 / ((2 + $instances) >> 1)",
        NoTransform = true,
        Map = @"
###+###
#%...%#
#.....#
+..$..+
#.....#
#%...%#
###+###",
        NoRandomDoorways = true
    };
}
