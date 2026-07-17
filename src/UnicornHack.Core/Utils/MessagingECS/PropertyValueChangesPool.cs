namespace UnicornHack.Utils.MessagingECS;

/// <summary>
///     A small per-thread FIFO object pool backing the pooled <see cref="IPropertyValueChanges" />
///     implementations. FIFO ordering means a just-returned instance is appended to the tail and
///     the next rent hands out the oldest returned instance from the head, rather than the one most
///     recently released — reducing the chance of handing back an instance a previous holder might
///     still touch. The backing storage is <see cref="ThreadStaticAttribute">thread-static</see> so
///     no locking is needed and pooled instances never cross threads: a wrapper is always rented
///     and released on the same game-logic thread (a captured wrapper is released when the
///     capturing message is cleaned during the same single-threaded queue drain).
/// </summary>
public static class PropertyValueChangesPool<T>
    where T : class
{
    private const int Capacity = 32;

    [ThreadStatic]
    private static T?[]? _items;

    [ThreadStatic]
    private static int _head;

    [ThreadStatic]
    private static int _tail;

    [ThreadStatic]
    private static int _count;

    public static T? TryRent()
    {
        if (_count == 0)
        {
            return null;
        }

        var items = _items!;
        var item = items[_head];
        items[_head] = null;
        _head = _head + 1 == Capacity ? 0 : _head + 1;
        _count--;
        return item;
    }

    public static void Return(T item)
    {
        if (_count == Capacity)
        {
            // Pool full: drop the surplus instance and let the GC reclaim it.
            return;
        }

        var items = _items ??= new T?[Capacity];
        items[_tail] = item;
        _tail = _tail + 1 == Capacity ? 0 : _tail + 1;
        _count++;
    }
}
