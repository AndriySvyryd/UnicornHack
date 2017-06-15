new DefiningMapFragment
{
    Name = "surface",
    GenerationWeight = new BranchWeight { W = new InfiniteWeight(), Name = "surface" },
    Connections = new LevelConnection[] { new LevelConnection { BranchName = "dungeon", Glyph = '>' } },
    NoRandomDoorways = true,
    LevelHeight = 3,
    LevelWidth = 3,
    Map = @"
###
#>#
###"
}
