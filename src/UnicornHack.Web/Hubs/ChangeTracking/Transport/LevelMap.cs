using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public class LevelMap
{
    [Key(0)]
    public List<RoomEntry> Rooms { get; set; } = [];

    /// <summary>
    ///     Room entity ids that the player can travel to right now.
    ///     Empty if the player can't travel (e.g. not on a door and hostiles present).
    /// </summary>
    [Key(1)]
    public List<int> AvailableTargets { get; set; } = [];
}
