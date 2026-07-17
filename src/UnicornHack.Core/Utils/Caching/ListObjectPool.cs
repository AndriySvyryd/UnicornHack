namespace UnicornHack.Utils.Caching;

public class ListObjectPool<T> : IObjectPool
    where T : class
{
    // FIFO ring buffer: rented from the head, returned to the tail, so a just-returned instance
    // goes to the back of the line rather than being handed straight back out — reducing the chance
    // of a caller obtaining an instance the code path that returned it is still using.
    private readonly T?[] _items;
    private readonly Func<T> _factory;
    private readonly int _preallocatedCount;
    private int _head;
    private int _tail;
    private int _count;

    public ListObjectPool(Func<T> factory, int initialSize, int maxSize, int preallocatedCount)
    {
        Debug.Assert(initialSize >= 1);
        Debug.Assert(initialSize >= preallocatedCount);
        Debug.Assert(maxSize >= initialSize);

        _factory = factory;
        _items = new T?[maxSize];
        _preallocatedCount = preallocatedCount - 1;
    }

    object IObjectPool.Rent() => Rent();

    public T Rent()
    {
        if (_count > 0)
        {
            var obj = _items[_head]!;
            _items[_head] = null;
            _head = _head + 1 == _items.Length ? 0 : _head + 1;
            _count--;
            return obj;
        }

        // Pool empty: warm up by preallocating a batch so subsequent rents are cheap.
        for (var i = 0; i < _preallocatedCount - 1; i++)
        {
            var pooledItem = _factory();
            (pooledItem as IPoolable)?.SetPool(this);
            Return(pooledItem);
        }

        var item = _factory();
        (item as IPoolable)?.SetPool(this);
        return item;
    }

    public void Return(object obj) => Return((T)obj);

    public void Return(T obj)
    {
        if (_count == _items.Length)
        {
            return;
        }

        _items[_tail] = obj;
        _tail = _tail + 1 == _items.Length ? 0 : _tail + 1;
        _count++;
    }
}
