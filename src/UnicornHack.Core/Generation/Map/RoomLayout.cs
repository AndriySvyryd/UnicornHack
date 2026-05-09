using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

/// <summary>
///     Fills a single <see cref="RoomComponent" /> from one or more <see cref="Chunk" />s
///     (typically produced by <see cref="MapFragment" />s).
/// </summary>
/// <remarks>
///     Owns the room-scope concerns:
///     <list type="bullet">
///         <item>Fragment placement within the room (sub-chunks)</item>
///         <item>Carving corridors between chunks inside the room</item>
///     </list>
///     Knows nothing about the level graph -- that's the <see cref="LevelLayout" />'s job.
///     Terrain allocation is owned by <see cref="RoomTerrain" /> and performed by the caller
///     before <see cref="Fill" /> runs. Door placement at the perimeter is owned by
///     <see cref="LevelLayout.MaterializeGraph" />, which queries the room's chunks for
///     suitable doorway tiles.
/// </remarks>
public abstract class RoomLayout
{
    protected static float DefaultCoverage = 0.5f;

    public float Coverage
    {
        get;
        set;
    } = DefaultCoverage;

    protected static byte DefaultMaxChunkCount = 6;

    public byte MaxChunkCount
    {
        get;
        set;
    } = DefaultMaxChunkCount;

    /// <summary>
    ///     Fills <paramref name="room" /> with terrain. The layout decides chunk placement
    ///     within the room. <paramref name="branch" /> supplies the level-scope defaults that
    ///     <paramref name="fragment" /> overrides may inherit from.
    /// </summary>
    /// <param name="doorwayDemand">
    ///     Number of doorway tiles the room must end up with in each <see cref="Direction" />,
    ///     keyed by the direction the corresponding inter-room edge leaves (or enters) the
    ///     room. Implementations must place chunks so that every entry's demand can be met
    ///     by walkable / wall doorway tiles in the matching half-plane (or quadrant for
    ///     diagonals) of the room; if the layout cannot satisfy the demand it must return
    ///     <c>null</c> so the caller can retry generation with a different seed.
    /// </param>
    /// <returns>
    ///     The chunks that make up this room, or <c>null</c> if the demand could not be met.
    /// </returns>
    public abstract List<Chunk>? Fill(
        RoomComponent room,
        Branch branch,
        DefiningMapFragment fragment,
        Dictionary<Direction, int> doorwayDemand);

    /// <summary>
    ///     Returns <c>true</c> when the doorway points on this room's chunks number at
    ///     least the total edge demand summed over every direction. <c>false</c> means the
    ///     layout must produce a different chunk set (or fail) for the room to be usable
    ///     by <see cref="LevelLayout.MaterializeGraph" />.
    /// </summary>
    protected static bool SatisfiesDemand(
        RoomComponent room,
        IReadOnlyList<Chunk> chunks,
        IReadOnlyDictionary<Direction, int> demand)
    {
        var required = 0;
        foreach (var (_, count) in demand)
        {
            if (count > 0)
            {
                required += count;
            }
        }

        if (required == 0)
        {
            return true;
        }

        var supply = 0;
        for (var i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            if (chunk.Room == room)
            {
                supply += chunk.DoorwayPoints.Count;
            }
        }

        return supply >= required;
    }
}
