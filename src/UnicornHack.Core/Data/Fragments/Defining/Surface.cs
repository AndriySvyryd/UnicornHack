using UnicornHack.Generation.Map;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Fragments;

public static partial class DefiningMapFragmentData
{
    public static readonly DefiningMapFragment Surface = new DefiningMapFragment
    {
        Name = "surface",
        GenerationWeight = "$branch == 'surface' ? Infinity : 0",
        Connections = new LevelConnection[] { new LevelConnection { Direction = ConnectionDirection.Source, BranchName = "dungeon", Glyph = '>' } },
        NoRandomDoorways = true,
        LevelHeight = 3,
        LevelWidth = 3,
        Map = @"
###
#>#
###"
    };
}
