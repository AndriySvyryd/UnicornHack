using UnicornHack.Utils.DataLoading;

namespace UnicornHack;

public class TestIndexedLoader<TIndexKey, T> : TestLoader<T>, IIndexedLoader<TIndexKey, T>
    where T : class, ILoadable
    where TIndexKey : notnull
{
    private readonly Func<T, TIndexKey> _indexKeySelector;
    private readonly IEqualityComparer<TIndexKey> _indexKeyComparer;

    public TestIndexedLoader(
        Func<T, TIndexKey> indexKeySelector, IIndexedLoader<TIndexKey, T>? production = null,
        IEqualityComparer<TIndexKey>? indexKeyComparer = null)
        : base(production)
    {
        _indexKeySelector = indexKeySelector;
        _indexKeyComparer = indexKeyComparer ?? EqualityComparer<TIndexKey>.Default;
    }

    public T? FindByIndex(TIndexKey key)
    {
        foreach (var item in TestItems)
        {
            if (_indexKeyComparer.Equals(_indexKeySelector(item), key))
            {
                return item;
            }
        }

        return IncludeProduction
            ? (Production as IIndexedLoader<TIndexKey, T>)?.FindByIndex(key)
            : null;
    }
}

public class TestIndexedGroupedLoader<TKey, TIndexKey, T> :
    TestGroupedLoader<TKey, T>, IIndexedGroupedLoader<TKey, TIndexKey, T>
    where T : class, ILoadable
    where TKey : notnull
    where TIndexKey : notnull
{
    private readonly Func<T, TIndexKey> _indexKeySelector;
    private readonly IEqualityComparer<TIndexKey> _indexKeyComparer;

    public TestIndexedGroupedLoader(
        Func<T, TKey> keySelector, Func<T, TIndexKey> indexKeySelector,
        IIndexedGroupedLoader<TKey, TIndexKey, T>? production = null,
        IEqualityComparer<TIndexKey>? indexKeyComparer = null)
        : base(keySelector, production)
    {
        _indexKeySelector = indexKeySelector;
        _indexKeyComparer = indexKeyComparer ?? EqualityComparer<TIndexKey>.Default;
    }

    public T? FindByIndex(TIndexKey key)
    {
        foreach (var item in TestItems)
        {
            if (_indexKeyComparer.Equals(_indexKeySelector(item), key))
            {
                return item;
            }
        }

        return IncludeProduction
            ? (Production as IIndexedLoader<TIndexKey, T>)?.FindByIndex(key)
            : null;
    }
}