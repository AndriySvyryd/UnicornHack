namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    // 5x5 NW-wall / SE-open corner with a ~25% trap candidate on the approach line
    // between the N/W doorways and the open SE quadrant.
    public static readonly NormalMapFragment TrappedQuadrant = new NormalMapFragment
    {
        Name = "trappedQuadrant",
        GenerationWeight = "10.0 / ((2 + $instances) >> 1)",
        NoMirror = true,
        NoRandomDoorways = true,
        Annotations = new Dictionary<char, string> { ['1'] = "...^" },
        Map = @"
##+##~
+....S
#....S
#.1..S
#....S
~SSSS~"
    };
}
