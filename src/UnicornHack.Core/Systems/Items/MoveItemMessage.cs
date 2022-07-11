using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Items;

public class MoveItemMessage : IMessage
{
    public const string Name = "MoveItem";

    public static MoveItemMessage Create(GameManager manager)
        => manager.Queue.CreateMessage<MoveItemMessage>(Name);

    private GameEntity? _itemEntity;
    private GameEntity? _targetContainerEntity;
    private GameEntity? _targetLevelEntity;

    public GameEntity ItemEntity
    {
        get => _itemEntity!;
        set
        {
            _itemEntity?.RemoveReference(this);
            _itemEntity = value;
            _itemEntity?.AddReference(this);
        }
    }

    public GameEntity? TargetContainerEntity
    {
        get => _targetContainerEntity;
        set
        {
            _targetContainerEntity?.RemoveReference(this);
            _targetContainerEntity = value;
            _targetContainerEntity?.AddReference(this);
        }
    }

    public GameEntity? TargetLevelEntity
    {
        get => _targetLevelEntity;
        set
        {
            _targetLevelEntity?.RemoveReference(this);
            _targetLevelEntity = value;
            _targetLevelEntity?.AddReference(this);
        }
    }

    public Point? TargetCell
    {
        get;
        set;
    }

    public bool SuppressLog
    {
        get;
        set;
    }

    public bool Force
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
        ItemEntity = default!;
        TargetContainerEntity = default;
        TargetLevelEntity = default;
        TargetCell = default;
        SuppressLog = default;
        Force = default;
    }
}
