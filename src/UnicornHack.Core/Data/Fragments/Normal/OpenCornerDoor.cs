namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    // 5x5 NW-wall / SE-open corner. S and E edges are Same-conditioned so the fragment
    // tiles with neighbours whose facing side is open floor.
    public static readonly NormalMapFragment OpenCornerDoor = new NormalMapFragment
    {
        Name = "openCornerDoor",
        GenerationWeight = "10.0 / ((2 + $instances) >> 1)",
        NoMirror = true,
        NoRandomDoorways = true,
        Map = @"
##+##~
+....S
#....S
#....S
.....S
~SSSS~"
    };
}
