using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

/// <summary>
///     A <see cref="RoomLayout" /> that places only the level's defining fragment as a single chunk
///     with no surrounding fragments. Used by <see cref="EmptyLevelLayout" /> for trivial levels
///     (e.g. the surface entrance) and by tests that want full control over the map.
/// </summary>
public sealed class EmptyRoomLayout : RoomLayout
{
    public override List<Chunk>? Fill(
        RoomComponent room,
        Branch branch,
        DefiningMapFragment fragment,
        Dictionary<Direction, int> doorwayDemand,
        RoomParameters roomParameters)
    {
        var defaultTerrain = fragment.GetEffectiveDefaultTerrain(branch);
        if (defaultTerrain != MapFeature.Default)
        {
            for (var index = 0; index < room.TileCount; index++)
            {
                room.Terrain[index] = defaultTerrain;
            }
        }

        var chunks = PlaceDefiningMapFragment(room, fragment, roomParameters);

        // Verify the single hand-crafted fragment supplies enough doorway tiles in every
        // requested direction. EmptyRoomLayout has no flexibility (no extra fragments
        // placed), so failure here means the level fragment itself is mis-configured —
        // returning null lets the outer retry surface a sensible "fragment authoring"
        // error rather than crashing later in MaterializeGraph.
        return SatisfiesDemand(room, chunks, doorwayDemand) ? chunks : null;
    }
}
