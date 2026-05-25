namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    // 5x5 four-way junction with a doorway on every side; centre tile has ~25% trap chance.
    public static readonly NormalMapFragment CrossJunction = new NormalMapFragment
    {
        Name = "crossJunction",
        GenerationWeight = "10.0 / ((2 + $instances) >> 1)",
        NoTransform = true,
        NoRandomDoorways = true,
        Annotations = new Dictionary<char, string> { ['1'] = "...^" },
        Map = @"
##+##
#...#
+.1.+
#...#
##+##"
    };
}
