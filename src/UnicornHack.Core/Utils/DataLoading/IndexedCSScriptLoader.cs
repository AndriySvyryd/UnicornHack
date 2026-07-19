namespace UnicornHack.Utils.DataLoading;

public class IndexedCSScriptLoader<TIndexKey, T> : CSScriptLoader<T>, IIndexedLoader<TIndexKey, T>
    where T : class, ILoadable
    where TIndexKey : notnull
{
    private readonly Func<T, TIndexKey> _indexKeySelector;
    private readonly IEqualityComparer<TIndexKey>? _indexKeyComparer;
    private Dictionary<TIndexKey, T>? _index;

    public IndexedCSScriptLoader(
        string relativePath,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type dataType,
        Func<T, TIndexKey> indexKeySelector,
        IEqualityComparer<TIndexKey>? indexKeyComparer = null)
        : base(relativePath, dataType)
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