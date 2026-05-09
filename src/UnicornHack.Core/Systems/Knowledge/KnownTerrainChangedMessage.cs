using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Knowledge;

public class KnownTerrainChangedMessage : IMessage
{
    public const string Name = "KnownTerrainChanged";

    public static KnownTerrainChangedMessage Create(GameManager manager)
        => manager.Queue.CreateMessage<KnownTerrainChangedMessage>(Name);

    public static void Enqueue(GameEntity roomEntity, int tilesExplored, GameManager manager)
    {
        var terrainChanged = Create(manager);
        terrainChanged.RoomEntity = roomEntity;
        terrainChanged.TilesExplored = tilesExplored;

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

    public int TilesExplored
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
        RoomEntity = default!;
        TilesExplored = default;
    }
}
