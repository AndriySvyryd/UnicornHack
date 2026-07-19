namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment Corner = new NormalMapFragment
    {
        Name = "corner",
        GenerationWeight = "40.0 / ((2 + $instances) >> 1)",
        NoMirror = true,
        Map = @"
#####~
#....F
#....F
#..1.F
#....F
~FFFF~",
        Annotations = new Dictionary<char, string> { { '1', "...^" } },
        NoRandomDoorways = true
    };
}
