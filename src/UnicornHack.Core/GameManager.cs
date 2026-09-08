using UnicornHack.Utils.Caching;
using UnicornHack.Utils.MessagingECS;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;

namespace UnicornHack;

public partial class GameManager :
    EntityManager<GameEntity>,
    IMessageQueue,
    IGameSystem<RemoveComponentMessage>,
    IGameSystem<EntityReferenceMessage<GameEntity>>,
    IGameSystem<DeleteEntityMessage>
{
    private readonly string[] _componentPropertyNames;
    private List<PooledTrackableCollectionItem>? _itemsToRelease;

    public GameManager()
        : base((int)EntityComponent.ComponentCount,
         entityPoolSize: 8192, entityMaxPoolSize: 16384, entityDictionaryCapacity: 16384)
    {
        _componentPropertyNames = new string[(int)EntityComponent.ComponentCount];
    }

    public Game Game
    {
        get;
        set;
    } = null!;

    public new SequentialMessageQueue<GameEntity> Queue => (SequentialMessageQueue<GameEntity>)base.Queue;

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
        var gameQueue = (SequentialMessageQueue<GameEntity>)queue;
        gameQueue.Register<RemoveComponentMessage>(this, RemoveComponentMessage.Name, order: 0);
        gameQueue.Register<EntityReferenceMessage<GameEntity>>(this, EntityReferenceMessage<GameEntity>.Name, order: 0);
        gameQueue.Register<DeleteEntityMessage>(this, DeleteEntityMessage.Name, order: 0);

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

    public override object? Rent(Type type)
    {
        if (type == typeof(GameEntity))
        {
            return base.Rent(type);
        }

        if (type == typeof(EffectComponent))
        {
            return CreateComponent<EffectComponent>(EntityComponent.Effect);
        }

        if (type == typeof(AbilityComponent))
        {
            return CreateComponent<AbilityComponent>(EntityComponent.Ability);
        }

        if (type == typeof(ActivePropertyEffect))
        {
            return ActivePropertyEffectPool.Rent();
        }

        if (type == typeof(ActiveEffectsComponent))
        {
            return CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);
        }

        if (type == typeof(ActiveComponentEffect))
        {
            return ActiveComponentEffectPool.Rent();
        }

        if (type == typeof(EffectAccumulator))
        {
            return EffectAccumulatorPool.Rent();
        }

        if (type == typeof(BeingComponent))
        {
            return CreateComponent<BeingComponent>(EntityComponent.Being);
        }

        if (type == typeof(PhysicalComponent))
        {
            return CreateComponent<PhysicalComponent>(EntityComponent.Physical);
        }

        if (type == typeof(PositionComponent))
        {
            return CreateComponent<PositionComponent>(EntityComponent.Position);
        }

        if (type == typeof(AIComponent))
        {
            return CreateComponent<AIComponent>(EntityComponent.AI);
        }

        if (type == typeof(ItemComponent))
        {
            return CreateComponent<ItemComponent>(EntityComponent.Item);
        }

        if (type == typeof(ConnectionComponent))
        {
            return CreateComponent<ConnectionComponent>(EntityComponent.Connection);
        }

        if (type == typeof(KnowledgeComponent))
        {
            return CreateComponent<KnowledgeComponent>(EntityComponent.Knowledge);
        }

        if (type == typeof(RoomComponent))
        {
            return CreateComponent<RoomComponent>(EntityComponent.Room);
        }

        if (type == typeof(PlayerComponent))
        {
            return CreateComponent<PlayerComponent>(EntityComponent.Player);
        }

        return null;
    }

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

    protected void Add<TComponent>(EntityComponent componentId, int poolSize, int maxPoolSize)
        where TComponent : Component, new()
        => Add<TComponent>((int)componentId, poolSize, maxPoolSize);

    protected override void Add<TComponent>(int componentId, int poolSize, int maxPoolSize)
    {
        _componentPropertyNames[componentId] = ((EntityComponent)componentId).ToString();
        base.Add<TComponent>(componentId, poolSize, maxPoolSize);
    }

    public override string GetComponentPropertyName(int componentId)
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

    protected internal override void OnMessageQueueDrained()
    {
        if (_itemsToRelease == null)
        {
            return;
        }

        foreach (var item in _itemsToRelease)
        {
            item.Release(this);
        }

        _itemsToRelease.Clear();
    }

    internal void AddToReleaseQueue(PooledTrackableCollectionItem item)
        => (_itemsToRelease ??= []).Add(item);

    MessageProcessingResult IMessageConsumer<RemoveComponentMessage, GameManager>.Process(
        RemoveComponentMessage message, GameManager manager)
    {
        message.Entity.RemoveComponent(message.Component);

        return MessageProcessingResult.ContinueProcessing;
    }

    MessageProcessingResult IMessageConsumer<DeleteEntityMessage, GameManager>.Process(
        DeleteEntityMessage message, GameManager manager)
    {
        RemoveAllComponents(message.Entity);

        return MessageProcessingResult.ContinueProcessing;
    }

    /// <summary>
    ///     Removes every component from <paramref name="entity" />.
    ///     Removing the last keep-alive component drops the entity's reference count to zero,
    ///     tearing it (and everything kept alive solely by it) down. Stops early if the entity is
    ///     disposed mid-way; safe to call on an already-disposed entity.
    /// </summary>
    private static void RemoveAllComponents(GameEntity entity)
    {
        for (var componentId = (int)EntityComponent.ComponentCount - 1; componentId >= 0; componentId--)
        {
            if (entity.Id == 0)
            {
                return;
            }

            if (entity.HasComponent(componentId))
            {
                entity.RemoveComponent((EntityComponent)componentId);
            }
        }
    }

    MessageProcessingResult IMessageConsumer<EntityReferenceMessage<GameEntity>, GameManager>.Process(
        EntityReferenceMessage<GameEntity> message, GameManager state)
        => MessageProcessingResult.ContinueProcessing;
}
