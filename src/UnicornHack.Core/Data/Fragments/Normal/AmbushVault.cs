namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    // 7x7 reward chamber: central feature wildcard guarded by four creatures at the
    // inner-square corners.
    public static readonly NormalMapFragment AmbushVault = new NormalMapFragment
    {
        Name = "ambushVault",
        GenerationWeight = "5.0 / ((2 + $instances) >> 1)",
        NoTransform = true,
        NoRandomDoorways = true,
        Map = @"
###+###
#%...%#
#.....#
+..$..+
#.....#
#%...%#
###+###"
    };
}
