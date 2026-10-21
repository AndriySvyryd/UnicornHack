using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public class RoomEntry
{
    [Key(0)]
    public int Id { get; set; }

    [Key(1)]
    public byte GridX { get; set; }

    [Key(2)]
    public byte GridY { get; set; }

    [Key(3)]
    public RoomFeatures Features { get; set; }

    [Key(4)]
    public bool IsVisited { get; set; }

    /// <summary>
    ///     Ids of rooms reachable from this room via revealed (non-hidden) connections.
    /// </summary>
    [Key(5)]
    public List<int> ConnectedTo { get; set; } = [];

    /// <summary>
    ///     Total actors other than the observer. Revealed in the current room and, at room
    ///     perception level 1 or higher, within room perception range through non-hidden
    ///     intra-level connections. <c>-1</c> when the count is not revealed.
    /// </summary>
    [Key(6)]
    public int RevealedActorCount { get; set; } = -1;

    /// <summary>
    ///     Pre-composed, localized hover tooltip text for this room cell. Built server-side
    ///     so it goes through the language service and stays consistent with the rest of the
    ///     localized payload.
    /// </summary>
    [Key(7)]
    public string? Tooltip { get; set; }

    [Key(8)]
    public string? EliteName { get; set; }
}
