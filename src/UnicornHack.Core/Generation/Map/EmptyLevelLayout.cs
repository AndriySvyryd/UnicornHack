using CSharpScriptSerialization;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

/// <summary>
///     The simplest <see cref="LevelLayout" />: produces a single <see cref="RoomComponent" />
///     containing only the level's defining fragment. Used for trivial entries (the surface
///     branch) and tests that supply their own map.
/// </summary>
public class EmptyLevelLayout : LevelLayout, ICSScriptSerializable
{
    public override List<Chunk>? Fill(GameLevel level, Branch branch)
    {
        var room = GetEmptyEntranceRoom(level, level.Game.Manager);
        // EmptyLevelLayout produces a single room that is both the entrance and the exit.
        // The picked fragment is responsible for the level's content; for the surface this
        // is the hand-authored Surface fragment, for tests it's whatever the test registered.
        room.Features = RoomFeatures.Entrance | RoomFeatures.Exit;
        var fragment = LevelGenerator.PickFragmentForRoom(
            level, room.Features, level.GenerationStats);
        var roomParameters = fragment.GetRoomParameters(branch);

        AllocateTerrain(room, roomParameters.RoomWidth, roomParameters.RoomHeight,
            roomParameters.DefaultTerrain);
        // EmptyLevelLayout has no inter-room graph, so the room's only outgoing doorway
        // is whatever the level fragment authored. Pass an empty demand so the layout
        // doesn't reject the single hand-crafted chunk.
        var chunks = branch.RoomLayout.Fill(room, branch, fragment, []);
        if (chunks is null)
        {
            return null;
        }

        roomParameters.CreatureGenerator.Fill(room,
            chunks.Where(c => c.Fragment is not ConnectingMapFragment).ToList());
        roomParameters.ItemGenerator.Fill(room, chunks);
        return chunks;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<EmptyLevelLayout>(GetPropertyConditions<EmptyLevelLayout>());

    protected static Dictionary<string, Func<TEmptyLevelLayout, object?, bool>> GetPropertyConditions<TEmptyLevelLayout>()
        where TEmptyLevelLayout : EmptyLevelLayout => new();

    ICSScriptSerializer ICSScriptSerializable.GetSerializer() => Serializer;
}
