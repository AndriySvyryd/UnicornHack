namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment OpenHallDoor = new NormalMapFragment
    {
        Name = "openHallDoor",
        GenerationWeight = "10.0 / ((2 + $instances) >> 1)",
        NoMirror = true,
        Map = @"
###+###~
+......S
#......S
#......S
#......S
#......S
#......S
~SSSSSS~",
        NoRandomDoorways = true
    };
}
