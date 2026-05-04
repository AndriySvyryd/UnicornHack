using System.Collections;
using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public partial class AbilityChange
{
    public const int PropertyCount = 8;

    [Key(0)]
    public BitArray? ChangedProperties { get; set; } = new BitArray(PropertyCount) { [0] = true };
    [Key(1)]
    public string? Name
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(1, true);
        }
    }
    [Key(2)]
    public int Activation
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(2, true);
        }
    }
    [Key(3)]
    public int? Slot
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(3, true);
        }
    }
    [Key(4)]
    public int? CooldownTick
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(4, true);
        }
    }
    [Key(5)]
    public int? CooldownXpLeft
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(5, true);
        }
    }
    [Key(6)]
    public int TargetingShape
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(6, true);
        }
    }
    [Key(7)]
    public int TargetingShapeSize
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(7, true);
        }
    }
}
