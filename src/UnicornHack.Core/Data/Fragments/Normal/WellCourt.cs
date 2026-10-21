namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment WellCourt = new NormalMapFragment
    {
        Name = "well court",
        GenerationWeight = "$branch == 'ruined stronghold' && $depth == 2 ? 1.0 / (1 + $instances) : 0",
        NoTransform = true,
        Map = @"
####+####
#.......#
#..~~~..#
+.......+
#..###..#
#.......#
####+####",
        Tags = new HashSet<string> { "!trapped" },
        NoRandomDoorways = true
    };
}
