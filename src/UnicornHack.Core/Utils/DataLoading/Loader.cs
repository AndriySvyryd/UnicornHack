using System;
using System.Collections.Generic;

namespace UnicornHack.Utils.DataLoading;

public abstract class Loader<T>
    where T : class, ILoadable
{
    protected Dictionary<string, T> NameLookup
    {
        get;
        set;
    }

    public T Get(string name)
    {
        var loadable = Find(name);
        if (loadable != null)
        {
            return loadable;
        }

        throw new InvalidOperationException($"'{name}' not found");
    }

    public T Find(string name)
    {
        EnsureLoaded();
        return name != null && NameLookup.TryGetValue(name, out var variant)
            ? variant
            : null;
    }

    public IEnumerable<T> GetAll()
    {
        EnsureLoaded();
        return NameLookup.Values;
    }

    protected abstract void EnsureLoaded();
}
