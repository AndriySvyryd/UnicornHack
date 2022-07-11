using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

public class RectangleMap : DynamicMap
{
    public override IEnumerable<Point> GetRoomPoints(LevelComponent level, Rectangle boundingRectangle)
        // TODO: Prefer elongated shapes
        => Rectangle.CreateRandom(level.GenerationRandom, boundingRectangle, MinSize);
}
