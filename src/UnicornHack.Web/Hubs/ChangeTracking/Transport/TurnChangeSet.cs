using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public partial class TurnChangeSet
{
    [Key(0)]
    public PlayerChange PlayerState { get; set; } = null!;
    [Key(1)]
    public List<object?>? Events
    {
        get; set;
    }
}
