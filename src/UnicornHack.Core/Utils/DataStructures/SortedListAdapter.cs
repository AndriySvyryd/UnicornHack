using System;
using System.Collections;
using System.Collections.Generic;

namespace UnicornHack.Utils.DataStructures;

public class SortedListAdapter<TKey, TValue> : IList<TValue>, ISnapshotableCollection<TValue>
{
    private readonly Func<TValue, TKey> _keyAccessor;

    public SortedListAdapter(Func<TValue, TKey> keyAccessor)
        : this(new SortedList<TKey, TValue>(), keyAccessor)
    {
    }

    public SortedListAdapter(SortedList<TKey, TValue> list, Func<TValue, TKey> keyAccessor)
    {
        List = list;
        _keyAccessor = keyAccessor;
    }

    public SortedList<TKey, TValue> List
    {
        get;
    }

    public int Count => List.Count;
    public bool IsReadOnly => List.Values.IsReadOnly;

    public HashSet<TValue> Snapshot
    {
        get;
        private set;
    }

    public HashSet<TValue> CreateSnapshot()
    {
        if (Snapshot == null)
        {
            Snapshot = new HashSet<TValue>(List.Values);
        }
        else
        {
            Snapshot.Clear();
            Snapshot.AddRange(List.Values);
        }

        return Snapshot;
    }

    public IEnumerator<TValue> GetEnumerator() => List.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => List.Values.GetEnumerator();
    public void Add(TValue item) => List.Add(_keyAccessor(item), item);
    public void Clear() => List.Clear();
    public bool Contains(TValue item) => List.ContainsValue(item);
    public void CopyTo(TValue[] array, int arrayIndex) => List.Values.CopyTo(array, arrayIndex);
    public bool Remove(TValue item) => List.Remove(_keyAccessor(item));
    public int IndexOf(TValue item) => List.IndexOfValue(item);
    public void Insert(int index, TValue item) => throw new NotSupportedException();
    public void RemoveAt(int index) => List.RemoveAt(index);

    public TValue this[int index]
    {
        get => List.Values[index];
        set => List.Values[index] = value;
    }
}
