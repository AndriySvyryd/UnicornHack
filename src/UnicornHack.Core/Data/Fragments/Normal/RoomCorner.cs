namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment RoomCorner = new NormalMapFragment
    {
        Name = "roomCorner",
        GenerationWeight = "20.0 / ((2 + $instances) >> 1)",
        NoMirror = true,
        Map = @"
~EEEE~
E#####
E#...+
E#...#
E#...#
~##+##",
        NoRandomDoorways = true
    };
}
