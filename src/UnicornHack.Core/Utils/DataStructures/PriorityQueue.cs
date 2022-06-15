using System.Collections;
using System.Collections.Generic;

namespace UnicornHack.Utils.DataStructures;

// TODO: Use the class from .NET 6
public class PriorityQueue<T> : ISnapshotableCollection<T>
{
    private readonly List<T> _list = new();
    private readonly IComparer<T> _comparer;

    public PriorityQueue() : this(Comparer<T>.Default, 0)
    {
    }

    public PriorityQueue(IComparer<T> comparer) : this(comparer, 0)
    {
    }

    public PriorityQueue(IComparer<T> comparer, int capacity)
    {
        _comparer = comparer;
        if (capacity != 0)
        {
            _list.Capacity = capacity;
        }
    }

    public PriorityQueue(IEnumerable<T> source) : this(source, Comparer<T>.Default)
    {
    }

    public PriorityQueue(IEnumerable<T> source, IComparer<T> comparer) : this(comparer, 0)
    {
        foreach (var element in source)
        {
            Push(element);
        }
    }

    public PriorityQueue(ICollection<T> source) : this(source, Comparer<T>.Default)
    {
    }

    public PriorityQueue(ICollection<T> source, IComparer<T> comparer) : this(comparer, source.Count)
    {
        foreach (var element in source)
        {
            Push(element);
        }
    }

    public int Count => _list.Count;

    public HashSet<T> Snapshot
    {
        get;
        private set;
    }

    public HashSet<T> CreateSnapshot()
    {
        if (Snapshot == null)
        {
            Snapshot = new HashSet<T>(_list);
        }
        else
        {
            Snapshot.Clear();
            Snapshot.AddRange(_list);
        }

        return Snapshot;
    }

    public int Push(T element)
    {
        var newElementPosition = _list.Count;
        _list.Add(element);

        return BubbleUp(newElementPosition);
    }

    public T Pop() => RemoveAt(0);

    public bool Remove(T item)
    {
        var i = GetPosition(item);
        if (i == -1)
        {
            return false;
        }

        RemoveAt(i);
        return true;
    }

    private T RemoveAt(int position)
    {
        var result = _list[position];
        _list[position] = _list[^1];
        _list.RemoveAt(_list.Count - 1);

        if (position != _list.Count)
        {
            BubbleDown(position);
        }

        return result;
    }

    public void Clear() => _list.Clear();

    public int Update(int position)
    {
        var newPosition = BubbleUp(position);
        return newPosition != position ? newPosition : BubbleDown(newPosition);
    }

    public int Update(T item)
        => Update(GetPosition(item));

    private int BubbleUp(int position)
    {
        while (position != 0)
        {
            var parentPosition = (position - 1) >> 1;
            if (CompareItemsAt(parentPosition, position) <= 0)
            {
                break;
            }

            SwitchElements(parentPosition, position);
            position = parentPosition;
        }

        return position;
    }

    private int BubbleDown(int position)
    {
        while (true)
        {
            var parentPosition = position;
            var leftChildPosition = (position << 1) + 1;
            var rightChildPosition = (position << 1) + 2;

            if (_list.Count > leftChildPosition && CompareItemsAt(position, leftChildPosition) > 0)
            {
                position = leftChildPosition;
            }

            if (_list.Count > rightChildPosition && CompareItemsAt(position, rightChildPosition) > 0)
            {
                position = rightChildPosition;
            }

            if (position == parentPosition)
            {
                break;
            }

            SwitchElements(position, parentPosition);
        }

        return position;
    }

    public T Peek() => _list.Count > 0 ? _list[index: 0] : default;

    private int GetPosition(T item)
    {
        for (var i = 0; i < _list.Count; i++)
        {
            if (item.Equals(_list[i]))
            {
                return i;
            }
        }

        return -1;
    }

    private void SwitchElements(int i, int j)
    {
        var temp = _list[i];
        _list[i] = _list[j];
        _list[j] = temp;
    }

    private int CompareItemsAt(int i, int j) => _comparer.Compare(_list[i], _list[j]);

    bool ICollection<T>.IsReadOnly => false;

    void ICollection<T>.Add(T item) => Push(item);
    bool ICollection<T>.Contains(T item) => GetPosition(item) != -1;

    void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
        for (var i = 0; i < _list.Count; i++)
        {
            array[i + arrayIndex] = _list[i];
        }
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => _list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
}
