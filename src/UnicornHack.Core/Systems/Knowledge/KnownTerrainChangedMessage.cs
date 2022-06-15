using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Senses;

public class KnownTerrainChangedMessage : IMessage
{
    public const string Name = "KnownTerrainChanged";

    public static KnownTerrainChangedMessage Create(GameManager manager)
        => manager.Queue.CreateMessage<KnownTerrainChangedMessage>(Name);

    public static void Enqueue(GameEntity levelEntity, int tilesExplored, GameManager manager)
    {
        var terrainChanged = Create(manager);
        terrainChanged.LevelEntity = levelEntity;
        terrainChanged.TilesExplored = tilesExplored;

        manager.Enqueue(terrainChanged);
    }

    private GameEntity _levelEntity;

    public GameEntity LevelEntity
    {
        get => _levelEntity;
        set
        {
            _levelEntity?.RemoveReference(this);
            _levelEntity = value;
            _levelEntity?.AddReference(this);
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
    }

    public void Clean()
    {
        LevelEntity = default;
        TilesExplored = default;
    }
}
