using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public class ItemChange
{
    public const int PropertyCount = 7;
    private byte? _changedProperties = 1;

    [Key(0)]
    public byte? ChangedProperties
    {
        get => _changedProperties;
        set => _changedProperties = value;
    }

    private void MarkChanged(int propertyIndex)
    {
        if (_changedProperties.HasValue)
        {
            _changedProperties = (byte)(_changedProperties.GetValueOrDefault() | (1 << propertyIndex));
        }
    }
    [Key(1)]
    public int LevelX
    {
        get => field; set
        {
            field = value;
            MarkChanged(1);
        }
    }
    [Key(2)]
    public int LevelY
    {
        get => field; set
        {
            field = value;
            MarkChanged(2);
        }
    }
    [Key(3)]
    public ItemType Type
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
}
