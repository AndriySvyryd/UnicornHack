namespace UnicornHack.Data.Branches;

public static partial class BranchData
{
    public static readonly Branch RuinedStronghold = new Branch
    {
        Length = 3,
        GenerationWeight = "$branch == 'outer wards' ? 1 : 0",
        Layout = new GridLevelLayout
        {
            GridWidth = 7,
            GridHeight = 5,
            Interconnectedness = 0.75F,
            RewardRoomProbability = 0.3F,
            CurseRoomProbability = 0.1F,
            HiddenConnectionProbability = 0.3F,
            MinRooms = 28,
            MaxRooms = 32
        },
        CreatureGenerator = new CreatureGenerator { MaximumInitialCount = 10 },
        RoomLayout = new DenseRoomLayout(),
        IsTerminal = true,
        Name = "ruined stronghold"
    };
}
