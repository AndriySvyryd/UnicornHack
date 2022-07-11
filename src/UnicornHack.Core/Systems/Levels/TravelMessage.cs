using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Levels;

public class TravelMessage : IMessage
{
    public const string Name = "Travel";

    public static TravelMessage Create(GameManager manager)
        => manager.Queue.CreateMessage<TravelMessage>(Name);

    private GameEntity? _entity;

    public GameEntity ActorEntity
    {
        get => _entity!;
        set
        {
            _entity?.RemoveReference(this);
            _entity = value;
            _entity?.AddReference(this);
        }
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

    public bool MoveOffConflicting
    {
        get;
        set;
    }

    string IMessage.MessageName
    {
        get;
        set;
    } = null!;

    public void Clean()
    {
        ActorEntity = default!;
        TargetHeading = default;
        TargetCell = default;
        MoveOffConflicting = default;
    }
}
