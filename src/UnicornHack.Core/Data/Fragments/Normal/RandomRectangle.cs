namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment RandomRectangle = new NormalMapFragment
    {
        Name = "randomRectangle",
        DynamicMap = new RectangleMap { MinSize = new Dimensions { Width = 5, Height = 5 } },
        Tags = new HashSet<string> { "door-n", "door-s", "door-e", "door-w" }
    };
}
