namespace UnicornHack.Utils.DataLoading;

public class SingleCSScriptLoader<T> : CSScriptLoaderBase<T>, ISingleLoader<T>
    where T : class, ILoadable, new()
{
    private readonly string _name;

    public SingleCSScriptLoader(
        string relativePath,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type dataType)
        : base(relativePath, dataType)
    {
        _name = new T().Name;
        FilePattern = CSScriptLoaderHelpers.GetScriptFilename(_name) + CSScriptLoaderHelpers.ScriptExtension;
    }

    public T Object => Find(_name)!;
}
