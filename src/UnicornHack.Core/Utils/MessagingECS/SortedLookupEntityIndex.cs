using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UnicornHack.Utils.MessagingECS;

// TODO: Add tests
public class SortedLookupEntityIndex<TEntity, TKey, TSortKey> : EntityIndexBase<TEntity, TKey>
    where TEntity : Entity, new()
{
    private readonly IKeyValueGetter<TEntity, TSortKey> _sortValueGetter;
    private readonly IComparer<TSortKey> _comparer;

    private static readonly IReadOnlyDictionary<TSortKey, TEntity> EmptyDictionary =
        new Dictionary<TSortKey, TEntity>();

    public SortedLookupEntityIndex(
        string name,
        IEntityGroup<TEntity> group,
        IKeyValueGetter<TEntity, TKey> keyValueGetter,
        IKeyValueGetter<TEntity, TSortKey> sortValueGetter,
        IComparer<TSortKey> comparer = null)
        : base(name, group, keyValueGetter)
    {
        _sortValueGetter = sortValueGetter;
        _comparer = comparer ?? Comparer<TSortKey>.Default;
        Index = new Dictionary<TKey, SortedDictionary<TSortKey, TEntity>>();
    }

    public SortedLookupEntityIndex(
        string name,
        EntityGroup<TEntity> group,
        IKeyValueGetter<TEntity, TKey> keyValueGetter,
        IKeyValueGetter<TEntity, TSortKey> sortValueGetter,
        IEqualityComparer<TKey> equalityComparer,
        IComparer<TSortKey> comparer = null)
        : base(name, group, keyValueGetter)
    {
        _sortValueGetter = sortValueGetter;
        _comparer = comparer ?? Comparer<TSortKey>.Default;
        Index = new Dictionary<TKey, SortedDictionary<TSortKey, TEntity>>(equalityComparer);
    }

    protected Dictionary<TKey, SortedDictionary<TSortKey, TEntity>> Index
    {
        get;
    }

    public IReadOnlyDictionary<TSortKey, TEntity> this[TKey key]
        => Index.TryGetValue(key, out var entities) ? entities : EmptyDictionary;

    private SortedDictionary<TSortKey, TEntity> GetOrAddEntities(TKey key)
    {
        if (!Index.TryGetValue(key, out var entities))
        {
            entities = new SortedDictionary<TSortKey, TEntity>(_comparer);
            Index.Add(key, entities);
        }

        return entities;
    }

    protected override bool TryAddEntity(TKey key, in EntityChange<TEntity> entityChange)
    {
        if (_sortValueGetter.TryGetKey(entityChange, ValueType.Current, out var sortKey))
        {
            GetOrAddEntities(key).Add(sortKey, entityChange.Entity);

            return true;
        }

        return false;
    }

    protected override bool TryRemoveEntity(TKey key, in EntityChange<TEntity> entityChange)
    {
        if (!Index.TryGetValue(key, out var entities))
        {
            return false;
        }

        if (!_sortValueGetter.TryGetKey(entityChange, ValueType.PreferOld, out var sortKey))
        {
            var entity = entityChange.Entity;
            // This would only happen if the sort key is changed in the component after it has been removed
            Debug.Assert(entities.All(p => p.Value != entity));
            return false;
        }

        if (entities.Remove(sortKey))
        {
            if (entities.Count == 0)
            {
                Index.Remove(key);
            }

            return true;
        }

        return false;
    }

    protected override bool HandleNonKeyPropertyValuesChanged(in EntityChange<TEntity> entityChange)
    {
        Debug.Assert(entityChange.RemovedComponent == null);

        if (!KeyValueGetter.TryGetKey(entityChange, ValueType.Current, out var key))
        {
            return false;
        }

        Component componentUsed = null;
        var changes = entityChange.PropertyChanges;
        for (var i = 0; i < changes.Count; i++)
        {
            var changedComponent = changes.GetChangedComponent(i);
            if (_sortValueGetter.PropertyUsed(changedComponent.ComponentId, changes.GetChangedPropertyName(i)))
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
            return false;
        }

        var entities = GetOrAddEntities(key);

        if (_sortValueGetter.TryGetKey(entityChange, ValueType.PreferOld, out var oldSortKey))
        {
            entities.Remove(oldSortKey);
        }

        if (_sortValueGetter.TryGetKey(entityChange, ValueType.Current, out var newSortKey))
        {
            entities.Add(newSortKey, entityChange.Entity);
        }

        if (entities.Count == 0)
        {
            Index.Remove(key);
        }

        return true;
    }

    public override string ToString() => "SortedLookupIndex: " + Name;
}
