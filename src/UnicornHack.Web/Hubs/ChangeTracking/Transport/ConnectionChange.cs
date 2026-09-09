using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public class ConnectionChange
{
    public const int PropertyCount = 4;
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
    public Direction Orientation
    {
        get => field; set
        {
            field = value;
            MarkChanged(3);
        }
    }
}
