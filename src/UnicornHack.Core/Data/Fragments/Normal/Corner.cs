namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    // 5x5 inner wall-elbow with Filled S and E edges; lands anywhere a 5x5 lot opens up
    // next to other chunks, splitting an open space.
    public static readonly NormalMapFragment Corner = new NormalMapFragment
    {
        Name = "corner",
        GenerationWeight = "40.0 / ((2 + $instances) >> 1)",
        NoMirror = true,
        NoRandomDoorways = true,
        Map = @"
#####~
#....F
#....F
#..1.F
#....F
~FFFF~",
        Annotations = new Dictionary<char, string>
        {
            ['1'] = "...^"
        }
    };
}
