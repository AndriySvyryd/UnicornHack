using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Senses;

public class VisibleTerrainChangedMessage : IMessage
{
    public const string Name = "VisibleTerrainChanged";

    public static VisibleTerrainChangedMessage Create(GameManager manager)
        => manager.Queue.CreateMessage<VisibleTerrainChangedMessage>(Name);

    public static void Enqueue(GameEntity roomEntity, GameManager manager)
    {
        var terrainChanged = Create(manager);
        terrainChanged.RoomEntity = roomEntity;

        manager.Enqueue(terrainChanged);
    }

    private GameEntity? _roomEntity;

    public GameEntity RoomEntity
    {
        get => _roomEntity!;
        set
        {
            _roomEntity?.RemoveReference(this);
            _roomEntity = value;
            _roomEntity?.AddReference(this);
        }
    }

    string IMessage.MessageName
    {
        get;
        set;
    } = null!;

    public void Clean()
        => RoomEntity = default!;
}
