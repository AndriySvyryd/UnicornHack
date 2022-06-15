using UnicornHack.Generation.Map;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Data.Fragments;

public static partial class DefiningMapFragmentData
{
    public static readonly DefiningMapFragment Dungeon = new DefiningMapFragment
    {
        Name = "dungeon",
        GenerationWeight = "$branch == 'dungeon' ? 1 : 0",
        NoRandomDoorways = true,
        Layout = new UniformLayout { Coverage = 0.33F, MaxLotSize = new Dimensions { Width = 15, Height = 15 }, MinLotSize = new Dimensions { Width = 5, Height = 5 } }
    };
}
