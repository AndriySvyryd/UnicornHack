using UnicornHack.Generation;
using UnicornHack.Generation.Map;

namespace UnicornHack.Data.Fragments
{
    public static partial class DefiningMapFragmentData
    {
        public static readonly DefiningMapFragment D2 = new DefiningMapFragment
        {
            Name = "d2",
            GenerationWeight = new BranchWeight
            {
                Matched = new DefaultWeight {Multiplier = 0F},
                Name = "dungeon",
                MinDepth = 2,
                MaxDepth = 2
            },
            NoRandomDoorways = true,
            LevelHeight = 5,
            LevelWidth = 5,
            Map = @"
#####
#BBB#
#B<B#
#BBB#
#####"
        };
    }
}
