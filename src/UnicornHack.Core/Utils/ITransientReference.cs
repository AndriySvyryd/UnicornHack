using System;

namespace UnicornHack.Utils;

public interface ITransientReference<out T> : IDisposable
    where T : class, IOwnerReferenceable
{
    T Referenced
    {
        get;
    }
}
