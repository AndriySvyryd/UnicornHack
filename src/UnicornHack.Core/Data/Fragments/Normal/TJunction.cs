namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment TJunction = new NormalMapFragment
    {
        Name = "tJunction",
        GenerationWeight = "20.0 / ((2 + $instances) >> 1)",
        NoMirror = true,
        Map = @"
##+##
#...#
+.1.+
#...#
#####",
        Annotations = new Dictionary<char, string> { { '1', "...^" } },
        NoRandomDoorways = true
    };
}
