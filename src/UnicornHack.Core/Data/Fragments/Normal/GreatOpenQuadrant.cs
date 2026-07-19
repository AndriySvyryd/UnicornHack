namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment GreatOpenQuadrant = new NormalMapFragment
    {
        Name = "greatOpenQuadrant",
        GenerationWeight = "5.0 / ((2 + $instances) >> 1)",
        NoMirror = true,
        Map = @"
####+####~
+........S
#........S
#........S
#........S
#........S
#........S
#........S
~SSSSSSSS~",
        NoRandomDoorways = true
    };
}
