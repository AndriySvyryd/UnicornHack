using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public class EffectChange
{
    public const int PropertyCount = 5;
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
    public EffectType EffectType
    {
        get => field; set
        {
            field = value;
            MarkChanged(1);
        }
    }

    [Key(2)]
    public int ExpirationTick
    {
        get => field; set
        {
            field = value;
            MarkChanged(2);
        }
    }

    [Key(3)]
    public int Magnitude
    {
        get => field; set
        {
            field = value;
            MarkChanged(3);
        }
    }

    [Key(4)]
    public bool InForce
    {
        get => field; set
        {
            field = value;
            MarkChanged(4);
        }
    }
}
