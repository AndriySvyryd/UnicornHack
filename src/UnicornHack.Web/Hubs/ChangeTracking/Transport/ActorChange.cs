using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public class ActorChange
{
    public const int PropertyCount = 13;
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
    public byte LevelX
    {
        get => field; set
        {
            field = value;
            MarkChanged(1);
        }
    }
    [Key(2)]
    public byte LevelY
    {
        get => field; set
        {
            field = value;
            MarkChanged(2);
        }
    }
    [Key(3)]
    public byte Heading
    {
        get => field; set
        {
            field = value;
            MarkChanged(3);
        }
    }
    [Key(4)]
    public string? BaseName
    {
        get => field; set
        {
            field = value;
            MarkChanged(4);
        }
    }
    [Key(5)]
    public string? Name
    {
        get => field; set
        {
            field = value;
            MarkChanged(5);
        }
    }
    [Key(6)]
    public bool IsCurrentlyPerceived
    {
        get => field; set
        {
            field = value;
            MarkChanged(6);
        }
    }
    [Key(7)]
    public int Hp
    {
        get => field; set
        {
            field = value;
            MarkChanged(7);
        }
    }
    [Key(8)]
    public int MaxHp
    {
        get => field; set
        {
            field = value;
            MarkChanged(8);
        }
    }
    [Key(9)]
    public int Ep
    {
        get => field; set
        {
            field = value;
            MarkChanged(9);
        }
    }
    [Key(10)]
    public int MaxEp
    {
        get => field; set
        {
            field = value;
            MarkChanged(10);
        }
    }
    [Key(11)]
    public int NextActionTick
    {
        get => field; set
        {
            field = value;
            MarkChanged(11);
        }
    }

    [Key(12)]
    public ActorActionChange? NextAction
    {
        get => field; set
        {
            field = value;
            MarkChanged(12);
        }
    }
}
