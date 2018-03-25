using UnicornHack.Utils;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Generation.Map
{
    public class EmptyLayout : Layout
    {
        protected override Rectangle? SelectNextLot(RectangleIntervalTree placedFragments, SimpleRandom random) => null;
    }
}
