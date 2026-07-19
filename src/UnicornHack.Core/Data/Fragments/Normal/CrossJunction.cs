namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment CrossJunction = new NormalMapFragment
    {
        Name = "crossJunction",
        GenerationWeight = "10.0 / ((2 + $instances) >> 1)",
        NoTransform = true,
        Map = @"
##+##
#...#
+.1.+
#...#
##+##",
        Annotations = new Dictionary<char, string> { { '1', "...^" } },
        NoRandomDoorways = true
    };
}
