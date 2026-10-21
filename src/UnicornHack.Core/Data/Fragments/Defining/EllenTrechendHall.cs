namespace UnicornHack.Data.Fragments;

public static partial class DefiningMapFragmentData
{
    public static readonly DefiningMapFragment EllenTrechendHall = new DefiningMapFragment
    {
        Name = "ellen trechend hall",
        GenerationWeight = "$branch == 'ruined stronghold' && $depth <= 2 && $globalInstances == 0 ? 1 : 0",
        NoTransform = true,
        NoOverwrite = true,
        Creatures = new Dictionary<char, string> { { 'B', "Ellen Trechend" } },
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
#.......................#
+...........B...........+
#.......................#
#....##........##.......#
#.......................#
#.......................#
#....##........##.......#
#....................>..#
#.......................#
############+############",
        Tags = new HashSet<string> { "boss" }
    };
}
