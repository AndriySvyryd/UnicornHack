namespace UnicornHack.Utils.MessagingECS;

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

    bool IEntityChangeListener<TEntity>.OnPropertyValuesChanged(in EntityChange<TEntity> entityChange)
    {
        Debug.Assert(entityChange.RemovedComponent == null);

        Component? componentUsed = null;
        var changes = entityChange.PropertyChanges;
        for (var i = 0; i < changes.Count; i++)
        {
            var changedComponent = changes.GetChangedComponent(i);
            if (KeyValueGetter.PropertyUsed(changedComponent.ComponentId, changes.GetChangedPropertyName(i)))
            {
                // The component might have been removed by the previous change listener
                if (entityChange.Entity.FindComponent(changedComponent.ComponentId) != changedComponent)
                {
                    return true;
                }

                componentUsed = changedComponent;
                break;
            }
        }

        if (componentUsed == null)
        {
            return HandleNonKeyPropertyValuesChanged(entityChange);
        }

        if (KeyValueGetter.TryGetKey(entityChange, ValueType.PreferOld, out var oldKey))
        {
            TryRemoveEntity(oldKey, entityChange);
        }

        if (KeyValueGetter.TryGetKey(entityChange, ValueType.Current, out var newKey))
        {
            TryAddEntity(newKey, entityChange);
        }

        return true;
    }

    protected abstract bool TryAddEntity(TKey key, in EntityChange<TEntity> entityChange);
    protected abstract bool TryRemoveEntity(TKey key, in EntityChange<TEntity> entityChange);
    protected abstract bool HandleNonKeyPropertyValuesChanged(in EntityChange<TEntity> entityChange);
}
