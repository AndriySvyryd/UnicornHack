using System.Collections.Generic;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Generation.Map;

public class RectangleMap : DynamicMap
{
    public override IEnumerable<Point> GetRoomPoints(LevelComponent level, Rectangle boundingRectangle)
        // TODO: Prefer elongated shapes
        => Rectangle.CreateRandom(level.GenerationRandom, boundingRectangle, MinSize);
}
