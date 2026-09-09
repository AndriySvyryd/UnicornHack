using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public class RoomMapChanges
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
    public List<(short, MapFeature)>? TerrainChanges
    {
        get => field; set
        {
            field = value;
            MarkChanged(1);
        }
    }
    [Key(2)]
    public List<(short, DirectionFlags)>? WallNeighborsChanges
    {
        get => field; set
        {
            field = value;
            MarkChanged(2);
        }
    }
    [Key(3)]
    public List<(short, byte)>? VisibleTerrainChanges
    {
        get => field; set
        {
            field = value;
            MarkChanged(3);
        }
    }
}
