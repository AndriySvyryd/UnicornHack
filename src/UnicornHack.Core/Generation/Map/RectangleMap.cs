using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

public class RectangleMap : DynamicMap
{
    public override IEnumerable<Point> GetRoomPoints(RoomComponent room, Rectangle boundingRectangle,
        DirectionFlags preferredEdges = DirectionFlags.None, DirectionFlags flushEdges = DirectionFlags.None)
        // Bias toward the bounding rectangle's edges so generated rooms hug the lot
        // perimeter rather than clustering in the centre. When the lot has unmet
        // intra-level connection demand on specific cardinals, force the bias toward
        // those edges so subsequent doorway carving has wall to work against. Flush edges
        // pin the carved shape against the lot side that already abuts a placed chunk.
        => Rectangle.CreateRandomNearEdge(
            room.Level.GenerationRandom, boundingRectangle, MinSize, preferredEdges, flushEdges);
}
