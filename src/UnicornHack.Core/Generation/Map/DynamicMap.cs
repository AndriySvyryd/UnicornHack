using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

public abstract class DynamicMap
{
    public Dimensions MinSize
    {
        get;
        set;
    } = new(5, 5);

    /// <summary>
    ///     Returns the cells that make up the dynamic payload inside
    ///     <paramref name="boundingRectangle" />. <paramref name="preferredEdges" />
    ///     advertises the room perimeter directions on which the owning lot is flush with
    ///     unmet intra-level connection demand; placers honour the flags by biasing the
    ///     placed shape toward those edges so subsequent doorway carving has a wall to
    ///     work against. <paramref name="flushEdges" /> is stricter: each set cardinal
    ///     pins the corresponding edge of the carved shape flush against
    ///     <paramref name="boundingRectangle" />, which the layout uses when the lot is
    ///     known to overlap an existing chunk on that side. <see cref="DirectionFlags.None" />
    ///     in either argument leaves the corresponding axis to its default behaviour.
    /// </summary>
    public abstract IEnumerable<Point> GetRoomPoints(RoomComponent room, Rectangle boundingRectangle,
        DirectionFlags preferredEdges = DirectionFlags.None, DirectionFlags flushEdges = DirectionFlags.None);
}
