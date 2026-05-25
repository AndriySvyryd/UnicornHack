namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    // 5x5 NW-wall / SE-open corner with a single pillar in the open quadrant; same edge
    // contract as OpenCornerDoor.
    public static readonly NormalMapFragment PillarQuadrant = new NormalMapFragment
    {
        Name = "pillarQuadrant",
        GenerationWeight = "10.0 / ((2 + $instances) >> 1)",
        NoMirror = true,
        NoRandomDoorways = true,
        Map = @"
##+##~
+....S
#....S
#..#.S
.....S
~SSSS~"
    };
}
