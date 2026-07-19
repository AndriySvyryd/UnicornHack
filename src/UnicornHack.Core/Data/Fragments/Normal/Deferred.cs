namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment Deferred = new NormalMapFragment
    {
        Name = "$deferred",
        GenerationWeight = "0",
        NoTransform = true,
        DynamicMap = new RectangleMap { MinSize = new Dimensions { Width = 1, Height = 1 } }
    };
}
