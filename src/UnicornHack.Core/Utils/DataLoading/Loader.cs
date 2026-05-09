namespace UnicornHack.Utils.DataLoading;

public abstract class Loader<T> : ILoader<T>
    where T : class, ILoadable
{
    private Dictionary<string, T>? _nameLookup;
    private IReadOnlyList<T>? _list;
    protected Dictionary<string, T> NameLookup => _nameLookup ??= Load();

    public T Get(string name)
    {
        var loadable = Find(name);
        if (loadable != null)
        {
            return loadable;
        }

        throw new InvalidOperationException($"{typeof(T).Name} '{name}' not found");
    }

    public T? Find(string name)
        => NameLookup.TryGetValue(name, out var variant)
        ? variant
        : null;

    public IEnumerable<T> GetAll()
        => NameLookup.Values;

    /// <summary>
    ///     Snapshot of all loaded values as a stable list. Cached for the lifetime of the
    ///     loader — production loaders are append-only at load time and never mutate
    ///     afterwards. Tests that need synthetic data wrap a production loader in
    ///     <c>TestLoader&lt;T&gt;</c> instead of mutating it.
    /// </summary>
    public IReadOnlyList<T> GetAsList() => _list ??= NameLookup.Values.ToList();

    protected abstract Dictionary<string, T> Load();
}
