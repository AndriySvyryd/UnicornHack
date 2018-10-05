using System;
using System.Collections.Generic;

namespace UnicornHack.Utils.DataLoading
{
    public class GroupedCSScriptLoader<TKey, T> : CSScriptLoaderBase<T> where T : class, ILoadable
    {
        private Dictionary<TKey, List<T>> _objects;
        private readonly Func<T, IEnumerable<TKey>> _keySelector;

        public GroupedCSScriptLoader(string relativePath, Func<T, TKey> keySelector, Type dataType = null)
            : base(relativePath, dataType) => _keySelector = i => Sequence.Single(keySelector(i));

        public GroupedCSScriptLoader(string relativePath, Func<T, IEnumerable<TKey>> keySelector, Type dataType = null)
            : base(relativePath, dataType) => _keySelector = keySelector;

        protected override void EnsureLoaded()
        {
            base.EnsureLoaded();

            if (_objects == null)
            {
                var objects = new Dictionary<TKey, List<T>>();
                foreach (var value in NameLookup.Values)
                {
                    foreach (var key in _keySelector(value))
                    {
                        if (!objects.TryGetValue(key, out var list))
                        {
                            list = new List<T>();
                            objects[key] = list;
                        }

                        list.Add(value);
                    }
                }

                _objects = objects;
            }
        }

        public IReadOnlyList<T> GetAllValues(TKey key)
        {
            EnsureLoaded();
            return _objects.TryGetValue(key, out var list) ? list : new List<T>();
        }

        public IEnumerable<TKey> GetAllKeys()
        {
            EnsureLoaded();
            return _objects.Keys;
        }
    }
}
