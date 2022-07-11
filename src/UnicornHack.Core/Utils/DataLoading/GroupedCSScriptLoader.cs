namespace UnicornHack.Utils.DataLoading;

public class GroupedCSScriptLoader<TKey, T> : CSScriptLoaderBase<T> where T : class, ILoadable
    where TKey : notnull
{
    private Dictionary<TKey, List<T>>? _objects;
    protected Dictionary<TKey, List<T>> Objects => _objects ??= Group();
    private readonly Func<T, IEnumerable<TKey>> _keySelector;

    public GroupedCSScriptLoader(string relativePath, Func<T, TKey> keySelector, Type dataType)
        : base(relativePath, dataType)
    {
        _keySelector = i => Sequence.Single(keySelector(i));
    }

    public GroupedCSScriptLoader(string relativePath, Func<T, IEnumerable<TKey>> keySelector, Type dataType)
        : base(relativePath, dataType)
    {
        _keySelector = keySelector;
    }

    protected Dictionary<TKey, List<T>> Group()
    {
        var objects = new Dictionary<TKey, List<T>>();
        foreach (var value in NameLookup.Values)
        {
            foreach (var key in _keySelector(value))
            {
                if (!objects.TryGetValue(key, out var list))
                {
                    list = new List<T>();
                    objects[key] = list;
                }

                list.Add(value);
            }
        }

        return objects;
    }

    public IReadOnlyList<T> GetAllValues(TKey key)
        => Objects.TryGetValue(key, out var list) ? list : new List<T>();

    public IEnumerable<TKey> GetAllKeys()
        => Objects.Keys;
}
