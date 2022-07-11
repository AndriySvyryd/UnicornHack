using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Items;

public class EquipItemMessage : IMessage
{
    public const string Name = "EquipItem";

    public static EquipItemMessage Create(GameManager manager)
        => manager.Queue.CreateMessage<EquipItemMessage>(Name);

    private GameEntity? _itemEntity;
    private GameEntity? _actorEntity;

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

    public GameEntity ActorEntity
    {
        get => _actorEntity!;
        set
        {
            _actorEntity?.RemoveReference(this);
            _actorEntity = value;
            _actorEntity?.AddReference(this);
        }
    }

    public EquipmentSlot Slot
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
        ActorEntity = default!;
        Slot = default;
        SuppressLog = default;
        Force = default;
    }
}
