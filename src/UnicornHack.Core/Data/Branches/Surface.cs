namespace UnicornHack.Data.Branches;

public static partial class BranchData
{
    public static readonly Branch Surface = new Branch
    {
        Length = 1,
        GenerationWeight = "0",
        RoomLayout = new EmptyRoomLayout { Coverage = 0.5F, MaxChunkCount = 6 },
        RoomWidth = 3,
        RoomHeight = 3,
        Name = "surface"
    };
}
