using System.Collections;
using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public partial class ConnectionChange
{
    public const int PropertyCount = 4;

    [Key(0)]
    public BitArray? ChangedProperties { get; set; } = new BitArray(PropertyCount) { [0] = true };
    [Key(1)]
    public int LevelX
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(1, true);
        }
    }
    [Key(2)]
    public int LevelY
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(2, true);
        }
    }
    [Key(3)]
    public bool IsDown
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(3, true);
        }
    }
}
