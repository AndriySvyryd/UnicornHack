using System.Collections.Generic;
using UnicornHack.Utils;

namespace UnicornHack.Generation.Map
{
    public abstract class DynamicMap
    {
        public Dimensions MinSize { get; set; } = new Dimensions(5, 5);

        public abstract IEnumerable<Point> GetRoomPoints(Level level, Rectangle boundingRectangle);
    }
}