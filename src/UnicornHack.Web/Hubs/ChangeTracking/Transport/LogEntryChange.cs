using System.Collections;
using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public partial class LogEntryChange
{
    public const int PropertyCount = 3;

    [Key(0)]
    public BitArray? ChangedProperties { get; set; } = new BitArray(PropertyCount) { [0] = true };

    [Key(1)]
    public string? Message
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(1, true);
        }
    }
    [Key(2)]
    public int Ticks
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(2, true);
        }
    }
}
