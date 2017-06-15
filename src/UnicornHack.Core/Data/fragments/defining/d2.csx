new DefiningMapFragment
{
    Name = "d2",
    GenerationWeight = new BranchWeight { W = new ConstantWeight { }, Name = "dungeon", MinDepth = 2, MaxDepth = 2 },
    NoRandomDoorways = true,
    LevelHeight = 5,
    LevelWidth = 5,
    Map = @"
#####
#BBB#
#B<B#
#BBB#
#####"
}
