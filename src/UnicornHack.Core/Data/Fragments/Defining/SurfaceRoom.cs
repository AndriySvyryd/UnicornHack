namespace UnicornHack.Data.Fragments;

public static partial class DefiningMapFragmentData
{
    public static readonly DefiningMapFragment SurfaceRoom = new DefiningMapFragment
    {
        Name = "surface room",
        GenerationWeight = "$branch == 'surface' ? Infinity : 0",
        NoTransform = true,
        Connections = new List<LevelConnection> { new LevelConnection { Direction = Directionality.Source, BranchName = "entrance", Glyph = '>' } },
        NoRandomDoorways = true,
        Map = @"
###
#>#
###"
    };
}
