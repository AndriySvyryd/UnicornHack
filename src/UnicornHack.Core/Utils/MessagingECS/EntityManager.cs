using UnicornHack.Utils.Caching;

namespace UnicornHack.Utils.MessagingECS;

public abstract class EntityManager<TEntity> : IEntityManager
    where TEntity : Entity, new()
{
    private readonly SnapshotableDictionary<int, TEntity> _entities;

    private readonly List<EntityGroup<TEntity>>?[] _groupsByComponentId;
    private readonly ListObjectPool<TEntity> _entityPool;
    private readonly IObjectPool?[] _componentPools;

    // entityPoolSize is the eager initial fill; entityMaxPoolSize caps retained instances for reuse;
    // entityDictionaryCapacity pre-sizes the per-game entity store so it doesn't grow-and-realloc its
    // backing arrays repeatedly as a level's entities are added.
    protected EntityManager(int componentCount, int entityPoolSize, int entityMaxPoolSize, int entityDictionaryCapacity)
    {
        ComponentCount = componentCount;

        _entityPool =
            new ListObjectPool<TEntity>(() => new TEntity(), entityPoolSize, entityMaxPoolSize, entityPoolSize);
        _componentPools = new IObjectPool[ComponentCount];

        _groupsByComponentId = new List<EntityGroup<TEntity>>?[ComponentCount];

        _entities = new SnapshotableDictionary<int, TEntity>(
            new Dictionary<int, TEntity>(entityDictionaryCapacity), e => e.Id);

        Queue = null!;
    }

    public int ComponentCount
    {
        get;
    }

    public IMessageQueue Queue
    {
        get;
        private set;
    }

    public bool IsLoading
    {
        get;
        set;
    }

    public EntityManager<TEntity> Initialize(IMessageQueue queue)
    {
        Debug.Assert(Queue == null);
        Debug.Assert(_entities.Count == 0);

        Queue = queue;
        InitializeSystems(queue);

        Debug.Assert(_componentPools.All(p => p != null));

        return this;
    }

    protected abstract void InitializeSystems(IMessageQueue queue);

    protected internal virtual void OnMessageQueueDrained()
    {
    }

    public void Enqueue<TMessage>(TMessage message, bool lowPriority = false)
        where TMessage : class, IMessage, new()
        => Queue.Enqueue(message, lowPriority);

    protected virtual void Add<TComponent>(int componentId, int poolSize)
        where TComponent : Component, new()
        => Add<TComponent>(componentId, poolSize, poolSize);

    // poolSize is the eager-preallocation / initial-fill count; maxPoolSize caps how many returned
    // instances the pool retains for reuse. They differ for component types with a large per-level
    // working set (effects, abilities): a small poolSize avoids a big up-front allocation while a
    // large maxPoolSize lets the per-level create-and-free churn be recycled instead of GC'd.
    protected virtual void Add<TComponent>(int componentId, int poolSize, int maxPoolSize)
        where TComponent : Component, new()
        => _componentPools[componentId] =
            new ListObjectPool<TComponent>(() => new TComponent(), poolSize, maxPoolSize, poolSize);

    public virtual object? Rent(Type type)
        => type == typeof(TEntity) ? _entityPool.Rent() : null;

    protected EntityGroup<TEntity> CreateGroup(string name, EntityMatcher<TEntity> matcher, int capacity = 8)
    {
        var group = new EntityGroup<TEntity>(name, matcher, capacity);

        foreach (var id in matcher.GetAllIds())
        {
            (_groupsByComponentId[id] ?? (_groupsByComponentId[id] = new List<EntityGroup<TEntity>>()))
                .Add(group);
        }

        return group;
    }

    public abstract int GetNextEntityId();

    public abstract string GetComponentPropertyName(int componentId);

    ITransientReference<Entity> IEntityManager.CreateEntity() => CreateEntity();

    public TransientReference<TEntity, EntityManager<TEntity>> CreateEntity()
        => new(CreateEntityNoReference(), this);

    protected virtual TEntity CreateEntityNoReference()
    {
        var entity = _entityPool.Rent();
        entity.Initialize(GetNextEntityId(), ComponentCount, this);
        _entities[entity.Id] = entity;
        return entity;
    }

    /// <summary>
    ///     <paramref name="entity" /> should be empty
    /// </summary>
    /// <param name="entity"></param>
    public void AddEntity(TEntity entity)
    {
        _entities[entity.Id] = entity;
        entity.Initialize(entity.Id, ComponentCount, this);
    }

    /// <summary>
    ///     Should only be called from Entity
    /// </summary>
    /// <param name="entity"></param>
    void IEntityManager.RemoveEntity(Entity entity)
    {
        Debug.Assert(_entities.Dictionary.ContainsKey(entity.Id));
        Debug.Assert(!entity.HasAnyComponent(Enumerable.Range(0, ComponentCount).ToArray()));

        _entities.Dictionary.Remove(entity.Id);
    }

    public IEnumerable<TEntity> GetEntities() => _entities;

    public TEntity? FindEntity(int id) => _entities.Dictionary.TryGetValue(id, out var entity) ? entity : null;
    public virtual Entity? LoadEntity(int id) => null;

    Entity? IEntityManager.FindEntity(int id) => FindEntity(id);

    public TComponent CreateComponent<TComponent>(int componentId)
        where TComponent : Component, new() => ((ListObjectPool<TComponent>)_componentPools[componentId]!).Rent();

    public void OnComponentAdded(Component component)
        => OnComponentAddedOrRemoved(component, removed: false);

    public void OnComponentRemoved(Component component)
        => OnComponentAddedOrRemoved(component, removed: true);

    private void OnComponentAddedOrRemoved(Component component, bool removed)
    {
        var entity = (TEntity)component.Entity!;
        var groups = _groupsByComponentId[component.ComponentId];
        if (groups != null)
        {
            foreach (var group in groups)
            {
                group.HandleEntityComponentChanged(entity, component, removed);
            }
        }
    }

    public void OnPropertyValueChanged<T>(
        string propertyName, T oldValue, T newValue, int componentId, Component component)
    {
        var groups = _groupsByComponentId[componentId];
        if (groups != null)
        {
            var changes = IPropertyValueChanges.Create(
                new PropertyValueChange<T>(component, propertyName, oldValue, newValue));
            var entityChange = new EntityChange<TEntity>((TEntity)component.Entity!, changes);
            foreach (var group in groups)
            {
                group.HandlePropertyValuesChanged(entityChange);
            }

            // The creator owns this instance and returns it now that the synchronous group loop is done.
            changes.Return();
        }
    }

    public void OnCollectionChanged(
        Component component,
        string propertyName,
        in EntityCollectionChange oldValue,
        in EntityCollectionChange newValue)
    {
        var groups = _groupsByComponentId[component.ComponentId];
        if (groups == null)
        {
            return;
        }

        var entity = (TEntity)component.Entity;
        var hasSubscribers = false;
        foreach (var group in groups)
        {
            if (group.Matcher.Matches(entity)
                && group.HasPropertyValueSubscribersFor(propertyName, null))
            {
                hasSubscribers = true;
                break;
            }
        }

        if (!hasSubscribers)
        {
            return;
        }

        var changes = IPropertyValueChanges.Create(
            new PropertyValueChange<EntityCollectionChange>(component, propertyName, oldValue, newValue));
        var entityChange = new EntityChange<TEntity>(entity, changes);
        foreach (var group in groups)
        {
            group.HandlePropertyValuesChanged(entityChange);
        }

        changes.Return();
    }

    public void OnNestedPropertyValueChanged<T>(
        Component component, string propertyName, string nestedPropertyName,
        object nestedItem, T oldValue, T newValue)
    {
        var groups = _groupsByComponentId[component.ComponentId];
        if (groups == null)
        {
            return;
        }

        var entity = (TEntity)component.Entity;
        var hasSubscribers = false;
        foreach (var group in groups)
        {
            if (group.Matcher.Matches(entity)
                && group.HasPropertyValueSubscribersFor(propertyName, nestedPropertyName))
            {
                hasSubscribers = true;
                break;
            }
        }

        if (!hasSubscribers)
        {
            return;
        }

        var changes = IPropertyValueChanges.Create(
            new PropertyValueChange<T>(
                component, propertyName, oldValue, newValue, nestedItem, nestedPropertyName));
        var entityChange = new EntityChange<TEntity>(entity, changes);
        foreach (var group in groups)
        {
            group.HandlePropertyValuesChanged(entityChange);
        }

        changes.Return();
    }

    public void OnPropertyValuesChanged(Entity entity, IPropertyValueChanges changes)
    {
        var entityChange = new EntityChange<TEntity>((TEntity)entity, changes);
        var propertyChanges = entityChange.PropertyChanges;
        HashSet<string>? handledGroups = null;
        for (var i = 0; i < propertyChanges.Count; i++)
        {
            var groups = _groupsByComponentId[propertyChanges.GetChangedComponent(i).ComponentId];
            if (groups == null)
            {
                continue;
            }

            if (handledGroups == null
                && i < propertyChanges.Count - 1)
            {
                handledGroups = new HashSet<string>();
            }

            foreach (var group in groups)
            {
                if (handledGroups != null)
                {
                    if (!handledGroups.Add(group.Name))
                    {
                        continue;
                    }
                }

                group.HandlePropertyValuesChanged(entityChange);
            }
        }
    }

    public virtual void RemoveFromSecondaryTracker(ITrackable trackable)
    {
    }
}
