using System.Linq.Expressions;

namespace UnicornHack.Utils.MessagingECS;

public class CollectionEntityRelationship<TEntity, TCollection> : EntityRelationshipBase<TEntity>
    where TEntity : Entity, new()
    where TCollection : class, ICollection<TEntity>, new()
{
    private DependentsGroup? _dependents;

    public CollectionEntityRelationship(
        string name,
        IEntityGroup<TEntity> dependentGroup,
        IEntityGroup<TEntity> principalGroup,
        IKeyValueGetter<TEntity, int> keyValueGetter,
        Action<TEntity, EntityChange<TEntity>> handlePrincipalDeleted,
        Expression<Func<TEntity, TCollection?>>? getDependent = null,
        Expression<Func<TEntity, TEntity?>>? getPrincipal = null,
        Func<TCollection>? factory = null,
        bool keepPrincipalAlive = false,
        bool keepDependentAlive = false)
        : this(
            name,
            dependentGroup,
            principalGroup,
            keyValueGetter,
            handlePrincipalDeleted,
            keepPrincipalAlive,
            keepDependentAlive)
    {
        Accessor = getDependent == null
            ? new ExternalCollectionAccessor<TEntity, TCollection, TEntity>()
            : new PropertyCollectionAccessor<TEntity, TCollection, TEntity>(getDependent, getPrincipal);
        Accessor.SetDefaultFactory(
            factory
            ?? (typeof(TCollection) == typeof(HashSet<TEntity>)
                ? () => (TCollection)(object)new HashSet<TEntity>(EntityEqualityComparer<TEntity>.Instance)
                : () => new TCollection()));
    }

    public CollectionEntityRelationship(
        string name,
        IEntityGroup<TEntity> dependentGroup,
        IEntityGroup<TEntity> principalGroup,
        IKeyValueGetter<TEntity, int> keyValueGetter,
        Action<TEntity, EntityChange<TEntity>> handlePrincipalDeleted,
        Func<TEntity, TCollection?> getDependent,
        Action<TEntity, TCollection?> setDependent,
        Func<Component, TCollection?> componentGetDependent,
        Func<TEntity, TEntity?>? getPrincipal = null,
        Action<TEntity, TEntity?>? setPrincipal = null,
        Func<Component, TEntity?>? componentGetPrincipal = null,
        Func<TCollection>? factory = null,
        bool keepPrincipalAlive = false,
        bool keepDependentAlive = false)
        : this(
            name,
            dependentGroup,
            principalGroup,
            keyValueGetter,
            handlePrincipalDeleted,
            keepPrincipalAlive,
            keepDependentAlive)
    {
        Accessor = new PropertyCollectionAccessor<TEntity, TCollection, TEntity>(
            getDependent, setDependent, componentGetDependent, getPrincipal, setPrincipal, componentGetPrincipal);
        Accessor.SetDefaultFactory(
            factory
            ?? (typeof(TCollection) == typeof(HashSet<TEntity>)
                ? () => (TCollection)(object)new HashSet<TEntity>(EntityEqualityComparer<TEntity>.Instance)
                : () => new TCollection()));
    }

    private CollectionEntityRelationship(
        string name,
        IEntityGroup<TEntity> dependentGroup,
        IEntityGroup<TEntity> principalGroup,
        IKeyValueGetter<TEntity, int> keyValueGetter,
        Action<TEntity, EntityChange<TEntity>> handlePrincipalDeleted,
        bool keepPrincipalAlive,
        bool keepDependentAlive)
        : base(
            name,
            dependentGroup,
            principalGroup,
            keyValueGetter,
            handlePrincipalDeleted,
            keepPrincipalAlive,
            keepDependentAlive)
    {
        principalGroup.AddListener(new PrincipalGroupListener(this));
        Accessor = null!;
    }

    protected ICollectionAccessor<TEntity, TCollection, TEntity> Accessor
    {
        get;
    }

    protected Dictionary<int, HashSet<TEntity>> OrphanedEntities
    {
        get;
    } = new();

    public IEntityGroup<TEntity> Dependents
        => _dependents ??= new DependentsGroup(this);

    public IReadOnlyCollection<TEntity> GetDependents(TEntity principal, Component? removedComponent = null)
        => (IReadOnlyCollection<TEntity>?)Accessor.GetDependents(principal, removedComponent) ??
           Array.Empty<TEntity>();

    public TEntity? GetPrincipal(TEntity dependent, Component? removedComponent = null)
        => Accessor.TryGetPrincipal(dependent, out var principal, removedComponent)
            ? principal
            : KeyValueGetter.TryGetKey(new EntityChange<TEntity>(dependent), ValueType.Current, out var key)
                ? FindPrincipal(key, dependent, fallback: false)
                : null;

    protected override bool TryAddEntity(int key, in EntityChange<TEntity> entityChange)
    {
        var principal = FindPrincipal(key, entityChange.Entity, fallback: false);
        if (principal != null)
        {
            Accessor.GetOrCreateDependents(principal).Add(entityChange.Entity);
            OnEntityAdded(entityChange, principal);
            return true;
        }

        if (!OrphanedEntities.TryGetValue(key, out var orphanedEntities))
        {
            orphanedEntities = new HashSet<TEntity>(EntityEqualityComparer<TEntity>.Instance);
            OrphanedEntities.Add(key, orphanedEntities);
        }

        orphanedEntities.Add(entityChange.Entity);

        return false;
    }

    private void OnEntityAdded(in EntityChange<TEntity> entityChange, TEntity principal)
    {
        Accessor.SetPrincipal(entityChange.Entity, principal);

        _dependents?.Add(entityChange, principal);

        ((IEntityRelationshipBase<TEntity>)this).OnEntityAdded(entityChange.Entity, principal);
    }

    protected override bool TryRemoveEntity(int key, in EntityChange<TEntity> entityChange)
    {
        var principal = FindPrincipal(key, entityChange.Entity, fallback: true);
        if (principal == null)
        {
            return false;
        }

        var dependentEntities = Accessor.GetDependents(principal);
        if (dependentEntities != null
            && dependentEntities.Remove(entityChange.Entity))
        {
            OnEntityRemoved(entityChange, principal);
            return true;
        }

        Debug.Assert(OrphanedEntities.Count == 0, $"Orphaned entities found in {Name}");

        return false;
    }

    private void OnEntityRemoved(in EntityChange<TEntity> entityChange, TEntity principal)
    {
        Accessor.SetPrincipal(entityChange.Entity, null);

        _dependents?.Remove(entityChange, principal);

        ((IEntityRelationshipBase<TEntity>)this).OnEntityRemoved(entityChange.Entity, principal);
    }

    protected override bool HandleNonKeyPropertyValuesChanged(in EntityChange<TEntity> entityChange)
    {
        _dependents?.PropertyValuesChanged(entityChange);

        return true;
    }

    public override string ToString() => "CollectionEntityRelationship: " + Name;

    private class DependentsGroup : EntityGroupBase<TEntity>
    {
        private readonly IEntityRelationshipBase<TEntity> _relationship;

        public DependentsGroup(IEntityRelationshipBase<TEntity> relationship)
            : base(relationship.Name)
        {
            _relationship = relationship;
        }

        public override TEntity? FindEntity(int id) => _relationship.FindEntity(id);

        public override bool ContainsEntity(int id) => _relationship.ContainsEntity(id);

        public void Add(in EntityChange<TEntity> entityChange, TEntity principal)
            => OnAdded(entityChange, principal);

        public void Remove(in EntityChange<TEntity> entityChange, TEntity principal)
            => OnRemoved(entityChange, principal);

        public void PropertyValuesChanged(in EntityChange<TEntity> entityChange)
        {
            if (FindEntity(entityChange.Entity.Id) == null)
            {
                return;
            }

            OnPropertyValuesChanged(entityChange);
        }

        public override string ToString() => "CollectionDependentGroup: " + Name;
    }

    private class PrincipalGroupListener : IEntityChangeListener<TEntity>
    {
        private readonly CollectionEntityRelationship<TEntity, TCollection> _relationship;

        public PrincipalGroupListener(CollectionEntityRelationship<TEntity, TCollection> relationship)
        {
            _relationship = relationship;
        }

        public void OnEntityAdded(in EntityChange<TEntity> entityChange)
        {
            var principal = entityChange.Entity;
            var key = entityChange.Entity.Id;
            if (!_relationship.OrphanedEntities.TryGetValue(key, out var orphanedEntities)
                || orphanedEntities.Count == 0)
            {
                return;
            }

            var dependents = _relationship.Accessor.GetOrCreateDependents(principal);
            foreach (var orphanedEntity in orphanedEntities)
            {
                Debug.Assert(orphanedEntity.Manager != null);

                dependents.Add(orphanedEntity);
                _relationship.OnEntityAdded(new EntityChange<TEntity>(orphanedEntity), principal);
            }

            _relationship.OrphanedEntities.Remove(key);
        }

        public void OnEntityRemoved(in EntityChange<TEntity> entityChange)
        {
            var principal = entityChange.Entity;
            var dependents = _relationship.Accessor.GetDependents(principal, entityChange.RemovedComponent);
            if (dependents == null
                || dependents.Count == 0)
            {
                return;
            }

            Debug.Assert(_relationship.OrphanedEntities.Count == 0,
                $"Orphaned entities found in {_relationship.Name}");

            foreach (var dependent in dependents)
            {
                var dependentChange = new EntityChange<TEntity>(dependent);

                _relationship.OnEntityRemoved(dependentChange, principal);
                _relationship.HandlePrincipalEntityRemoved(dependent, entityChange);
            }

            _relationship.Accessor.ResetDependents(principal);
        }

        public bool OnPropertyValuesChanged(in EntityChange<TEntity> entityChange)
            => false;

        public override string ToString() => "Principal " + _relationship;
    }
}
