namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment TrappedQuadrant = new NormalMapFragment
    {
        Name = "trappedQuadrant",
        GenerationWeight = "10.0 / ((2 + $instances) >> 1)",
        NoMirror = true,
        Map = @"
##+##~
+....S
#....S
#.1..S
#....S
~SSSS~",
        Annotations = new Dictionary<char, string> { { '1', "...^" } },
        NoRandomDoorways = true
    };
}
