using System.Collections;
using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public partial class ItemChange
{
    public const int PropertyCount = 7;

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
    public ItemType Type
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(3, true);
        }
    }
    [Key(4)]
    public string? BaseName
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(4, true);
        }
    }
    [Key(5)]
    public string? Name
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(5, true);
        }
    }
    [Key(6)]
    public bool IsCurrentlyPerceived
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(6, true);
        }
    }
}
