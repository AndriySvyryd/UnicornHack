namespace UnicornHack.Utils.DataLoading;

public class IndexedGroupedCSScriptLoader<TKey, TIndexKey, T> :
    GroupedCSScriptLoader<TKey, T>, IIndexedGroupedLoader<TKey, TIndexKey, T>
    where T : class, ILoadable
    where TKey : notnull
    where TIndexKey : notnull
{
    private readonly Func<T, TIndexKey> _indexKeySelector;
    private readonly IEqualityComparer<TIndexKey>? _indexKeyComparer;
    private Dictionary<TIndexKey, T>? _index;

    public IndexedGroupedCSScriptLoader(
        string relativePath,
        Func<T, TKey> keySelector,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type dataType,
        Func<T, TIndexKey> indexKeySelector,
        IEqualityComparer<TIndexKey>? indexKeyComparer = null)
        : base(relativePath, keySelector, dataType)
    {
        _indexKeySelector = indexKeySelector;
        _indexKeyComparer = indexKeyComparer;
    }

    public T? FindByIndex(TIndexKey key)
        => GetIndex().TryGetValue(key, out var value) ? value : null;

    private Dictionary<TIndexKey, T> GetIndex()
    {
        if (_index != null)
        {
            return _index;
        }

        var index = new Dictionary<TIndexKey, T>(_indexKeyComparer);
        foreach (var value in GetAll())
        {
            var key = _indexKeySelector(value);
            if (!index.TryAdd(key, value))
            {
                throw new InvalidOperationException(
                    $"Index key '{key}' is used by multiple {typeof(T).Name} templates.");
            }
        }

        return _index = index;
    }
}