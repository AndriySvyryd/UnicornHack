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
    ///     Number of hostile / neutral actors in the room. Revealed for the player's current
    ///     room and adjacent (1 hop) rooms, and for any visited room. <c>-1</c> when the count
    ///     is not yet revealed at the player's current closeness to this room.
    /// </summary>
    [Key(6)]
    public int RevealedActorCount { get; set; } = -1;

    /// <summary>
    ///     Number of items in the room. Same reveal rules as <see cref="RevealedActorCount" />.
    /// </summary>
    [Key(7)]
    public int RevealedItemCount { get; set; } = -1;

    /// <summary>
    ///     Localized name of the actor with the highest <c>HitPointMaximum</c> in the room
    ///     (a stand-in "strongest creature" metric). Set only for visited rooms.
    /// </summary>
    [Key(8)]
    public string? StrongestActorName { get; set; }

    /// <summary>
    ///     Pre-composed, localized hover tooltip text for this room cell. Built server-side
    ///     so it goes through the language service and stays consistent with the rest of the
    ///     localized payload.
    /// </summary>
    [Key(9)]
    public string? Tooltip { get; set; }
}
