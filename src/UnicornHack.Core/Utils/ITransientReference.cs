namespace UnicornHack.Utils;

public interface ITransientReference<out T> : IDisposable
    where T : class, IReferenceable
{
    T Referenced
    {
        get;
    }
}
