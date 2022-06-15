using System.Collections;
using System.Collections.Generic;

namespace UnicornHack.Utils;

public class ReferencingList<T> : IList<T>, IReadOnlyList<T>
    where T : IOwnerReferenceable
{
    private readonly List<T> _list;

    public ReferencingList()
    {
        _list = new List<T>();
    }

    public ReferencingList(int capacity)
    {
        _list = new List<T>(capacity);
    }

    public ReferencingList(IEnumerable<T> collection)
    {
        _list = new List<T>(collection);
    }

    public T this[int index]
    {
        get => _list[index];
        set => _list[index] = value;
    }

    public int Count => _list.Count;

    public bool IsReadOnly => ((IList<T>)_list).IsReadOnly;

    public void Add(T item)
    {
        item.AddReference(this);
        _list.Add(item);
    }

    public void Insert(int index, T item)
    {
        item.AddReference(this);
        _list.Insert(index, item);
    }

    public bool Remove(T item)
    {
        item.RemoveReference(this);
        return _list.Remove(item);
    }

    public void RemoveAt(int index)
    {
        _list[index].RemoveReference(this);
        _list.RemoveAt(index);
    }

    public void Clear()
    {
        foreach (var item in _list)
        {
            item.RemoveReference(this);
        }

        _list.Clear();
    }

    public int IndexOf(T item) => _list.IndexOf(item);

    public bool Contains(T item) => _list.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IList<T>)_list).GetEnumerator();
}
