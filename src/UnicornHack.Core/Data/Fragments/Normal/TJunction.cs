namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    // 5x5 three-way junction (N/E/W doorways, solid south wall); centre tile has ~25%
    // trap chance.
    public static readonly NormalMapFragment TJunction = new NormalMapFragment
    {
        Name = "tJunction",
        GenerationWeight = "20.0 / ((2 + $instances) >> 1)",
        NoMirror = true,
        NoRandomDoorways = true,
        Annotations = new Dictionary<char, string> { ['1'] = "...^" },
        Map = @"
##+##
#...#
+.1.+
#...#
#####"
    };
}
