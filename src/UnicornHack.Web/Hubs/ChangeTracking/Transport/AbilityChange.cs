using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public class AbilityChange
{
    public const int PropertyCount = 15;
    private ushort? _changedProperties = 1;

    [Key(0)]
    public ushort? ChangedProperties
    {
        get => _changedProperties;
        set => _changedProperties = value;
    }

    private void MarkChanged(int propertyIndex)
    {
        if (_changedProperties.HasValue)
        {
            _changedProperties = (ushort)(_changedProperties.GetValueOrDefault() | (1 << propertyIndex));
        }
    }
    [Key(1)]
    public string? Name
    {
        get => field; set
        {
            field = value;
            MarkChanged(1);
        }
    }
    [Key(2)]
    public int Activation
    {
        get => field; set
        {
            field = value;
            MarkChanged(2);
        }
    }
    [Key(3)]
    public int? Slot
    {
        get => field; set
        {
            field = value;
            MarkChanged(3);
        }
    }
    [Key(4)]
    public int? CooldownTick
    {
        get => field; set
        {
            field = value;
            MarkChanged(4);
        }
    }
    [Key(5)]
    public int? CooldownXpLeft
    {
        get => field; set
        {
            field = value;
            MarkChanged(5);
        }
    }
    [Key(6)]
    public int TargetingShape
    {
        get => field; set
        {
            field = value;
            MarkChanged(6);
        }
    }
    [Key(7)]
    public int TargetingShapeSize
    {
        get => field; set
        {
            field = value;
            MarkChanged(7);
        }
    }
    [Key(8)]
    public int Type
    {
        get => field; set
        {
            field = value;
            MarkChanged(8);
        }
    }
    [Key(9)]
    public int? Accumulator
    {
        get => field; set
        {
            field = value;
            MarkChanged(9);
        }
    }
    [Key(10)]
    public int Level
    {
        get => field; set
        {
            field = value;
            MarkChanged(10);
        }
    }
    [Key(11)]
    public int LevelReduction
    {
        get => field; set
        {
            field = value;
            MarkChanged(11);
        }
    }
    [Key(12)]
    public int? ExpirationTarget
    {
        get => field; set
        {
            field = value;
            MarkChanged(12);
        }
    }
    [Key(13)]
    public int? ExpirationEvent
    {
        get => field; set
        {
            field = value;
            MarkChanged(13);
        }
    }
    [Key(14)]
    public int? Noise
    {
        get => field; set
        {
            field = value;
            MarkChanged(14);
        }
    }
}
