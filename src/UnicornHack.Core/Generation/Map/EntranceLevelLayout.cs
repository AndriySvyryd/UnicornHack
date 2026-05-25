using CSharpScriptSerialization;

namespace UnicornHack.Generation.Map;

/// <summary>
///     <see cref="TreeLevelLayout" /> tuned for the introductory <c>entrance</c> branch:
///     a single short level placed before <c>dungeon</c> so new players get a small,
///     low-pressure first room set (few rooms, sparse graph, no curses, no hidden
///     connections, more rewards).
/// </summary>
public class EntranceLevelLayout : TreeLevelLayout
{
    public EntranceLevelLayout()
    {
        GridWidth = 5;
        GridHeight = 3;
        Interconnectedness = 0.25f;
        Sinuosity = 0.1f;
        RewardRoomProbability = 0.6f;
        CurseRoomProbability = 0.0f;
        HiddenConnectionProbability = 0.0f;
        MinRooms = 8;
        MaxRooms = 14;
    }

    public new static readonly ICSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<EntranceLevelLayout>(GetPropertyConditions<EntranceLevelLayout>());

    public new ICSScriptSerializer GetSerializer() => Serializer;
}
