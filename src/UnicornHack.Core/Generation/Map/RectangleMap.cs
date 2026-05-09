using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

public class RectangleMap : DynamicMap
{
    public override IEnumerable<Point> GetRoomPoints(RoomComponent room, Rectangle boundingRectangle)
        // Bias toward the bounding rectangle's edges so generated rooms hug the lot
        // perimeter rather than clustering in the centre.
        => Rectangle.CreateRandomNearEdge(room.Level.GenerationRandom, boundingRectangle, MinSize);
}
