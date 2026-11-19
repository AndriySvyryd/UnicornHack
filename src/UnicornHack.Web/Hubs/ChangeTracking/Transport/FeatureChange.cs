using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public class FeatureChange
{
    public const int PropertyCount = 11;

    [Key(0)]
    public ushort? ChangedProperties { get; set; } = 1;

    private void MarkChanged(int index)
    {
        if (ChangedProperties is { } mask)
        {
            ChangedProperties = (ushort)(mask | (1 << index));
        }
    }

    [Key(1)]
    public string Name { get => field; set { field = value; MarkChanged(1); } } = "";

    [Key(2)]
    public int LevelX
    {
        get => field; set
        {
            field = value;
            MarkChanged(2);
        }
    }

    [Key(3)]
    public int LevelY
    {
        get => field; set
        {
            field = value;
            MarkChanged(3);
        }
    }

    [Key(4)]
    public bool Destroyed
    {
        get => field; set
        {
            field = value;
            MarkChanged(4);
        }
    }

    [Key(5)]
    public bool BlocksMovement
    {
        get => field; set
        {
            field = value;
            MarkChanged(5);
        }
    }

    [Key(6)]
    public int? NextActionTick
    {
        get => field; set
        {
            field = value;
            MarkChanged(6);
        }
    }

    [Key(7)]
    public int? CooldownTick
    {
        get => field; set
        {
            field = value;
            MarkChanged(7);
        }
    }

    [Key(8)]
    public ActorActionChange[]? NextActions
    {
        get => field; set
        {
            field = value;
            MarkChanged(8);
        }
    }

    [Key(9)]
    public int[] AffectedCells { get => field; set { field = value; MarkChanged(9); } } = [];

    [Key(10)]
    public bool IsCurrentlyPerceived { get => field; set { field = value; MarkChanged(10); } }
}
