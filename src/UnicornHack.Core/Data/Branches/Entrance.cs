namespace UnicornHack.Data.Branches;

public static partial class BranchData
{
    /// <summary>
    ///     Single introductory level placed between <see cref="Surface" /> and
    ///     <see cref="Dungeon" />. Hosts the player's first dungeon-style room set with
    ///     gentle parameters so the depth-1 random seed doesn't put them in front of a
    ///     full-strength <see cref="Dungeon" /> map. The branch is excluded from random
    ///     destination selection (<c>GenerationWeight = "0"</c>); it's only reached via
    ///     the explicit <c>"entrance"</c> link on <see cref="DefiningMapFragmentData.SurfaceRoom" />.
    /// </summary>
    public static readonly Branch Entrance = new Branch
    {
        Name = "entrance",
        Length = 1,
        GenerationWeight = "0",
        Layout = new TreeLevelLayout()
        {
            GridWidth = 5,
            GridHeight = 3,
            Interconnectedness = 0.25f,
            Sinuosity = 0.1f,
            RewardRoomProbability = 0.6f,
            CurseRoomProbability = 0.0f,
            HiddenConnectionProbability = 0.0f,
            MinRooms = 10,
            MaxRooms = 14
        }
    };
}
