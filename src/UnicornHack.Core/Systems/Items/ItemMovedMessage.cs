using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Items;

public class ItemMovedMessage : IMessage
{
    public const string Name = "ItemMoved";

    public static ItemMovedMessage Create(GameManager manager)
        => manager.Queue.CreateMessage<ItemMovedMessage>(Name);

    private GameEntity? _itemEntity;
    private GameEntity? _initialContainer;
    private GameEntity? _initialRoomEntity;

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

    /// <summary>
    ///     The room the item started in (the room owning <see cref="InitialRoomCell" /> for a
    ///     ground item, or the room of the top container for an item moved out of an
    ///     inventory). Needed because <see cref="InitialRoomCell" /> is only a
    ///     <see cref="Point" />: a sensor must verify the cell belongs to its own room before
    ///     testing visibility, otherwise the coordinates would be indexed against an unrelated
    ///     room's visibility map and yield false positives.
    /// </summary>
    public GameEntity? InitialRoomEntity
    {
        get => _initialRoomEntity;
        set
        {
            _initialRoomEntity?.RemoveReference(this);
            _initialRoomEntity = value;
            _initialRoomEntity?.AddReference(this);
        }
    }

    public Point? InitialRoomCell
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
        InitialRoomEntity = default;
        InitialRoomCell = default;
        InitialCount = default;
        Successful = default;
        SuppressLog = default;
    }
}
