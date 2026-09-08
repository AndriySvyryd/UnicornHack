using System.Collections.ObjectModel;

namespace UnicornHack.Utils.DataStructures;

public abstract class ObservableLookupCollection<TKey, TValue> : ObservableCollection<TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _lookup = [];

    public TValue this[TKey key] => _lookup[key];

    public IEnumerable<TKey> Keys => _lookup.Keys;

    public IEnumerable<TValue> Values => _lookup.Values;

    public bool ContainsKey(TKey key) => _lookup.ContainsKey(key);

    public TValue? GetValueOrDefault(TKey key)
        => _lookup.GetValueOrDefault(key);

    public bool TryGetValue(TKey key, out TValue value)
        => _lookup.TryGetValue(key, out value!);

    protected abstract TKey GetKey(TValue item);

    protected override void InsertItem(int index, TValue item)
    {
        _lookup.Add(GetKey(item), item);
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, TValue item)
    {
        var oldKey = GetKey(this[index]);
        var newKey = GetKey(item);
        if (!EqualityComparer<TKey>.Default.Equals(oldKey, newKey))
        {
            _lookup.Add(newKey, item);
            _lookup.Remove(oldKey);
        }
        else
        {
            _lookup[newKey] = item;
        }

        base.SetItem(index, item);
    }

    protected override void RemoveItem(int index)
    {
        _lookup.Remove(GetKey(this[index]));
        base.RemoveItem(index);
    }

    protected override void ClearItems()
    {
        _lookup.Clear();
        base.ClearItems();
    }
}
