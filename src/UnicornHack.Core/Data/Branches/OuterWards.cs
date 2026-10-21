namespace UnicornHack.Data.Branches;

public static partial class BranchData
{
    public static readonly Branch OuterWards = new Branch
    {
        Length = 1,
        GenerationWeight = "0",
        Layout = new TreeLevelLayout
        {
            MaxRooms = 14,
            GridWidth = 5,
            GridHeight = 3,
            Interconnectedness = 0.25F,
            RewardRoomProbability = 0.6F,
            CurseRoomProbability = 0F,
            HiddenConnectionProbability = 0F,
            MinRooms = 10,
            RewardWeights = new Dictionary<RoomFeatures, string>
            {
                {
                    RoomFeatures.TraitPedestal,
                    "1.0 / ((2 + $globalCount) >> 1)"
                },
                {
                    RoomFeatures.FeatPedestal,
                    "1.0 / ((2 + $globalCount) >> 1)"
                }
            },
            Sinuosity = 0.1F
        },
        CreatureGenerator = new CreatureGenerator { ExpectedInitialCount = 6, MaximumInitialCount = 8 },
        Name = "outer wards"
    };
}
