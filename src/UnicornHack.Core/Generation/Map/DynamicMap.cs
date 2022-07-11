using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

public abstract class DynamicMap
{
    public Dimensions MinSize
    {
        get;
        set;
    } = new(5, 5);

    public abstract IEnumerable<Point> GetRoomPoints(LevelComponent level, Rectangle boundingRectangle);
}
