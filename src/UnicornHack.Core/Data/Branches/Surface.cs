namespace UnicornHack.Data.Branches;

public static partial class BranchData
{
    public static readonly Branch Surface = new Branch
    {
        Name = "surface",
        Length = 1,
        // Never a random inter-branch destination.
        GenerationWeight = "0",
        // The surface fragment is hand-authored 3x3, so the branch room dimensions match.
        RoomWidth = 3,
        RoomHeight = 3,
        Layout = new EmptyLevelLayout(),
        RoomLayout = new EmptyRoomLayout()
    };
}
