new EncompassingMapFragment
{
    Name = "dungeon",
    GenerationWeight = new BranchWeight { W = new DefaultWeight { }, Name = "dungeon" },
    NoRandomDoorways = true,
    LevelHeight = 40,
    LevelWidth = 80,
    Layout = new UniformLayout { MaxLotSize = new Dimensions { Width = 20, Height = 20 }, MinLotSize = new Dimensions { Width = 6, Height = 6 }, Coverage = 0.33F, MaxRoomCount = 16 }
}
