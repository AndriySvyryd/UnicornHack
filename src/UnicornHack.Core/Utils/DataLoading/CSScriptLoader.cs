namespace UnicornHack.Utils.DataLoading;

public class CSScriptLoader<T> : CSScriptLoaderBase<T> where T : class, ILoadable
{
    public CSScriptLoader(string relativePath, Type dataType) : base(relativePath, dataType)
    {
    }
}
