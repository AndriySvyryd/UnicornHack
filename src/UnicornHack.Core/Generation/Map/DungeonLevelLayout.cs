using CSharpScriptSerialization;

namespace UnicornHack.Generation.Map;

/// <summary>
///     <see cref="TreeLevelLayout" /> tuned for the main Dungeon branch. The introductory
///     first floor lives in the separate <c>entrance</c> branch driven by
///     <see cref="EntranceLevelLayout" />, so every Dungeon level is full strength.
/// </summary>
public class DungeonLevelLayout : TreeLevelLayout
{
    public new static readonly ICSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<DungeonLevelLayout>(GetPropertyConditions<DungeonLevelLayout>());

    public new ICSScriptSerializer GetSerializer() => Serializer;
}
