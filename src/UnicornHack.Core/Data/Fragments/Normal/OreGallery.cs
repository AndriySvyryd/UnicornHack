namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment OreGallery = new NormalMapFragment
    {
        Name = "ore gallery",
        GenerationWeight = "$branch == 'ruined stronghold' && $depth == 3 ? 2.0 / (1 + $instances) : 0",
        NoTransform = true,
        Map = @"
####+####
#,,,,,,,#
#,#...#,#
+.......+
#,#...#,#
#,,,,,,,#
####+####",
        Tags = new HashSet<string> { "!trapped" },
        NoRandomDoorways = true
    };
}
