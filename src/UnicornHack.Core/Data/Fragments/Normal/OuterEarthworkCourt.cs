namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment OuterEarthworkCourt = new NormalMapFragment
    {
        Name = "outer earthwork court",
        GenerationWeight = "$branch == 'outer wards' || ($branch == 'ruined stronghold' && $depth == 1) ? 3.0 / (1 + $instances) : 0",
        NoTransform = true,
        Map = @"
####+####
#,,,,,,,#
#,,###,,#
+,,...,,+
#,,###,,#
#,,,,,,,#
####+####",
        Tags = new HashSet<string> { "!trapped", "!cursed" },
        NoRandomDoorways = true
    };
}
