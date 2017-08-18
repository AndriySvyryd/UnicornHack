using UnicornHack.Generation;
using UnicornHack.Generation.Map;
using UnicornHack.Utils;

namespace UnicornHack.Data.Fragments
{
    public static partial class NormalMapFragmentData
    {
        public static readonly MapFragment RandomRectangle = new MapFragment
        {
            Name = "randomRectangle",
            GenerationWeight = new DefaultWeight(),
            DynamicMap = new RectangleMap {MinSize = new Dimensions {Width = 5, Height = 5}}
        };
    }
}