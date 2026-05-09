using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Levels;

/// <summary>
///     Raised by <see cref="TravelSystem" /> when an actor's travel resolution lands them
///     on a tile that holds a <see cref="ConnectionComponent" />. Carries a reference to
///     the connection entity so downstream consumers (e.g. the Web layer's per-turn event
///     builder) can classify the connection's scope — same level, different level on the
///     same branch, or a different branch — without re-walking the actor's position.
/// </summary>
public class SteppedOnConnectionMessage : IMessage
{
    public const string Name = "SteppedOnConnection";

    public static SteppedOnConnectionMessage Create(GameManager manager)
        => manager.Queue.CreateMessage<SteppedOnConnectionMessage>(Name);

    private GameEntity? _actorEntity;
    private GameEntity? _connectionEntity;

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

    public GameEntity ConnectionEntity
    {
        get => _connectionEntity!;
        set
        {
            _connectionEntity?.RemoveReference(this);
            _connectionEntity = value;
            _connectionEntity?.AddReference(this);
        }
    }

    string IMessage.MessageName
    {
        get;
        set;
    } = null!;

    public void Clean()
    {
        ActorEntity = default!;
        ConnectionEntity = default!;
    }
}
