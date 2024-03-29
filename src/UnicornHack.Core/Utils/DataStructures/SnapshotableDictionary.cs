using System.Collections;

namespace UnicornHack.Utils.DataStructures;

public class SnapshotableDictionary<TKey, TValue> : ISnapshotableCollection<TValue>
    where TKey : notnull
{
    private readonly Func<TValue, TKey> _keyAccessor;

    public SnapshotableDictionary(Func<TValue, TKey> keyAccessor)
        : this(new Dictionary<TKey, TValue>(), keyAccessor)
    {
    }

    public SnapshotableDictionary(Dictionary<TKey, TValue> dictionary, Func<TValue, TKey> keyAccessor)
    {
        Dictionary = dictionary;
        _keyAccessor = keyAccessor;
    }

    public Dictionary<TKey, TValue> Dictionary
    {
        get;
    }

    public int Count => Dictionary.Count;
    public bool IsReadOnly => false;

    public HashSet<TValue>? Snapshot
    {
        get;
        private set;
    }

    public HashSet<TValue> CreateSnapshot()
    {
        if (Snapshot == null)
        {
            Snapshot = new HashSet<TValue>(Dictionary.Values);
        }
        else
        {
            Snapshot.Clear();
            Snapshot.AddRange(Dictionary.Values);
        }

        return Snapshot;
    }

    public IEnumerator<TValue> GetEnumerator() => Dictionary.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Dictionary.Values.GetEnumerator();
    public void Add(TValue item) => Dictionary.Add(_keyAccessor(item), item);
    public void Clear() => Dictionary.Clear();
    public bool Contains(TValue item) => Dictionary.ContainsValue(item);
    public void CopyTo(TValue[] array, int arrayIndex) => Dictionary.Values.CopyTo(array, arrayIndex);
    public bool Remove(TValue item) => Dictionary.Remove(_keyAccessor(item));
    public void Insert(int _, TValue __) => throw new NotSupportedException();

    public TValue this[TKey index]
    {
        get => Dictionary[index];
        set => Dictionary[index] = value;
    }
}
