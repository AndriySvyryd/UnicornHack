namespace UnicornHack.Utils.DataLoading;

public abstract class Loader<T> : ILoader<T>
    where T : class, ILoadable
{
    private Dictionary<string, T>? _nameLookup;
    private IReadOnlyList<T>? _list;
    private Dictionary<int, T>? _byId;
    private Dictionary<string, int>? _idByName;
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

    /// <summary>
    ///     Template method invoked once, lazily, the first time this loader is used (through
    ///     <see cref="NameLookup" />). It produces the raw entries via <see cref="CreateNameLookup" />, lets a
    ///     derived loader post-process them through <see cref="Process" />, then builds the dense id index
    ///     over the final set. Derived loaders plug into the two extension points rather than overriding it.
    /// </summary>
    private Dictionary<string, T> Load()
    {
        var nameLookup = Process(CreateNameLookup());
        BuildIdIndex(nameLookup);
        return nameLookup;
    }

    /// <summary>
    ///     Extension point: produces the raw name -> value entries this loader exposes (e.g. by reading
    ///     script files or scanning attributes). Called exactly once per loader.
    /// </summary>
    protected abstract Dictionary<string, T> CreateNameLookup();

    /// <summary>
    ///     Extension point: post-processes the freshly produced entries before they become visible and before
    ///     the id index is built - for example expanding each source into a family of variants. The default
    ///     applies no customization; the returned dictionary becomes the loader's entries.
    /// </summary>
    protected virtual Dictionary<string, T> Process(Dictionary<string, T> nameLookup) => nameLookup;

    /// <summary>
    ///     Builds a dense, deterministic integer id per loaded value, assigned by ordinal name order. Provides
    ///     a compact, persist-friendly handle to a template (e.g. a being's <c>Species</c>). Note: the id
    ///     space shifts if the set of templates changes, so it is stable only for a fixed data set.
    /// </summary>
    private void BuildIdIndex(Dictionary<string, T> nameLookup)
    {
        var byId = new Dictionary<int, T>(nameLookup.Count);
        var idByName = new Dictionary<string, int>(nameLookup.Count, StringComparer.Ordinal);
        var i = 0;
        foreach(var pair in nameLookup)
        {
            i++;
            byId[i] = pair.Value;
            idByName[pair.Value.Name] = i;
        }

        _idByName = idByName;
        _byId = byId;
    }

    /// <summary>Returns the value with the given <see cref="GetId" /> id, or null if none.</summary>
    public T? FindById(int id)
    {
        _ = NameLookup;
        return _byId!.TryGetValue(id, out var variant) ? variant : null;
    }

    /// <summary>Returns the dense id assigned to the named value, or -1 if the name is unknown.</summary>
    public int GetId(string name)
    {
        _ = NameLookup;
        return _idByName!.TryGetValue(name, out var id) ? id : -1;
    }

    public IEnumerable<T> GetAll()
        => NameLookup.Values;

    /// <summary>
    ///     Snapshot of all loaded values as a stable list. Cached for the lifetime of the
    ///     loader — production loaders are append-only at load time and never mutate
    ///     afterwards. Tests that need synthetic data wrap a production loader in
    ///     <c>TestLoader&lt;T&gt;</c> instead of mutating it.
    /// </summary>
    public IReadOnlyList<T> GetAsList() => _list ??= NameLookup.Values.ToList();
}
