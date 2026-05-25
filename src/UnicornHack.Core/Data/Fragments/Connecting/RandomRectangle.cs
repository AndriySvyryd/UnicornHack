namespace UnicornHack.Data.Fragments;

public static partial class ConnectingMapFragmentData
{
    // Dynamic rectangle that injects a default LevelConnection, satisfying either an
    // Entrance or Exit stairs demand on the owning room.
    public static readonly ConnectingMapFragment RandomRectangle = new ConnectingMapFragment
    {
        Name = "randomRectangle",
        DynamicMap = new RectangleMap { MinSize = new Dimensions { Width = 5, Height = 5 } },
        Tags = ["stairs-up", "stairs-down", "door-n", "door-s", "door-e", "door-w"]
    };
}
