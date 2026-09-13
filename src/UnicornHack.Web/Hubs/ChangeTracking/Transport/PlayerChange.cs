using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public class PlayerChange
{
    public const int PropertyCount = 18;
    private uint? _changedProperties = 1;

    [Key(0)]
    public uint? ChangedProperties
    {
        get => _changedProperties;
        set => _changedProperties = value;
    }

    private void MarkChanged(int propertyIndex)
    {
        if (_changedProperties.HasValue)
        {
            _changedProperties = _changedProperties.GetValueOrDefault() | (1u << propertyIndex);
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
    public int Id
    {
        get => field; set
        {
            field = value;
            MarkChanged(2);
        }
    }
    [Key(3)]
    public int PreviousTick
    {
        get => field; set
        {
            field = value;
            MarkChanged(3);
        }
    }
    [Key(4)]
    public int CurrentTick
    {
        get => field; set
        {
            field = value;
            MarkChanged(4);
        }
    }
    [Key(5)]
    public int NextActionTick
    {
        get => field; set
        {
            field = value;
            MarkChanged(5);
        }
    }
    [Key(6)]
    public RoomChange? Room
    {
        get => field; set
        {
            field = value;
            MarkChanged(6);
        }
    }
    [Key(7)]
    public Dictionary<int, AbilityChange>? Abilities
    {
        get => field; set
        {
            field = value;
            MarkChanged(7);
        }
    }
    [Key(8)]
    public Dictionary<int, EffectChange>? Effects
    {
        get => field; set
        {
            field = value;
            MarkChanged(8);
        }
    }
    [Key(9)]
    public Dictionary<int, LogEntryChange>? Log
    {
        get => field; set
        {
            field = value;
            MarkChanged(9);
        }
    }
    [Key(10)]
    public int XP
    {
        get => field; set
        {
            field = value;
            MarkChanged(10);
        }
    }
    [Key(11)]
    public int Hp
    {
        get => field; set
        {
            field = value;
            MarkChanged(11);
        }
    }
    [Key(12)]
    public int MaxHp
    {
        get => field; set
        {
            field = value;
            MarkChanged(12);
        }
    }
    [Key(13)]
    public int Ep
    {
        get => field; set
        {
            field = value;
            MarkChanged(13);
        }
    }
    [Key(14)]
    public int MaxEp
    {
        get => field; set
        {
            field = value;
            MarkChanged(14);
        }
    }
    [Key(15)]
    public int ReservedEp
    {
        get => field; set
        {
            field = value;
            MarkChanged(15);
        }
    }
    [Key(16)]
    public int Fortune
    {
        get => field; set
        {
            field = value;
            MarkChanged(16);
        }
    }

    [Key(17)]
    public PlayerOptionsData? PendingOptions
    {
        get => field; set
        {
            field = value;
            MarkChanged(17);
        }
    }

}
