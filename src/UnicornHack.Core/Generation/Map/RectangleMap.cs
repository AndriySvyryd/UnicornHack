using System.Collections.Generic;
using UnicornHack.Utils;

namespace UnicornHack.Generation.Map
{
    public class RectangleMap : DynamicMap
    {
        public override IEnumerable<Point> GetRoomPoints(Level level, Rectangle boundingRectangle)
            // TODO: Prefer elongated shapes
            => Rectangle.CreateRandom(level.GenerationRandom, boundingRectangle, MinSize);
    }
}