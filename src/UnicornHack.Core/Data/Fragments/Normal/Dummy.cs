using UnicornHack.Generation;
using UnicornHack.Generation.Map;

namespace UnicornHack.Data.Fragments
{
    public static partial class NormalMapFragmentData
    {
        public static readonly MapFragment Dummy = new MapFragment
        {
            Name = "dummy",
            GenerationWeight = new ConstantWeight(),
            Map = @"
."
        };
    }
}