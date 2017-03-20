using UnicornHack.Utils;

namespace UnicornHack.Generation.Map
{
    public class EmptyLayout : Layout
    {
        protected override Rectangle? SelectNextLot(RectangleIntervalTree placedFragments, SimpleRandom random)
        {
            return null;
        }
    }
}