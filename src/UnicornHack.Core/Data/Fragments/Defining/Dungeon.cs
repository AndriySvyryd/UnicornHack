namespace UnicornHack.Data.Fragments;

public static partial class DefiningMapFragmentData
{
    public static readonly DefiningMapFragment Dungeon = new DefiningMapFragment { Name = "dungeon", GenerationWeight = "$branch == 'dungeon' ? 1 : 0", NoRandomDoorways = true, Layout = new UniformLayout() };
}
