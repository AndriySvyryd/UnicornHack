namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment Cellar = new NormalMapFragment
    {
        Name = "cellar",
        GenerationWeight = "$branch == 'ruined stronghold' && $depth == 2 ? 2.0 / (1 + $instances) : 0",
        NoTransform = true,
        Map = @"
####+####
#.......#
#.##....#
+.......+
#....##.#
#.......#
####+####",
        Tags = new HashSet<string> { "!trapped" },
        NoRandomDoorways = true
    };
}
