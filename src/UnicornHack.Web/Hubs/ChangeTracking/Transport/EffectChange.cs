using System.Collections;
using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public partial class EffectChange
{
    public const int PropertyCount = 5;

    [Key(0)]
    public BitArray? ChangedProperties { get; set; } = new BitArray(PropertyCount) { [0] = true };

    [Key(1)]
    public EffectType EffectType
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(1, true);
        }
    }

    [Key(2)]
    public int ExpirationTick
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(2, true);
        }
    }

    [Key(3)]
    public int Magnitude
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(3, true);
        }
    }

    [Key(4)]
    public bool InForce
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(4, true);
        }
    }
}
