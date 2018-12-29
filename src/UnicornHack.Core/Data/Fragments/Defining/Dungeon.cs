using UnicornHack.Generation;
using UnicornHack.Generation.Map;

namespace UnicornHack.Data.Fragments
{
    public static partial class DefiningMapFragmentData
    {
        public static readonly DefiningMapFragment Dungeon = new DefiningMapFragment
        {
            Name = "dungeon",
            GenerationWeight = new BranchWeight {Matched = new DefaultWeight(), Name = "dungeon"},
            NoRandomDoorways = true,
            Layout = new UniformLayout {Coverage = 0.33F}
        };
    }
}
