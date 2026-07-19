namespace UnicornHack.Data.Fragments;

public static partial class ConnectingMapFragmentData
{
    public static readonly ConnectingMapFragment RandomRectangle = new ConnectingMapFragment
    {
        Name = "randomRectangle",
        DynamicMap = new RectangleMap { MinSize = new Dimensions { Width = 5, Height = 5 } },
        Tags = new HashSet<string> { "door-n", "door-s", "door-e", "door-w" }
    };
}
