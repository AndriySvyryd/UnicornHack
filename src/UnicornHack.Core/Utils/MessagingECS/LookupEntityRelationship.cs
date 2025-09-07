using System.Linq.Expressions;

namespace UnicornHack.Utils.MessagingECS;

public class LookupEntityRelationship<TEntity, TKey, TDictionary> : EntityRelationshipBase<TEntity>
    where TEntity : Entity, new()
    where TKey : notnull
    where TDictionary : class, IDictionary<TKey, TEntity>, new()
{
    private DependentsGroup? _dependents;
    private readonly IKeyValueGetter<TEntity, TKey> _secondaryKeyValueGetter;
    private readonly IKeyValueGetter<TEntity, int>? _secondaryPrincipalKeyValueGetter;
    private readonly IEntityGroup<TEntity>? _secondaryPrincipalGroup;
    private readonly ICollectionAccessor<TEntity, TDictionary, KeyValuePair<TKey, TEntity>> _accessor;
    private readonly Dictionary<int, HashSet<TEntity>> _orphanedEntities = [];
    private readonly Dictionary<int, HashSet<TEntity>> _secondaryOrphanedEntities = [];
    private readonly Dictionary<int, HashSet<TEntity>>? _secondaryPrincipalToDependentsIndex;

    private static readonly IReadOnlyDictionary<TKey, TEntity> EmptyDictionary = new Dictionary<TKey, TEntity>();

    public IEntityGroup<TEntity> Dependents
        => _dependents ??= new DependentsGroup(this);

    public LookupEntityRelationship(
        string name,
        IEntityGroup<TEntity> dependentGroup,
        IEntityGroup<TEntity> principalGroup,
        IKeyValueGetter<TEntity, int> keyValueGetter,
        IKeyValueGetter<TEntity, TKey> secondaryKeyValueGetter,
        Action<TEntity, EntityChange<TEntity>> handlePrincipalDeleted,
        Expression<Func<TEntity, TDictionary?>>? getDependent = null,
        Expression<Func<TEntity, TEntity?>>? getPrincipal = null,
        IEntityGroup<TEntity>? secondaryPrincipalGroup = null,
        IKeyValueGetter<TEntity, int>? secondaryPrincipalKeyValueGetter = null,
        Func<TDictionary>? factory = null,
        bool keepPrincipalAlive = false,
        bool keepDependentAlive = false)
        : this(
            name,
            dependentGroup,
            principalGroup,
            keyValueGetter,
            secondaryKeyValueGetter,
            handlePrincipalDeleted,
            secondaryPrincipalGroup,
            secondaryPrincipalKeyValueGetter,
            keepPrincipalAlive,
            keepDependentAlive)
    {
        _accessor = getDependent == null
            ? new ExternalCollectionAccessor<TEntity, TDictionary, KeyValuePair<TKey, TEntity>>()
            : new PropertyCollectionAccessor<TEntity, TDictionary, KeyValuePair<TKey, TEntity>>(
                getDependent, getPrincipal);
        _accessor.SetDefaultFactory(factory ?? (() => new TDictionary()));
    }

    public LookupEntityRelationship(
        string name,
        IEntityGroup<TEntity> dependentGroup,
        IEntityGroup<TEntity> principalGroup,
        IKeyValueGetter<TEntity, int> keyValueGetter,
        IKeyValueGetter<TEntity, TKey> secondaryKeyValueGetter,
        Action<TEntity, EntityChange<TEntity>> handlePrincipalDeleted,
        Func<TEntity, TDictionary?> getDependent,
        Action<TEntity, TDictionary?> setDependent,
        Func<Component, TDictionary?> componentGetDependent,
        Func<TEntity, TEntity?>? getPrincipal = null,
        Action<TEntity, TEntity?>? setPrincipal = null,
        Func<Component, TEntity?>? componentGetPrincipal = null,
        IEntityGroup<TEntity>? secondaryPrincipalGroup = null,
        IKeyValueGetter<TEntity, int>? secondaryPrincipalKeyValueGetter = null,
        Func<TDictionary>? factory = null,
        bool keepPrincipalAlive = false,
        bool keepDependentAlive = false)
        : this(
            name,
            dependentGroup,
            principalGroup,
            keyValueGetter,
            secondaryKeyValueGetter,
            handlePrincipalDeleted,
            secondaryPrincipalGroup,
            secondaryPrincipalKeyValueGetter,
            keepPrincipalAlive,
            keepDependentAlive)
    {
        _accessor = new PropertyCollectionAccessor<TEntity, TDictionary, KeyValuePair<TKey, TEntity>>(
            getDependent, setDependent, componentGetDependent, getPrincipal, setPrincipal, componentGetPrincipal);
        _accessor.SetDefaultFactory(factory ?? (() => new TDictionary()));
    }

    private LookupEntityRelationship(
        string name,
        IEntityGroup<TEntity> dependentGroup,
        IEntityGroup<TEntity> principalGroup,
        IKeyValueGetter<TEntity, int> keyValueGetter,
        IKeyValueGetter<TEntity, TKey> secondaryKeyValueGetter,
        Action<TEntity, EntityChange<TEntity>> handlePrincipalDeleted,
        IEntityGroup<TEntity>? secondaryPrincipalGroup = null,
        IKeyValueGetter<TEntity, int>? secondaryPrincipalKeyValueGetter = null,
        bool keepPrincipalAlive = false,
        bool keepDependentAlive = false)
        : base(
            name,
            dependentGroup,
            principalGroup,
            keyValueGetter,
            handlePrincipalDeleted,
            keepPrincipalAlive,
            keepDependentAlive)
    {
        if (secondaryPrincipalKeyValueGetter != null && secondaryPrincipalGroup == null)
        {
            throw new ArgumentException(
                "Secondary principal key value getter provided without a corresponding secondary principal group",
                nameof(secondaryPrincipalKeyValueGetter));
        }

        if (secondaryPrincipalGroup != null && secondaryPrincipalKeyValueGetter == null)
        {
            throw new ArgumentException(
                "Secondary principal group provided without a corresponding secondary principal key value getter",
                nameof(secondaryPrincipalGroup));
        }

        principalGroup.AddListener(new PrincipalGroupListener(this));
        _secondaryKeyValueGetter = secondaryKeyValueGetter;
        _secondaryPrincipalGroup = secondaryPrincipalGroup;
        secondaryPrincipalGroup?.AddListener(new SecondaryPrincipalGroupListener(this));
        _secondaryPrincipalKeyValueGetter = secondaryPrincipalKeyValueGetter;

        if (secondaryPrincipalKeyValueGetter != null)
        {
            _secondaryPrincipalToDependentsIndex = [];
        }

        // TODO: Consider using a nested relationship instead of duplicating the logic for secondary principals and dependents
        // Akin to skip navigations

        _accessor = null!;
    }

    public IReadOnlyDictionary<TKey, TEntity> GetDependents(TEntity principal, Component? removedComponent = null)
        => (IReadOnlyDictionary<TKey, TEntity>?)_accessor.GetDependents(principal, removedComponent)
           ?? EmptyDictionary;

    public TEntity? GetPrincipal(TEntity dependent, Component? removedComponent = null)
        => _accessor.TryGetPrincipal(dependent, out var principal, removedComponent)
            ? principal
            : KeyValueGetter.TryGetKey(new EntityChange<TEntity>(dependent), ValueType.Current, out var key)
                ? FindPrincipal(key, dependent, fallback: false)
                : null;

    protected override bool ContainsEntity(TEntity entity)
    {
        var entityChange = new EntityChange<TEntity>(entity);
        return KeyValueGetter.TryGetKey(entityChange, ValueType.Current, out var key)
            && FindPrincipal(key, entityChange.Entity, fallback: false) is { }
            && _secondaryKeyValueGetter.TryGetKey(entityChange, ValueType.Current, out _)
            && (_secondaryPrincipalGroup == null
                || (_secondaryPrincipalKeyValueGetter!.TryGetKey(entityChange, ValueType.Current, out var secondaryPrincipalKey)
                    && _secondaryPrincipalGroup.ContainsEntity(secondaryPrincipalKey)));
    }

    protected override bool TryAddEntity(int key, in EntityChange<TEntity> entityChange)
        => TryAddEntity(key, entityChange, skipSecondaryPrincipalCheck: false);

    protected bool TryAddEntity(int key, in EntityChange<TEntity> entityChange, bool skipSecondaryPrincipalCheck)
    {
        if (!_secondaryKeyValueGetter.TryGetKey(entityChange, ValueType.Current, out var secondaryKey))
        {
            return false;
        }

        if (!skipSecondaryPrincipalCheck
            && _secondaryPrincipalGroup != null)
        {
            if (!_secondaryPrincipalKeyValueGetter!.TryGetKey(entityChange, ValueType.Current, out var secondaryPrincipalKey))
            {
                return false;
            }
            else if (!_secondaryPrincipalGroup.ContainsEntity(secondaryPrincipalKey))
            {
                if (!_secondaryOrphanedEntities.TryGetValue(secondaryPrincipalKey, out var secondaryOrphanedEntities))
                {
                    secondaryOrphanedEntities = new HashSet<TEntity>(EntityEqualityComparer<TEntity>.Instance);
                    _secondaryOrphanedEntities.Add(secondaryPrincipalKey, secondaryOrphanedEntities);
                }

                secondaryOrphanedEntities.Add(entityChange.Entity);
                return false;
            }
        }

        var principal = FindPrincipal(key, entityChange.Entity, fallback: false);
        if (principal != null)
        {
            _accessor.GetOrCreateDependents(principal).Add(secondaryKey, entityChange.Entity);
            OnEntityAdded(entityChange, principal);
            return true;
        }

        if (!_orphanedEntities.TryGetValue(key, out var orphanedEntities))
        {
            orphanedEntities = new HashSet<TEntity>(EntityEqualityComparer<TEntity>.Instance);
            _orphanedEntities.Add(key, orphanedEntities);
        }

        orphanedEntities.Add(entityChange.Entity);

        return false;
    }

    private void OnEntityAdded(in EntityChange<TEntity> entityChange, TEntity principal)
    {
        _accessor.SetPrincipal(entityChange.Entity, principal);

        _dependents?.Add(entityChange, principal);

        ((IEntityRelationshipBase<TEntity>)this).OnEntityAdded(entityChange.Entity, principal);

        if (_secondaryPrincipalKeyValueGetter != null &&
            _secondaryPrincipalKeyValueGetter.TryGetKey(entityChange, ValueType.Current, out var secondaryPrincipalKey))
        {
            if (!_secondaryPrincipalToDependentsIndex!.TryGetValue(secondaryPrincipalKey, out var dependentSet))
            {
                dependentSet = new HashSet<TEntity>(EntityEqualityComparer<TEntity>.Instance);
                _secondaryPrincipalToDependentsIndex.Add(secondaryPrincipalKey, dependentSet);
            }
            dependentSet.Add(entityChange.Entity);
        }
    }

    protected override bool TryRemoveEntity(int key, in EntityChange<TEntity> entityChange)
    {
        if (!_secondaryKeyValueGetter.TryGetKey(entityChange, ValueType.PreferOld, out var secondaryKey))
        {
            return false;
        }

        var principal = FindPrincipal(key, entityChange.Entity, fallback: true);
        if (principal == null)
        {
            return false;
        }

        int? secondaryPrincipalKey = null;
        if (_secondaryPrincipalKeyValueGetter != null)
        {
            if (_secondaryPrincipalKeyValueGetter.TryGetKey(entityChange, ValueType.PreferOld, out var secondaryPK)
                && _secondaryPrincipalGroup!.ContainsEntity(secondaryPK))
            {
                secondaryPrincipalKey = secondaryPK;
            }
            else
            {
                return false;
            }
        }

        var dependentEntities = _accessor.GetDependents(principal);
        if (dependentEntities != null
            && dependentEntities.Remove(secondaryKey))
        {
            if (secondaryPrincipalKey != null
                && _secondaryPrincipalToDependentsIndex!.TryGetValue(secondaryPrincipalKey.Value, out var dependentSet))
            {
                dependentSet.Remove(entityChange.Entity);
                if (dependentSet.Count == 0)
                {
                    _secondaryPrincipalToDependentsIndex.Remove(secondaryPrincipalKey.Value);
                }
            }

            OnEntityRemoved(entityChange, principal);
            return true;
        }

        Debug.Assert(_orphanedEntities.Count == 0, $"Orphaned entities found in {Name} for principal {key}");

        return false;
    }

    private void OnEntityRemoved(in EntityChange<TEntity> entityChange, TEntity principal)
    {
        _accessor.SetPrincipal(entityChange.Entity, null);

        _dependents?.Remove(entityChange, principal);

        ((IEntityRelationshipBase<TEntity>)this).OnEntityRemoved(entityChange.Entity, principal);
    }

    protected override bool HandleNonKeyPropertyValuesChanged(in EntityChange<TEntity> entityChange)
    {
        Debug.Assert(entityChange.RemovedComponent == null);

        if (!KeyValueGetter.TryGetKey(entityChange, ValueType.Current, out var key)
            || FindPrincipal(key, entityChange.Entity, fallback: true) is not { } principal)
        {
            return true;
        }

        var secondaryKeyPropertyChanged = false;
        var secondaryPrincipalKeyPropertyChanged = false;
        var changes = entityChange.PropertyChanges;
        for (var i = 0; i < changes.Count; i++)
        {
            var changedComponent = changes.GetChangedComponent(i);
            if (_secondaryKeyValueGetter.PropertyUsed(changedComponent.ComponentId,
                    changes.GetChangedPropertyName(i)))
            {
                // The component might have been removed by the previous change listener
                if (entityChange.Entity.FindComponent(changedComponent.ComponentId) != changedComponent)
                {
                    return true;
                }

                secondaryKeyPropertyChanged = true;
                break;
            }

            if (_secondaryPrincipalKeyValueGetter != null
                && _secondaryPrincipalKeyValueGetter.PropertyUsed(changedComponent.ComponentId, changes.GetChangedPropertyName(i)))
            {
                if (entityChange.Entity.FindComponent(changedComponent.ComponentId) != changedComponent)
                {
                    return true;
                }

                secondaryPrincipalKeyPropertyChanged = true;
            }
        }

        var entities = _accessor.GetOrCreateDependents(principal);
        var entity = entityChange.Entity;
        if (secondaryPrincipalKeyPropertyChanged)
        {
            if (_secondaryPrincipalKeyValueGetter!.TryGetKey(entityChange, ValueType.PreferOld, out var oldSecondaryPrincipalKey)
                && _secondaryPrincipalToDependentsIndex!.TryGetValue(oldSecondaryPrincipalKey, out var oldDependentSet))
            {
                oldDependentSet.Remove(entity);
                if (oldDependentSet.Count == 0)
                {
                    _secondaryPrincipalToDependentsIndex.Remove(oldSecondaryPrincipalKey);
                }
            }

            if (_secondaryPrincipalKeyValueGetter.TryGetKey(entityChange, ValueType.Current, out var newSecondaryPrimaryKey))
            {
                if (_secondaryPrincipalGroup!.ContainsEntity(newSecondaryPrimaryKey))
                {
                    if (_secondaryKeyValueGetter.TryGetKey(entityChange, ValueType.Current, out var newSortKey)
                        && !entities.ContainsKey(newSortKey))
                    {
                        entities.Add(newSortKey, entity);
                        OnEntityAdded(entityChange, principal);
                    }
                }
                else
                {
                    if (_secondaryKeyValueGetter.TryGetKey(entityChange, ValueType.PreferOld, out var oldSortKey)
                        && entities.Remove(oldSortKey))
                    {
                        OnEntityRemoved(entityChange, principal);
                    }
                }
            }
        }
        else
        {
            int? secondaryPrincipalKey = null;
            if (_secondaryPrincipalKeyValueGetter != null)
            {
                if (_secondaryPrincipalKeyValueGetter.TryGetKey(entityChange, ValueType.Current, out var secondaryPK)
                    && _secondaryPrincipalGroup!.ContainsEntity(secondaryPK))
                {
                    secondaryPrincipalKey = secondaryPK;
                }
                else
                {
                    return true;
                }
            }

            if (secondaryKeyPropertyChanged)
            {
                if (_secondaryKeyValueGetter.TryGetKey(entityChange, ValueType.PreferOld, out var oldSortKey)
                    && entities.Remove(oldSortKey))
                {
                    if (secondaryPrincipalKey != null
                        && _secondaryPrincipalToDependentsIndex!.TryGetValue(secondaryPrincipalKey.Value, out var dependentSet))
                    {
                        dependentSet.Remove(entityChange.Entity);
                        if (dependentSet.Count == 0)
                        {
                            _secondaryPrincipalToDependentsIndex.Remove(secondaryPrincipalKey.Value);
                        }
                    }

                    OnEntityRemoved(entityChange, principal);
                }

                if (_secondaryKeyValueGetter.TryGetKey(entityChange, ValueType.Current, out var newSortKey))
                {
                    entities.Add(newSortKey, entity);
                    OnEntityAdded(entityChange, principal);
                }
            }
            else
            {
                _dependents?.PropertyValuesChanged(entityChange);
            }
        }

        return true;
    }

    public override string ToString() => "LookupRelationship: " + Name;

    private class DependentsGroup : EntityGroupBase<TEntity>
    {
        private readonly LookupEntityRelationship<TEntity, TKey, TDictionary> _relationship;

        public DependentsGroup(LookupEntityRelationship<TEntity, TKey, TDictionary> relationship)
            : base(relationship.Name)
        {
            _relationship = relationship;
        }

        public override TEntity? FindEntity(int id)
            => ((IEntityRelationshipBase<TEntity>)_relationship).FindEntity(id);

        public override bool ContainsEntity(int id)
            => FindEntity(id) != null;

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

        public override string ToString() => "LookupDependentGroup: " + Name;
    }

    private class PrincipalGroupListener : IEntityChangeListener<TEntity>
    {
        private readonly LookupEntityRelationship<TEntity, TKey, TDictionary> _relationship;

        public PrincipalGroupListener(LookupEntityRelationship<TEntity, TKey, TDictionary> relationship)
        {
            _relationship = relationship;
        }

        public void OnEntityAdded(in EntityChange<TEntity> entityChange)
        {
            var principal = entityChange.Entity;
            var key = principal.Id;
            if (!_relationship._orphanedEntities.TryGetValue(key, out var orphanedEntities)
                || orphanedEntities.Count == 0)
            {
                return;
            }

            var dependents = _relationship._accessor.GetOrCreateDependents(principal);

            foreach (var orphanedEntity in orphanedEntities)
            {
                Debug.Assert(orphanedEntity.Manager != null);

                var orphanedEntityChange = new EntityChange<TEntity>(orphanedEntity);
                if (_relationship._secondaryKeyValueGetter.TryGetKey(
                        orphanedEntityChange, ValueType.Current, out var secondaryKey))
                {
                    dependents.Add(secondaryKey, orphanedEntity);
                    _relationship.OnEntityAdded(orphanedEntityChange, principal);
                }
                else if (_relationship._secondaryPrincipalGroup != null
                         && _relationship._secondaryPrincipalKeyValueGetter!.TryGetKey(
                             orphanedEntityChange, ValueType.Current, out var secondaryPrincipalKey)
                        && !_relationship._secondaryPrincipalGroup.ContainsEntity(secondaryPrincipalKey))
                {
                    // This can happen if the secondary principal existed when the dependent was added,
                    // but was removed before the principal was added
                    if (!_relationship._secondaryOrphanedEntities.TryGetValue(secondaryPrincipalKey, out var secondaryOrphanedEntities))
                    {
                        secondaryOrphanedEntities = new HashSet<TEntity>(EntityEqualityComparer<TEntity>.Instance);
                        _relationship._secondaryOrphanedEntities.Add(secondaryPrincipalKey, secondaryOrphanedEntities);
                    }
                    secondaryOrphanedEntities.Add(orphanedEntity);
                }
            }

            _relationship._orphanedEntities.Remove(key);
        }

        public void OnEntityRemoved(in EntityChange<TEntity> entityChange)
        {
            var principal = entityChange.Entity;
            var dependents = _relationship._accessor.GetDependents(principal, entityChange.RemovedComponent);
            if (dependents == null
                || dependents.Count == 0)
            {
                return;
            }

            Debug.Assert(_relationship._orphanedEntities.Count == 0,
                $"Orphaned entities found in {_relationship.Name}");

            foreach (var dependent in dependents.Values)
            {
                var dependentChange = new EntityChange<TEntity>(dependent);

                _relationship.OnEntityRemoved(dependentChange, principal);
                _relationship.HandlePrincipalEntityRemoved(dependent, entityChange);
            }

            _relationship._accessor.ResetDependents(principal);
        }

        public bool OnPropertyValuesChanged(in EntityChange<TEntity> entityChange)
            => false;

        public override string ToString() => "Principal " + _relationship;
    }

    private class SecondaryPrincipalGroupListener : IEntityChangeListener<TEntity>
    {
        private readonly LookupEntityRelationship<TEntity, TKey, TDictionary> _relationship;

        public SecondaryPrincipalGroupListener(LookupEntityRelationship<TEntity, TKey, TDictionary> relationship)
        {
            _relationship = relationship;
        }

        public void OnEntityAdded(in EntityChange<TEntity> entityChange)
        {
            var secondaryKey = entityChange.Entity.Id;
            if (!_relationship._secondaryOrphanedEntities.TryGetValue(secondaryKey, out var secondaryOrphanedEntities))
            {
                return;
            }

            foreach (var orphanedEntity in secondaryOrphanedEntities)
            {
                Debug.Assert(orphanedEntity.Manager != null);

                var orphanedEntityChange = new EntityChange<TEntity>(orphanedEntity);
                if (_relationship.KeyValueGetter.TryGetKey(orphanedEntityChange, ValueType.Current, out var primaryKey))
                {
                    _relationship.TryAddEntity(primaryKey, orphanedEntityChange, skipSecondaryPrincipalCheck: true);
                }
            }

            _relationship._secondaryOrphanedEntities.Remove(secondaryKey);
        }

        public void OnEntityRemoved(in EntityChange<TEntity> entityChange)
        {
            var secondaryPrincipalKey = entityChange.Entity.Id;

            var orphansRemoved = _relationship._secondaryOrphanedEntities.Remove(secondaryPrincipalKey);
            Debug.Assert(!orphansRemoved, $"Orphaned entities found in {_relationship.Name} for secondary principal {secondaryPrincipalKey}");

            if (!_relationship._secondaryPrincipalToDependentsIndex!.TryGetValue(secondaryPrincipalKey, out var dependentsToRemove))
            {
                return;
            }

            if (dependentsToRemove.Count == 0)
            {
                _relationship._secondaryPrincipalToDependentsIndex.Remove(secondaryPrincipalKey);
                return;
            }

            foreach (var dependent in dependentsToRemove.ToArray())
            {
                var dependentChange = new EntityChange<TEntity>(dependent);
                _relationship.KeyValueGetter.TryGetKey(dependentChange, ValueType.Current, out var primaryKey);
                _relationship.TryRemoveEntity(primaryKey, dependentChange);
                _relationship.HandlePrincipalEntityRemoved(dependent, entityChange);
            }
        }

        public bool OnPropertyValuesChanged(in EntityChange<TEntity> entityChange)
            => false;

        public override string ToString() => "SecondaryPrincipal " + _relationship;
    }
}
