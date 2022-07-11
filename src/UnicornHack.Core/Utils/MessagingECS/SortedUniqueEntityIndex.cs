namespace UnicornHack.Utils.MessagingECS;

public class SortedUniqueEntityIndex<TEntity, TKey> : EntityIndexBase<TEntity, TKey>
    where TEntity : Entity, new()
    where TKey : notnull
{
    public SortedUniqueEntityIndex(
        string name,
        IEntityGroup<TEntity> group,
        IKeyValueGetter<TEntity, TKey> keyValueGetter,
        IComparer<TKey>? comparer = null)
        : base(name, group, keyValueGetter)
    {
        Index = new SortedDictionary<TKey, TEntity>(comparer ?? Comparer<TKey>.Default);
    }

    protected SortedDictionary<TKey, TEntity> Index
    {
        get;
    }

    public TEntity? this[TKey key] => Index.TryGetValue(key, out var entity) ? entity : null;

    protected override bool TryAddEntity(TKey key, in EntityChange<TEntity> entityChange)
    {
        Debug.Assert(!Index.TryGetValue(key, out _));

        Index[key] = entityChange.Entity;

        return true;
    }

    protected override bool TryRemoveEntity(TKey key, in EntityChange<TEntity> entityChange)
    {
        Debug.Assert(Index.TryGetValue(key, out _));

        return Index.Remove(key);
    }

    protected override bool HandleNonKeyPropertyValuesChanged(in EntityChange<TEntity> entityChange)
        => false;

    public TEntity First() => Index.First().Value;

    public TEntity Last() => Index.Last().Value;

    public int Count => Index.Count;

    public override string ToString() => "SortedIndex: " + Name;
}
