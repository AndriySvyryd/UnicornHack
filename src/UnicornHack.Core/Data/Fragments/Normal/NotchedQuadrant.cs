namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    // 5x5 NW-wall / SE-open corner with a two-tile wall jut into the open quadrant;
    // forces an L-bend detour and offers small line-of-sight cover.
    public static readonly NormalMapFragment NotchedQuadrant = new NormalMapFragment
    {
        Name = "notchedQuadrant",
        GenerationWeight = "10.0 / ((2 + $instances) >> 1)",
        NoMirror = true,
        NoRandomDoorways = true,
        Map = @"
##+##~
+....S
#..#.S
#..#.S
.....S
~SSSS~"
    };
}
