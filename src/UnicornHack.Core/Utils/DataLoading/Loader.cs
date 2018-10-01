using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace UnicornHack.Utils.DataLoading
{
    public abstract class Loader<T>
        where T : class, ILoadable
    {
        protected ConcurrentDictionary<string, T> NameLookup { get; } =
            new ConcurrentDictionary<string, T>(StringComparer.OrdinalIgnoreCase);

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

            if (NameLookup.TryGetValue(name, out var variant))
            {
                return variant;
            }

            return null;
        }

        public IEnumerable<T> GetAll()
        {
            EnsureLoaded();
            return NameLookup.Values;
        }

        protected abstract void EnsureLoaded();
    }
}
