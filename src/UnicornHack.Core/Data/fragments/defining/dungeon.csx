new DefiningMapFragment
{
    Name = "dungeon",
    GenerationWeight = new BranchWeight { W = new DefaultWeight { }, Name = "dungeon" },
    NoRandomDoorways = true,
    Layout = new UniformLayout { MaxLotSize = new Dimensions { Width = 20, Height = 20 }, MinLotSize = new Dimensions { Width = 6, Height = 6 }, Coverage = 0.4F, MaxRoomCount = 16 }
}
