using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnicornHack.Utils.MessagingECS;

public class UniqueEntityIndex<TEntity, TKey> : EntityIndexBase<TEntity, TKey>, IEnumerable<TEntity>
    where TEntity : Entity, new()
{
    public UniqueEntityIndex(
        string name,
        IEntityGroup<TEntity> group,
        IKeyValueGetter<TEntity, TKey> keyValueGetter,
        IEqualityComparer<TKey> comparer = null)
        : base(name, group, keyValueGetter)
    {
        Index = new Dictionary<TKey, TEntity>(comparer ?? EqualityComparer<TKey>.Default);
    }

    protected Dictionary<TKey, TEntity> Index
    {
        get;
    }

    public TEntity this[TKey key] => Index.TryGetValue(key, out var entity) ? entity : null;

    protected override bool TryAddEntity(TKey key, in EntityChange<TEntity> entityChange)
    {
        if (Index.ContainsKey(key))
        {
            throw new InvalidOperationException(
                $"The key {key} already exists in the unique index for {Group.Name}");
        }

        Index[key] = entityChange.Entity;

        return true;
    }

    protected override bool TryRemoveEntity(TKey key, in EntityChange<TEntity> entityChange)
    {
        var removed = Index.Remove(key);
        Debug.Assert(removed);

        return true;
    }

    protected override bool HandleNonKeyPropertyValuesChanged(in EntityChange<TEntity> entityChange)
        => false;

    public override string ToString() => "UniqueIndex: " + Name;

    public IEnumerator<TEntity> GetEnumerator()
        => Index.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
