namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment WallWalk = new NormalMapFragment
    {
        Name = "wall walk",
        GenerationWeight = "$branch == 'ruined stronghold' && $depth == 2 ? 2.0 / (1 + $instances) : 0",
        NoTransform = true,
        Map = @"
####+####
#.......#
#.##.##.#
+.......+
#.##.##.#
#.......#
####+####",
        Tags = new HashSet<string> { "!trapped" },
        NoRandomDoorways = true
    };
}
