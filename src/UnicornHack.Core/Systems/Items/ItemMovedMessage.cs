using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Items;

public class ItemMovedMessage : IMessage
{
    public const string Name = "ItemMoved";

    public static ItemMovedMessage Create(GameManager manager)
        => manager.Queue.CreateMessage<ItemMovedMessage>(Name);

    private GameEntity? _itemEntity;
    private GameEntity? _initialContainer;

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

    public GameEntity? InitialContainer
    {
        get => _initialContainer;
        set
        {
            _initialContainer?.RemoveReference(this);
            _initialContainer = value;
            _initialContainer?.AddReference(this);
        }
    }

    public Point? InitialLevelCell
    {
        get;
        set;
    }

    public int InitialCount
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
    } = null!;

    public void Clean()
    {
        ItemEntity = default!;
        InitialContainer = default;
        InitialLevelCell = default;
        InitialCount = default;
        Successful = default;
        SuppressLog = default;
    }
}
