using System.Collections;
using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public partial class PlayerChange
{
    public const int PropertyCount = 17;

    [Key(0)]
    public BitArray? ChangedProperties { get; set; } = new BitArray(PropertyCount) { [0] = true };

    [Key(1)]
    public string? Name
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(1, true);
        }
    }
    [Key(2)]
    public int Id
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(2, true);
        }
    }
    [Key(3)]
    public int PreviousTick
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(3, true);
        }
    }
    [Key(4)]
    public int CurrentTick
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(4, true);
        }
    }
    [Key(5)]
    public LevelChange? Level
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(5, true);
        }
    }
    [Key(6)]
    public Dictionary<int, RaceChange>? Races
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(6, true);
        }
    }
    [Key(7)]
    public Dictionary<int, AbilityChange>? Abilities
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(7, true);
        }
    }
    [Key(8)]
    public Dictionary<int, LogEntryChange>? Log
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(8, true);
        }
    }
    [Key(9)]
    public int NextActionTick
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(9, true);
        }
    }
    [Key(10)]
    public int XP
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(10, true);
        }
    }
    [Key(11)]
    public int Hp
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(11, true);
        }
    }
    [Key(12)]
    public int MaxHp
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(12, true);
        }
    }
    [Key(13)]
    public int Ep
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(13, true);
        }
    }
    [Key(14)]
    public int MaxEp
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(14, true);
        }
    }
    [Key(15)]
    public int ReservedEp
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(15, true);
        }
    }
    [Key(16)]
    public int Fortune
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(16, true);
        }
    }
}
