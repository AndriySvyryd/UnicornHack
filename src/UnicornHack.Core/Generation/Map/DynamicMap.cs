using System.Collections.Generic;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Generation.Map;

public abstract class DynamicMap
{
    public Dimensions MinSize
    {
        get;
        set;
    } = new Dimensions(5, 5);

    public abstract IEnumerable<Point> GetRoomPoints(LevelComponent level, Rectangle boundingRectangle);
}
