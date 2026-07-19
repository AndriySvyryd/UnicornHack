using System.Collections;
using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public partial class AbilityChange
{
    public const int PropertyCount = 15;

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
    [Key(8)]
    public int Type
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(8, true);
        }
    }
    [Key(9)]
    public int? Accumulator
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(9, true);
        }
    }
    [Key(10)]
    public int Level
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(10, true);
        }
    }
    [Key(11)]
    public int LevelReduction
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(11, true);
        }
    }
    [Key(12)]
    public int? ExpirationTarget
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(12, true);
        }
    }
    [Key(13)]
    public int? ExpirationEvent
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(13, true);
        }
    }
    [Key(14)]
    public int? Noise
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(14, true);
        }
    }
}
