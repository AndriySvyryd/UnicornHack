namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    // 9x7 rectangular grand chamber with a doorway centred on every side. Mirror-symmetric,
    // so only the two rotations (9x7 / 7x9) are kept. A large size-large chunk that fills a
    // sizeable oblong lot on its own.
    public static readonly NormalMapFragment GrandChamber = new NormalMapFragment
    {
        Name = "grandChamber",
        GenerationWeight = "4.0 / ((2 + $instances) >> 1)",
        NoMirror = true,
        NoRandomDoorways = true,
        Map = @"
####+####
#.......#
#.......#
+.......+
#.......#
#.......#
####+####"
    };
}
