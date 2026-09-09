using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public class RoomChange
{
    public const int PropertyCount = 12;
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
    public Dictionary<int, ActorChange>? Actors
    {
        get => field; set
        {
            field = value;
            MarkChanged(1);
        }
    }
    [Key(2)]
    public Dictionary<int, ItemChange>? Items
    {
        get => field; set
        {
            field = value;
            MarkChanged(2);
        }
    }
    [Key(3)]
    public Dictionary<int, ConnectionChange>? Connections
    {
        get => field; set
        {
            field = value;
            MarkChanged(3);
        }
    }
    [Key(4)]
    public RoomMap? LevelMap
    {
        get => field; set
        {
            field = value;
            MarkChanged(4);
        }
    }
    [Key(5)]
    public RoomMapChanges? LevelMapChanges
    {
        get => field; set
        {
            field = value;
            MarkChanged(5);
        }
    }
    [Key(6)]
    public string? BranchName
    {
        get => field; set
        {
            field = value;
            MarkChanged(6);
        }
    }
    [Key(7)]
    public int Depth
    {
        get => field; set
        {
            field = value;
            MarkChanged(7);
        }
    }
    [Key(8)]
    public int Width
    {
        get => field; set
        {
            field = value;
            MarkChanged(8);
        }
    }
    [Key(9)]
    public int Height
    {
        get => field; set
        {
            field = value;
            MarkChanged(9);
        }
    }
    [Key(10)]
    public byte GridX
    {
        get => field; set
        {
            field = value;
            MarkChanged(10);
        }
    }
    [Key(11)]
    public byte GridY
    {
        get => field; set
        {
            field = value;
            MarkChanged(11);
        }
    }
}
