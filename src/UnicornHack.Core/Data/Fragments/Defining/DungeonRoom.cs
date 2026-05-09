namespace UnicornHack.Data.Fragments;

public static partial class DefiningMapFragmentData
{
    public static readonly DefiningMapFragment DungeonRoom = new DefiningMapFragment
    {
        Name = "dungeon room",
        GenerationWeight = "$branch == 'dungeon' || $branch == 'entrance' ? 1 : 0",
        NoRandomDoorways = true
    };
}
