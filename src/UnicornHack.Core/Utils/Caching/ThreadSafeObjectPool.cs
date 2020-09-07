using System;
using System.Diagnostics;
using System.Threading;

namespace UnicornHack.Utils.Caching
{
    public class ThreadSafeObjectPool<T> : IObjectPool
        where T : class
    {
        private readonly T[] _items;
        private readonly Func<T> _factory;
        private readonly int _preallocatedCount;

        public ThreadSafeObjectPool(Func<T> factory)
            : this(factory, Environment.ProcessorCount * 2, 0)
        {
        }

        public ThreadSafeObjectPool(Func<T> factory, int size, int preallocatedCount)
        {
            Debug.Assert(size >= 1);
            Debug.Assert(size >= preallocatedCount);

            _factory = factory;
            _items = new T[size];
            _preallocatedCount = preallocatedCount;
        }

        object IObjectPool.Rent() => Rent();

        public T Rent()
        {
            var items = _items;
            for (var i = 0; i < items.Length; i++)
            {
                var obj = items[i];
                if (obj != null
                    && obj == Interlocked.CompareExchange(ref items[i], null, obj)
                )
                {
                    return obj;
                }
            }

            if (_preallocatedCount > 1)
            {
                lock (_items)
                {
                    for (var i = 0; i < _preallocatedCount - 1; i++)
                    {
                        var pooledItem = _factory();
                        (pooledItem as IPoolable)?.SetPool(this);
                        _items[i] = _factory();
                    }
                }
            }

            var item = _factory();
            (item as IPoolable)?.SetPool(this);
            return _factory();
        }

        public void Return(object obj) => Return((T)obj);

        public void Return(T obj)
        {
            var items = _items;
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i] != null)
                {
                    continue;
                }

                items[i] = obj;
                break;
            }
        }
    }
}
