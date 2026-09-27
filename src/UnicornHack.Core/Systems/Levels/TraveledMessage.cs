using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Levels;

public class TraveledMessage : IMessage
{
    public const string Name = "Traveled";

    public static TraveledMessage Create(GameManager manager)
        => manager.Queue.CreateMessage<TraveledMessage>(Name);

    private GameEntity? _entity;
    private GameEntity? _initialLevel;

    public GameEntity Entity
    {
        get => _entity!;
        set
        {
            _entity?.RemoveReference(this);
            _entity = value;
            _entity?.AddReference(this);
        }
    }

    public GameEntity InitialLevel
    {
        get => _initialLevel!;
        set
        {
            _initialLevel?.RemoveReference(this);
            _initialLevel = value;
            _initialLevel?.AddReference(this);
        }
    }

    public Direction InitialHeading
    {
        get;
        set;
    }

    public Point InitialLevelCell
    {
        get;
        set;
    }

    public string InitialBranchName
    {
        get;
        set;
    } = null!;

    public byte InitialDepth
    {
        get;
        set;
    }

    public Direction TargetHeading
    {
        get;
        set;
    }

    public Point TargetCell
    {
        get;
        set;
    }

    public bool Successful
    {
        get;
        set;
    }

    public int Delay
    {
        get;
        set;
    }

    string IMessage.MessageName
    {
        get => Name;
        set => Debug.Assert(value == Name);
    }

    public void Clean()
    {
        Entity = default!;
        InitialLevel = default!;
        InitialHeading = default;
        InitialLevelCell = default;
        InitialBranchName = default!;
        InitialDepth = default;
        TargetHeading = default;
        TargetCell = default;
        Successful = default;
        Delay = default;
    }
}
