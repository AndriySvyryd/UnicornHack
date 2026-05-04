using System.Collections;
using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public partial class ActorChange
{
    public const int PropertyCount = 17;

    [Key(0)]
    public BitArray? ChangedProperties { get; set; } = new BitArray(PropertyCount) { [0] = true };
    [Key(1)]
    public byte LevelX
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(1, true);
        }
    }
    [Key(2)]
    public byte LevelY
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(2, true);
        }
    }
    [Key(3)]
    public byte Heading
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(3, true);
        }
    }
    [Key(4)]
    public string? BaseName
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(4, true);
        }
    }
    [Key(5)]
    public string? Name
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(5, true);
        }
    }
    [Key(6)]
    public bool IsCurrentlyPerceived
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(6, true);
        }
    }
    [Key(7)]
    public int Hp
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(7, true);
        }
    }
    [Key(8)]
    public int MaxHp
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(8, true);
        }
    }
    [Key(9)]
    public int Ep
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(9, true);
        }
    }
    [Key(10)]
    public int MaxEp
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(10, true);
        }
    }
    [Key(11)]
    public int NextActionTick
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(11, true);
        }
    }
    [Key(12)]
    public ActorActionChange? NextAction
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(12, true);
        }
    }
    [Key(13)]
    public AttackSummary? MeleeAttack
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(13, true);
        }
    }
    [Key(14)]
    public AttackSummary? RangeAttack
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(14, true);
        }
    }
    [Key(15)]
    public AttackSummary? MeleeDefense
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(15, true);
        }
    }
    [Key(16)]
    public AttackSummary? RangeDefense
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(16, true);
        }
    }
}
