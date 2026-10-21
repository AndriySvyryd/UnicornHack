namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment HouseholdYard = new NormalMapFragment
    {
        Name = "household yard",
        GenerationWeight = "$branch == 'outer wards' || ($branch == 'ruined stronghold' && $depth <= 2) ? 2.0 / (1 + $instances) : 0",
        NoTransform = true,
        Map = @"
####+####
#,,...,,#
#,#...#,#
+.......+
#,#...#,#
#,,...,,#
####+####",
        Tags = new HashSet<string> { "!trapped", "!cursed" },
        NoRandomDoorways = true
    };
}
