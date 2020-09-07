using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnicornHack.Utils.Caching
{
    public class ListObjectPool<T> : IObjectPool
        where T : class
    {
        private readonly List<T> _items;
        private readonly Func<T> _factory;
        private readonly int _preallocatedCount;
        private readonly int _maxSize;

        public ListObjectPool(Func<T> factory, int initialSize, int maxSize, int preallocatedCount)
        {
            Debug.Assert(initialSize >= 1);
            Debug.Assert(initialSize >= preallocatedCount);
            Debug.Assert(maxSize >= initialSize);

            _factory = factory;
            _items = new List<T>(initialSize);
            _maxSize = maxSize;
            _preallocatedCount = preallocatedCount - 1;
        }

        object IObjectPool.Rent() => Rent();

        public T Rent()
        {
            var items = _items;
            if (items.Count > 0)
            {
                var obj = items[items.Count - 1];
                items.RemoveAt(items.Count - 1);
                return obj;
            }

            for (var i = 0; i < _preallocatedCount - 1; i++)
            {
                var pooledItem = _factory();
                (pooledItem as IPoolable)?.SetPool(this);
                _items.Add(pooledItem);
            }

            var item = _factory();
            (item as IPoolable)?.SetPool(this);
            return item;
        }

        public void Return(object obj) => Return((T)obj);

        public void Return(T obj)
        {
            if (_items.Count < _maxSize)
            {
                _items.Add(obj);
            }
        }
    }
}
