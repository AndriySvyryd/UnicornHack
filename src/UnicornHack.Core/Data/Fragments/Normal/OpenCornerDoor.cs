namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment OpenCornerDoor = new NormalMapFragment
    {
        Name = "openCornerDoor",
        GenerationWeight = "10.0 / ((2 + $instances) >> 1)",
        NoMirror = true,
        Map = @"
##+##~
+....S
#....S
#....S
#....S
~SSSS~",
        NoRandomDoorways = true
    };
}
