namespace UnicornHack.Data.Fragments;

public static partial class DefiningMapFragmentData
{
    public static readonly DefiningMapFragment RubezahlHall = new DefiningMapFragment
    {
        Name = "rubezahl hall",
        GenerationWeight = "$branch == 'ruined stronghold' && $depth == 3 && $globalInstances == 0 ? 1 : 0",
        NoTransform = true,
        NoOverwrite = true,
        Creatures = new Dictionary<char, string> { { 'B', "Rubezahl" } },
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
#.......................#
#.......................#
############+############",
        Tags = new HashSet<string> { "boss" }
    };
}
