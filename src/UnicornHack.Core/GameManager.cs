using UnicornHack.Utils.Caching;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack;

public partial class GameManager :
    EntityManager<GameEntity>,
    IMessageQueue,
    IGameSystem<RemoveComponentMessage>,
    IGameSystem<EntityReferenceMessage<GameEntity>>
{
    private readonly string[] _componentPropertyNames;

    public GameManager()
        : base((int)EntityComponent.ComponentCount, entityPoolSize: 32)
    {
        _componentPropertyNames = new string[(int)EntityComponent.ComponentCount];
    }

    public Game Game
    {
        get;
        set;
    } = null!;

    public new SequentialMessageQueue<GameManager> Queue => (SequentialMessageQueue<GameManager>)base.Queue;

    public ListObjectPool<List<(Point, byte)>> PointByteListArrayPool
    {
        get;
    } = new(() => new List<(Point, byte)>(), initialSize: 4, maxSize: 16, preallocatedCount: 0);

    public ListObjectPool<List<(int, byte)>> IntByteListArrayPool
    {
        get;
    } = new(() => new List<(int, byte)>(), initialSize: 4, maxSize: 16, preallocatedCount: 0);

    public ListObjectPool<List<(GameEntity, byte)>> GameEntityByteListArrayPool
    {
        get;
    } = new(() => new List<(GameEntity, byte)>(), initialSize: 4, maxSize: 16, preallocatedCount: 0);

    protected override void InitializeSystems(IMessageQueue queue)
    {
        var gameQueue = (SequentialMessageQueue<GameManager>)queue;
        gameQueue.Register<RemoveComponentMessage>(this, RemoveComponentMessage.Name, order: 0);
        gameQueue.Register<EntityReferenceMessage<GameEntity>>(this, EntityReferenceMessage<GameEntity>.Name, order: 0);

        InitializeLevels(gameQueue);
        InitializeBeings(gameQueue);
        InitializeAbilities(gameQueue);
        InitializeActors(gameQueue);
        InitializeItems(gameQueue);
        InitializeFaculties(gameQueue);
        InitializeSenses(gameQueue);
        InitializeEffects(gameQueue);
        InitializeTime(gameQueue);
        InitializeKnowledge(gameQueue);
    }

    public override int GetNextEntityId() => ++Game.NextEntityId;

    protected override GameEntity CreateEntityNoReference()
    {
        var entity = base.CreateEntityNoReference();
        Game.Repository.Add(entity);
        return entity;
    }

    public TComponent CreateComponent<TComponent>(EntityComponent componentId)
        where TComponent : GameComponent, new()
        => CreateComponent<TComponent>((int)componentId);

    protected void Add<TComponent>(EntityComponent componentId, int poolSize)
        where TComponent : Component, new()
        => Add<TComponent>((int)componentId, poolSize);

    protected override void Add<TComponent>(int componentId, int poolSize)
    {
        _componentPropertyNames[componentId] = ((EntityComponent)componentId).ToString();
        base.Add<TComponent>(componentId, poolSize);
    }

    public string GetComponentPropertyName(int componentId)
        => _componentPropertyNames[componentId];

    public GameEntity? FindEntity(int? id) => id.HasValue ? base.FindEntity(id.Value) : null;
    public override Entity? LoadEntity(int id) => Game.Repository.Find<GameEntity>(Game.Id, id);

    public override void RemoveFromSecondaryTracker(ITrackable trackable)
        => Game.Repository.RemoveTracked(trackable);

    public TMessage CreateMessage<TMessage>(string name)
        where TMessage : class, IMessage, new()
        => Queue.CreateMessage<TMessage>(name);

    public void ReturnMessage<TMessage>(TMessage message)
        where TMessage : class, IMessage, new()
        => Queue.ReturnMessage(message);

    public void Process<TMessage>(TMessage message)
        where TMessage : class, IMessage, new()
        => Queue.Process(message, this);

    MessageProcessingResult IMessageConsumer<RemoveComponentMessage, GameManager>.Process(
        RemoveComponentMessage message, GameManager manager)
    {
        message.Entity.RemoveComponent(message.Component);

        return MessageProcessingResult.ContinueProcessing;
    }

    MessageProcessingResult IMessageConsumer<EntityReferenceMessage<GameEntity>, GameManager>.Process(
        EntityReferenceMessage<GameEntity> message, GameManager state)
        => MessageProcessingResult.ContinueProcessing;
}
