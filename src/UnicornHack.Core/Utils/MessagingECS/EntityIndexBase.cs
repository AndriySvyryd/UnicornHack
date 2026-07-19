namespace UnicornHack.Utils.MessagingECS;

/// <summary>
///     Listens on an entity group and maintains a key-based index of entities.
/// </summary>
public abstract class EntityIndexBase<TEntity, TKey> : IEntityChangeListener<TEntity>
    where TEntity : Entity, new()
{
    protected EntityIndexBase(
        string name,
        IEntityGroup<TEntity> group,
        IKeyValueGetter<TEntity, TKey> keyValueGetter)
    {
        Name = name;
        KeyValueGetter = keyValueGetter;
        group.AddListener(this);
        Group = group;
    }

    public string Name
    {
        get;
    }

    protected IKeyValueGetter<TEntity, TKey> KeyValueGetter
    {
        get;
    }

    protected IEntityGroup<TEntity> Group
    {
        get;
    }

    void IEntityChangeListener<TEntity>.OnEntityAdded(in EntityChange<TEntity> entityChange)
    {
        if (KeyValueGetter.TryGetKey(entityChange, ValueType.Current, out var key))
        {
            TryAddEntity(key, entityChange);
        }
    }

    void IEntityChangeListener<TEntity>.OnEntityRemoved(in EntityChange<TEntity> entityChange)
    {
        if (KeyValueGetter.TryGetKey(entityChange, ValueType.PreferOld, out var key))
        {
            TryRemoveEntity(key, entityChange);
        }
    }

    void IEntityChangeListener<TEntity>.OnPropertyValuesChanged(in EntityChange<TEntity> entityChange)
    {
        if (entityChange.AddedComponent != null
            || entityChange.RemovedComponent != null)
        {
            var hasOldKey = KeyValueGetter.TryGetKey(
                entityChange, ValueType.PreferOld, out var componentOldKey);
            var hasCurrentKey = KeyValueGetter.TryGetKey(
                entityChange, ValueType.Current, out var componentCurrentKey);
            if (hasOldKey == hasCurrentKey
                && (!hasOldKey
                    || EqualityComparer<TKey>.Default.Equals(componentOldKey, componentCurrentKey)))
            {
                return;
            }

            using var componentChangeReference = entityChange.Entity.AddReference(this);
            if (hasOldKey)
            {
                TryRemoveEntity(componentOldKey, entityChange);
            }

            if (hasCurrentKey)
            {
                TryAddEntity(componentCurrentKey, entityChange);
            }

            return;
        }

        var keyValueChanged = false;
        var changes = entityChange.PropertyChanges;
        for (var i = 0; i < changes.Count; i++)
        {
            var changedComponent = changes.GetChangedComponent(i);
            if (KeyValueGetter.PropertyUsed(changedComponent.ComponentId, changes.GetChangedPropertyName(i)))
            {
                // The component might have been removed by the previous change listener
                if (entityChange.Entity.FindComponent(changedComponent.ComponentId) != changedComponent)
                {
                    return;
                }

                keyValueChanged = true;
                break;
            }
        }

        // Keep the entity alive across the remove-then-add processing below - a primary-key move handled
        // here, or a secondary-key/cell move handled by the subclass. When an index/relationship is the
        // entity's only anchor (keepDependentAlive), the remove would otherwise drop its last reference and
        // reclaim it before it is re-added.
        using var reference = entityChange.Entity.AddReference(this);

        if (!keyValueChanged)
        {
            // Non-key property changed: delegate to subclass (e.g. relationships handle secondary key changes).
            HandleNonKeyPropertyValuesChanged(entityChange);
            return;
        }

        if (KeyValueGetter.TryGetKey(entityChange, ValueType.PreferOld, out var oldKey))
        {
            TryRemoveEntity(oldKey, entityChange);
        }

        if (KeyValueGetter.TryGetKey(entityChange, ValueType.Current, out var newKey))
        {
            TryAddEntity(newKey, entityChange);
        }
    }

    protected abstract bool TryAddEntity(TKey key, in EntityChange<TEntity> entityChange);
    protected abstract bool TryRemoveEntity(TKey key, in EntityChange<TEntity> entityChange);
    protected abstract void HandleNonKeyPropertyValuesChanged(in EntityChange<TEntity> entityChange);
}
