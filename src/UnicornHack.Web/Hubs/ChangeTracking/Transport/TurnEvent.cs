using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public class TurnEvent
{
    [Key(0)]
    public TurnEventType Type { get; set; }

    [Key(1)]
    public int Value1 { get; set; }

    [Key(2)]
    public int Value2 { get; set; }
}
