namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    // 9x9 NW-wall / SE-open great quadrant: the larger sibling of OpenHallDoor. S and E
    // edges are Same-conditioned so the fragment tiles against neighbours whose facing
    // side is open floor, letting it fill a big corner lot while docking with adjacent
    // chunks. A doorway sits on the N wall.
    public static readonly NormalMapFragment GreatOpenQuadrant = new NormalMapFragment
    {
        Name = "greatOpenQuadrant",
        GenerationWeight = "5.0 / ((2 + $instances) >> 1)",
        NoMirror = true,
        NoRandomDoorways = true,
        Map = @"
####+####~
+........S
#........S
#........S
#........S
#........S
#........S
#........S
~SSSSSSSS~"
    };
}
