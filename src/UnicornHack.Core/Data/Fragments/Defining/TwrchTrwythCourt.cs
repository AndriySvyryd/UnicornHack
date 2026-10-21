namespace UnicornHack.Data.Fragments;

public static partial class DefiningMapFragmentData
{
    public static readonly DefiningMapFragment TwrchTrwythCourt = new DefiningMapFragment
    {
        Name = "twrch trwyth court",
        GenerationWeight = "$branch == 'outer wards' && $depth == 1 && $globalInstances == 0 ? 1 : 0",
        NoTransform = true,
        NoOverwrite = true,
        Creatures = new Dictionary<char, string> { { 'B', "Twrch Trwyth" } },
        NoRandomDoorways = true,
        CreatureGenerator = new CreatureGenerator { ExpectedInitialCount = 0 },
        ItemGenerator = new ItemGenerator { ExpectedInitialCount = 0 },
        Map = @"
############+############
#.......................#
#.......................#
#....##........##.......#
#.......................#
#.......................#
#.......................#
+...........B...........+
#.......................#
#.......................#
#.......................#
#....##........##.......#
#.......................#
#....................>..#
#.......................#
############+############",
        Tags = new HashSet<string> { "boss" }
    };
}
