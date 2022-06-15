using UnicornHack.Primitives;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Items;

public class ItemEquippedMessage : IMessage
{
    public const string Name = "ItemEquipped";

    public static ItemEquippedMessage Create(GameManager manager)
        => manager.Queue.CreateMessage<ItemEquippedMessage>(Name);

    private GameEntity _itemEntity;
    private GameEntity _actorEntity;

    public GameEntity ItemEntity
    {
        get => _itemEntity;
        set
        {
            _itemEntity?.RemoveReference(this);
            _itemEntity = value;
            _itemEntity?.AddReference(this);
        }
    }

    public GameEntity ActorEntity
    {
        get => _actorEntity;
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

    public EquipmentSlot OldSlot
    {
        get;
        set;
    }

    public bool Successful
    {
        get;
        set;
    }

    public bool SuppressLog
    {
        get;
        set;
    }

    string IMessage.MessageName
    {
        get;
        set;
    }

    public void Clean()
    {
        ItemEntity = default;
        ActorEntity = default;
        Slot = default;
        OldSlot = default;
        Successful = default;
        SuppressLog = default;
    }
}
