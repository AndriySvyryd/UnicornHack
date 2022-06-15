using UnicornHack.Generation.Map;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment RandomRectangle = new NormalMapFragment { Name = "randomRectangle", DynamicMap = new RectangleMap { MinSize = new Dimensions { Width = 5, Height = 5 } } };
}
