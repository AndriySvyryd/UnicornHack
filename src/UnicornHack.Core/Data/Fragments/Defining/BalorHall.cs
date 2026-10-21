namespace UnicornHack.Data.Fragments;

public static partial class DefiningMapFragmentData
{
    public static readonly DefiningMapFragment BalorHall = new DefiningMapFragment
    {
        Name = "balor hall",
        GenerationWeight = "$branch == 'ruined stronghold' && $depth == 3 && $globalInstances == 0 ? 1 : 0",
        NoTransform = true,
        NoOverwrite = true,
        Creatures = new Dictionary<char, string> { { 'B', "Balor ua Neit" }, { 'g', "fomorian attendant" } },
        NoRandomDoorways = true,
        CreatureGenerator = new CreatureGenerator { ExpectedInitialCount = 0 },
        ItemGenerator = new ItemGenerator { ExpectedInitialCount = 0 },
        Map = @"
############+############
#.......................#
#....##........##.......#
#.......................#
#.......................#
#....##........##.......#
#........g.....g........#
+...........B...........+
#.......................#
#........g.....g........#
#....##........##.......#
#.......................#
#.......................#
#.......................#
#.......................#
############+############",
        Tags = new HashSet<string> { "boss" }
    };
}
