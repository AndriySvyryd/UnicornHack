namespace UnicornHack.Utils.MessagingECS;

/// <summary>
///     A many-to-many relationship driven by a foreign key carried on each item of a component-owned
///     observable collection. Each dependent entity owns a collection of items and every item may reference
///     a principal entity by id. The relationship keeps a reverse index (principal id -> the dependents that
///     currently carry at least one item referencing it) current from collection and item-property changes
///     routed through the dependent <see cref="EntityGroup{TEntity}" />, and invokes
///     <c>handlePrincipalRemoved</c> for every affected dependent when a principal is removed.
///     Referential integrity: adding an item, or changing its foreign key, to reference a principal that
///     does not resolve to a live entity throws,
///       except while the manager is loading.
///     A foreign key of <c>0</c> is treated as "no reference" (matching the framework's default-key convention).
///     <c>handleLastReferenceRemoved</c> runs when a dependent loses its final item referencing a principal,
///     including collection removal, foreign-key clearing, and dependent removal.
///     When <c>keepPrincipalAlive</c> is set, each dependent holds a keep-alive reference on every principal it
///     currently references, so a principal with no other anchor stays alive for as long as a referencing item
///     persists.
/// </summary>
public class ManyToManyEntityRelationship<TEntity, TItem> : IEntityChangeListener<TEntity>
    where TEntity : Entity, new()
    where TItem : NotificationCollectionItem
{
    private readonly IEntityGroup<TEntity> _principalGroup;
    private readonly Func<TEntity, IEnumerable<TItem>?> _getCollection;
    private readonly IKeyValueGetter<TEntity, int> _foreignKeyValueGetter;
    private readonly Action<TEntity, TEntity> _handlePrincipalRemoved;
    private readonly Action<TEntity, TEntity>? _handleLastDependentRemoved;
    private readonly Func<TEntity, Component?>? _getPrincipalRemovedComponent;

    private readonly Dictionary<int, DependentState> _dependents = new();
    private readonly Dictionary<int, HashSet<TEntity>> _principalToDependents = new();
    private readonly bool _keepPrincipalAlive;
    private ReferencedPrincipalsGroup? _referencedPrincipals;

    // While a principal is being removed, its id - so the per-item keep-alive release in RemoveFromReverse is
    // deferred to OnPrincipalRemoved (a revert-style handler dropping items must not double-release).
    private int? _principalBeingRemoved;

    public ManyToManyEntityRelationship(
        string name,
        IEntityGroup<TEntity> dependentGroup,
        IEntityGroup<TEntity> principalGroup,
        Func<TEntity, IEnumerable<TItem>?> getCollection,
        IKeyValueGetter<TEntity, int> foreignKeyValueGetter,
        Action<TEntity, TEntity> handlePrincipalRemoved,
        Action<TEntity, TEntity>? handleLastDependentRemoved = null,
        Func<TEntity, Component?>? getPrincipalRemovedComponent = null,
        bool keepPrincipalAlive = false)
    {
        Name = name;
        _principalGroup = principalGroup;
        _getCollection = getCollection;
        _foreignKeyValueGetter = foreignKeyValueGetter;
        _handlePrincipalRemoved = handlePrincipalRemoved;
        _handleLastDependentRemoved = handleLastDependentRemoved;
        _getPrincipalRemovedComponent = getPrincipalRemovedComponent;
        _keepPrincipalAlive = keepPrincipalAlive;

        dependentGroup.AddListener(this);
        principalGroup.AddListener(new PrincipalListener(this));
    }

    public string Name
    {
        get;
    }

    /// <summary>The dependents currently carrying at least one item referencing the given principal.</summary>
    public IReadOnlyCollection<TEntity> GetDependents(int principalId)
        => _principalToDependents.TryGetValue(principalId, out var set)
            ? set
            : Array.Empty<TEntity>();

    /// <summary>The principals currently referenced by at least one of the given dependent's items.</summary>
    public IReadOnlyCollection<TEntity> GetPrincipals(int dependentId)
    {
        if (!_dependents.TryGetValue(dependentId, out var state)
            || state.KeyCounts.Count == 0)
        {
            return Array.Empty<TEntity>();
        }

        var principals = new List<TEntity>(state.KeyCounts.Count);
        foreach (var key in state.KeyCounts.Keys)
        {
            if (_principalGroup.FindEntity(key) is { } principal)
            {
                principals.Add(principal);
            }
        }

        return principals;
    }

    /// <summary>
    ///     A group of the principals that are currently referenced by at least one dependent's item. A
    ///     principal enters when it gains its first referencing dependent and leaves when it loses its last,
    ///     firing entity-added / entity-removed notifications with the referencing dependent as the
    ///     principal argument (mirroring <c>CollectionEntityRelationship.Dependents</c> so the same change
    ///     listeners and queued messages work against either relationship shape).
    /// </summary>
    public IEntityGroup<TEntity> Principals
        => _referencedPrincipals ??= new ReferencedPrincipalsGroup(this);

    public void AddDependentsListener(IDependentEntityChangeListener<TEntity> listener)
        => ((DependentEntityGroupBase<TEntity>)Principals).AddDependentListener(listener);

    public void RemoveDependentsListener(IDependentEntityChangeListener<TEntity> listener)
        => ((DependentEntityGroupBase<TEntity>)Principals).RemoveDependentListener(listener);

    void IEntityChangeListener<TEntity>.OnEntityAdded(in EntityChange<TEntity> entityChange)
    {
        var entity = entityChange.Entity;
        var collection = _getCollection(entity);
        if (collection == null)
        {
            return;
        }

        Debug.Assert(!_dependents.ContainsKey(entity.Id), $"Dependent '{entity.Id}' already tracked in '{Name}'");

        var state = new DependentState(entity);
        _dependents[entity.Id] = state;

        foreach (var item in collection)
        {
            AddInitialItem(state, item);
        }
    }

    void IEntityChangeListener<TEntity>.OnEntityRemoved(in EntityChange<TEntity> entityChange)
    {
        if (!_dependents.Remove(entityChange.Entity.Id, out var state))
        {
            return;
        }

        if (state.KeyCounts.Count > 0)
        {
            var keys = new int[state.KeyCounts.Count];
            state.KeyCounts.Keys.CopyTo(keys, 0);
            foreach (var key in keys)
            {
                var principal = _principalGroup.FindEntity(key);
                // Drop the reverse link first (firing the referenced-principal removal while the principal is
                // still intact), then let the owner run any structural teardown of the now-orphaned principal.
                RemoveFromReverse(key, state.Entity);
                if (_handleLastDependentRemoved != null
                    && principal != null)
                {
                    _handleLastDependentRemoved(state.Entity, principal);
                }
            }
        }

        state.KeyCounts.Clear();
    }

    void IEntityChangeListener<TEntity>.OnPropertyValuesChanged(in EntityChange<TEntity> entityChange)
    {
        if (!_dependents.TryGetValue(entityChange.Entity.Id, out var state))
        {
            return;
        }

        var keyPropertyUsed = false;
        var changes = entityChange.PropertyChanges;
        for (var i = 0; i < changes.Count; i++)
        {
            var changedComponent = changes.GetChangedComponent(i);
            if (_foreignKeyValueGetter.PropertyUsed(
                    changedComponent.ComponentId, changes.GetChangedPropertyName(i)))
            {
                keyPropertyUsed = true;
                break;
            }
        }

        if (!keyPropertyUsed)
        {
            return;
        }

        var hasOldKey = TryGetForeignKey(entityChange, ValueType.PreferOld, out var oldKey);
        var hasCurrentKey = TryGetForeignKey(entityChange, ValueType.Current, out var currentKey);
        if (hasOldKey == hasCurrentKey
            && (!hasOldKey || oldKey == currentKey))
        {
            return;
        }

        using var reference = entityChange.Entity.AddReference(this);
        if (hasOldKey)
        {
            RemoveKey(state, oldKey);
        }

        if (hasCurrentKey)
        {
            AddKey(state, currentKey);
        }
    }

    private void AddInitialItem(DependentState state, TItem item)
    {
        var component = item.ContainingComponent;
        var propertyName = item.ContainingPropertyName;
        if (component == null
            || propertyName == null)
        {
            throw new InvalidOperationException(
                $"An item added to '{Name}' must have a containing component and property name.");
        }

        var changes = IPropertyValueChanges.Create(
            new PropertyValueChange<EntityCollectionChange>(
                component,
                propertyName,
                default,
                new EntityCollectionChange(EntityCollectionChangeAction.Add, item)));
        try
        {
            var entityChange = new EntityChange<TEntity>(state.Entity, changes);
            if (TryGetForeignKey(entityChange, ValueType.Current, out var key))
            {
                AddKey(state, key);
            }
        }
        finally
        {
            changes.Return();
        }
    }

    private bool TryGetForeignKey(in EntityChange<TEntity> entityChange, ValueType valueType, out int key)
        => _foreignKeyValueGetter.TryGetKey(entityChange, valueType, out key) && key != 0;

    private void AddKey(DependentState state, int key)
    {
        if (!_principalGroup.ContainsEntity(key)
            && state.Entity.Manager?.IsLoading != true)
        {
            throw new InvalidOperationException(
                $"Couldn't find principal '{key}' in '{_principalGroup.Name}' referenced from entity"
                + $" '{state.Entity.Id}' through '{Name}'");
        }

        state.KeyCounts.TryGetValue(key, out var count);
        state.KeyCounts[key] = count + 1;
        if (count != 0)
        {
            return;
        }

        if (!_principalToDependents.TryGetValue(key, out var set))
        {
            set = new HashSet<TEntity>(EntityEqualityComparer<TEntity>.Instance);
            _principalToDependents[key] = set;
        }

        set.Add(state.Entity);

        var principal = _principalGroup.FindEntity(key);
        if (principal == null
            && _keepPrincipalAlive
            && state.Entity.Manager is { IsLoading: true } manager)
        {
            // During load the principal may not be materialized yet; pull it in so a dependent that has no
            // other keep-alive anchor is still kept alive.
            manager.LoadEntity(key);
            principal = _principalGroup.FindEntity(key);
        }

        if (_keepPrincipalAlive
            && principal != null)
        {
            state.Entity.AddReference(principal);
        }

        if (_referencedPrincipals != null
            && set.Count == 1
            && principal != null)
        {
            _referencedPrincipals.Add(new EntityChange<TEntity>(principal), state.Entity);
        }
    }

    private void RemoveKey(DependentState state, int key)
    {
        if (!state.KeyCounts.TryGetValue(key, out var count))
        {
            return;
        }

        if (count <= 1)
        {
            var principal = _principalGroup.FindEntity(key);
            state.KeyCounts.Remove(key);
            RemoveFromReverse(key, state.Entity);
            if (_handleLastDependentRemoved != null
                && key != _principalBeingRemoved
                && principal != null)
            {
                _handleLastDependentRemoved(state.Entity, principal);
            }
        }
        else
        {
            state.KeyCounts[key] = count - 1;
        }
    }

    private void RemoveFromReverse(int key, TEntity dependent)
    {
        if (!_principalToDependents.TryGetValue(key, out var set)
            || !set.Remove(dependent))
        {
            return;
        }

        // Release the keep-alive reference this dependent held on the principal, unless the principal is the
        // one currently being removed (OnPrincipalRemoved releases those together, after re-anchoring).
        if (_keepPrincipalAlive
            && key != _principalBeingRemoved
            && _principalGroup.FindEntity(key) is { } livePrincipal)
        {
            dependent.RemoveReference(livePrincipal);
        }

        if (set.Count == 0)
        {
            _principalToDependents.Remove(key);
            if (_referencedPrincipals != null
                && _principalGroup.FindEntity(key) is { } principal)
            {
                _referencedPrincipals.Remove(
                    new EntityChange<TEntity>(principal, removedComponent: _getPrincipalRemovedComponent?.Invoke(principal)),
                    dependent);
            }
        }
    }

    private void OnPrincipalRemoved(TEntity principal)
    {
        if (!_principalToDependents.TryGetValue(principal.Id, out var set)
            || set.Count == 0)
        {
            return;
        }

        var affected = new TEntity[set.Count];
        set.CopyTo(affected);

        if (_referencedPrincipals != null)
        {
            var removedComponent = _getPrincipalRemovedComponent?.Invoke(principal);
            foreach (var dependent in affected)
            {
                _referencedPrincipals.Remove(new EntityChange<TEntity>(principal, removedComponent: removedComponent), dependent);
            }
        }

        // While the handlers run, defer the per-item keep-alive release in RemoveFromReverse (a revert-style
        // handler drops items, firing collection events): each dependent's single reference on this principal
        // is released together below, after the handlers have had a chance to re-anchor the dependents.
        var previousBeingRemoved = _principalBeingRemoved;
        _principalBeingRemoved = principal.Id;
        foreach (var dependent in affected)
        {
            _handlePrincipalRemoved(dependent, principal);
        }

        _principalBeingRemoved = previousBeingRemoved;

        // The revert branch of the handler removes items (updating the index through collection events); the
        // unlink branch clears foreign keys in place (no event), so reconcile any remaining bookkeeping here.
        _principalToDependents.Remove(principal.Id);
        foreach (var dependent in affected)
        {
            if (_dependents.TryGetValue(dependent.Id, out var state))
            {
                state.KeyCounts.Remove(principal.Id);
            }

            if (_keepPrincipalAlive)
            {
                dependent.RemoveReference(principal);
            }
        }
    }

    public override string ToString() => "ManyToManyEntityRelationship: " + Name;

    private sealed class DependentState(TEntity entity)
    {
        public TEntity Entity
        {
            get;
        } = entity;

        public Dictionary<int, int> KeyCounts
        {
            get;
        } = new();
    }

    private sealed class PrincipalListener(ManyToManyEntityRelationship<TEntity, TItem> relationship)
        : IEntityChangeListener<TEntity>
    {
        public void OnEntityAdded(in EntityChange<TEntity> entityChange)
        {
        }

        public void OnEntityRemoved(in EntityChange<TEntity> entityChange)
            => relationship.OnPrincipalRemoved(entityChange.Entity);

        public void OnPropertyValuesChanged(in EntityChange<TEntity> entityChange)
        {
        }

        public override string ToString() => "Principal " + relationship;
    }

    private sealed class ReferencedPrincipalsGroup(ManyToManyEntityRelationship<TEntity, TItem> relationship)
        : DependentEntityGroupBase<TEntity>(relationship.Name)
    {
        public override TEntity? FindEntity(int id)
            => relationship._principalToDependents.ContainsKey(id)
                ? relationship._principalGroup.FindEntity(id)
                : null;

        public override bool ContainsEntity(int id)
            => relationship._principalToDependents.ContainsKey(id);

        public void Add(in EntityChange<TEntity> entityChange, TEntity dependent)
            => OnDependentAdded(entityChange, dependent);

        public void Remove(in EntityChange<TEntity> entityChange, TEntity dependent)
            => OnDependentRemoved(entityChange, dependent);

        public override string ToString() => "ReferencedPrincipals: " + Name;
    }
}
