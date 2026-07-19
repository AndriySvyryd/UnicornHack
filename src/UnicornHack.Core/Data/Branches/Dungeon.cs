namespace UnicornHack.Data.Branches;

public static partial class BranchData
{
    public static readonly Branch Dungeon = new Branch { Length = 10, Layout = new TreeLevelLayout(), RoomLayout = new DenseRoomLayout(), Name = "dungeon" };
}
