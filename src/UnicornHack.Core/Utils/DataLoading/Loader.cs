namespace UnicornHack.Utils.DataLoading;

public abstract class Loader<T>
    where T : class, ILoadable
{
    private Dictionary<string, T>? _nameLookup;
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

    protected abstract Dictionary<string, T> Load();
}
