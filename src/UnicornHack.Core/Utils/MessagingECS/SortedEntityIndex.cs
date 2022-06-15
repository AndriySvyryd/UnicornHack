using System.Collections.Generic;
using System.Linq;

namespace UnicornHack.Utils.MessagingECS;

public class SortedEntityIndex<TEntity, TKey> : EntityIndexBase<TEntity, TKey>
    where TEntity : Entity, new()
{
    private readonly IComparer<TEntity> _comparer;

    public SortedEntityIndex(string name,
        IEntityGroup<TEntity> group,
        IKeyValueGetter<TEntity, TKey> keyValueGetter,
        IComparer<TEntity> comparer,
        IEqualityComparer<TKey> equalityComparer = null)
        : base(name, group, keyValueGetter)
    {
        _comparer = comparer;
        Index = new Dictionary<TKey, SortedSet<TEntity>>(equalityComparer ?? EqualityComparer<TKey>.Default);
    }

    protected Dictionary<TKey, SortedSet<TEntity>> Index
    {
        get;
    }

    public IEnumerable<TEntity> this[TKey key]
        => Index.TryGetValue(key, out var entities) ? entities : Enumerable.Empty<TEntity>();

    private SortedSet<TEntity> GetOrAddEntities(TKey key)
    {
        if (!Index.TryGetValue(key, out var entities))
        {
            entities = new SortedSet<TEntity>(_comparer);
            Index.Add(key, entities);
        }

        return entities;
    }

    protected override bool TryAddEntity(TKey key, in EntityChange<TEntity> entityChange)
        => GetOrAddEntities(key).Add(entityChange.Entity);

    protected override bool TryRemoveEntity(TKey key, in EntityChange<TEntity> entityChange)
        => GetOrAddEntities(key).Remove(entityChange.Entity);

    protected override bool HandleNonKeyPropertyValuesChanged(in EntityChange<TEntity> entityChange)
        => false;

    public override string ToString() => "SortedIndex: " + Name;
}
