namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    // 5x5 NW-wall / SE-open corner with two pillars on the interior diagonal; same edge
    // contract as OpenCornerDoor.
    public static readonly NormalMapFragment TwinPillarQuadrant = new NormalMapFragment
    {
        Name = "twinPillarQuadrant",
        GenerationWeight = "10.0 / ((2 + $instances) >> 1)",
        NoMirror = true,
        NoRandomDoorways = true,
        Map = @"
##+##~
+....S
#.#..S
#..#.S
#....S
~SSSS~"
    };
}
