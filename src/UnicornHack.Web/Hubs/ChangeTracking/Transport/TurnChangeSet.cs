using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public class TurnChangeSet
{
    [Key(0)]
    public PlayerChange PlayerState { get; set; } = null!;
    [Key(1)]
    public List<TurnEvent>? Events
    {
        get; set;
    }
}
