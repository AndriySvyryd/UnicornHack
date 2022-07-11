namespace UnicornHack.Utils.DataLoading;

public class CSScriptLoader<T> : CSScriptLoaderBase<T> where T : class, ILoadable
{
    private IReadOnlyList<T>? _objects;

    public CSScriptLoader(string relativePath, Type dataType) : base(relativePath, dataType)
    {
    }

    public IReadOnlyList<T> GetAsList() => _objects ??= NameLookup.Values.ToList();
}
