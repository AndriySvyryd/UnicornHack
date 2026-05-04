using System.Collections;
using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public partial class LevelChange
{
    public const int PropertyCount = 10;

    [Key(0)]
    public BitArray? ChangedProperties { get; set; } = new BitArray(PropertyCount) { [0] = true };

    [Key(1)]
    public Dictionary<int, ActorChange>? Actors
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(1, true);
        }
    }
    [Key(2)]
    public Dictionary<int, ItemChange>? Items
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(2, true);
        }
    }
    [Key(3)]
    public Dictionary<int, ConnectionChange>? Connections
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(3, true);
        }
    }
    [Key(4)]
    public LevelMap? LevelMap
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(4, true);
        }
    }
    [Key(5)]
    public LevelMapChanges? LevelMapChanges
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(5, true);
        }
    }
    [Key(6)]
    public string? BranchName
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(6, true);
        }
    }
    [Key(7)]
    public int Depth
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(7, true);
        }
    }
    [Key(8)]
    public int Width
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(8, true);
        }
    }
    [Key(9)]
    public int Height
    {
        get => field; set
        {
            field = value;
            ChangedProperties?.Set(9, true);
        }
    }
}
