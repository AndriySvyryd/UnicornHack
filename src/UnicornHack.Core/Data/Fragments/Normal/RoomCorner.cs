namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    // 5x5 NW-anchored room corner: N and W edges are Edge-conditioned so the fragment
    // only seats against the room boundary; S and E walls carry doorways opening into
    // the room interior.
    public static readonly NormalMapFragment RoomCorner = new NormalMapFragment
    {
        Name = "roomCorner",
        GenerationWeight = "20.0 / ((2 + $instances) >> 1)",
        NoMirror = true,
        NoRandomDoorways = true,
        Map = @"
~EEEE~
E#####
E#...+
E#...#
E#...#
~##+##"
    };
}
