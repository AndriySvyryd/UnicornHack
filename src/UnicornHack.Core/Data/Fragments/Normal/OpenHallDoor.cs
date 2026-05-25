namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    // 7x7 larger sibling of OpenCornerDoor with the same S/E-open edge contract,
    // for sizeable lots that still need to tile on two sides.
    public static readonly NormalMapFragment OpenHallDoor = new NormalMapFragment
    {
        Name = "openHallDoor",
        GenerationWeight = "10.0 / ((2 + $instances) >> 1)",
        NoMirror = true,
        NoRandomDoorways = true,
        Map = @"
###+###~
+......S
#......S
#......S
#......S
#......S
.......S
~SSSSSS~"
    };
}
