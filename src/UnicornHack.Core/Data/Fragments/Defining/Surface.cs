using UnicornHack.Generation;
using UnicornHack.Generation.Map;

namespace UnicornHack.Data.Fragments
{
    public static partial class DefiningMapFragmentData
    {
        public static readonly DefiningMapFragment Surface = new DefiningMapFragment
        {
            Name = "surface",
            GenerationWeight = new BranchWeight {Matched = new InfiniteWeight(), Name = "surface"},
            Connections = new[] {new LevelConnection {BranchName = "dungeon", Glyph = '>'}},
            NoRandomDoorways = true,
            LevelHeight = 3,
            LevelWidth = 3,
            Map = @"
###
#>#
###"
        };
    }
}
