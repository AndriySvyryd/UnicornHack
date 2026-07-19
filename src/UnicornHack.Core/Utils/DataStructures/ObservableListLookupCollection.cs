using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Utils.DataStructures;

public abstract class ObservableListLookupCollection<TKey, TValue> : NotificationCollection<TValue>
    where TKey : notnull
    where TValue : NotificationCollectionItem
{
    private static readonly IReadOnlyList<TValue> EmptyValues = Array.Empty<TValue>();
    private readonly Dictionary<TKey, List<TValue>> _lookup = [];

    protected ObservableListLookupCollection(Component component, string propertyName)
        : base(component, propertyName)
    {
    }

    public IEnumerable<TKey> Keys => _lookup.Keys;

    public IEnumerable<IReadOnlyList<TValue>> Values => _lookup.Values;

    public bool ContainsKey(TKey key) => _lookup.ContainsKey(key);

    public IReadOnlyList<TValue> GetValues(TKey key)
        => _lookup.GetValueOrDefault(key) ?? EmptyValues;

    public bool TryGetValues(TKey key, out IReadOnlyList<TValue> values)
    {
        if (_lookup.TryGetValue(key, out var list))
        {
            values = list;
            return true;
        }

        values = EmptyValues;
        return false;
    }

    protected abstract TKey? GetKey(TValue item);

    protected override void InsertItem(int index, TValue item)
    {
        AddToLookup(item, index);
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, TValue item)
    {
        RemoveFromLookup(this[index]);
        AddToLookup(item, index);
        base.SetItem(index, item);
    }

    protected override void RemoveItem(int index)
    {
        RemoveFromLookup(this[index]);
        base.RemoveItem(index);
    }

    protected override void MoveItem(int oldIndex, int newIndex)
    {
        var item = this[oldIndex];
        if (GetKey(item) is { } key
            && _lookup.TryGetValue(key, out var values))
        {
            values.Remove(item);

            var keyIndex = 0;
            var lastPrecedingIndex = oldIndex < newIndex ? newIndex : newIndex - 1;
            for (var i = 0; i <= lastPrecedingIndex; i++)
            {
                if (i != oldIndex
                    && EqualityComparer<TKey>.Default.Equals(GetKey(this[i]), key))
                {
                    keyIndex++;
                }
            }

            values.Insert(keyIndex, item);
        }

        base.MoveItem(oldIndex, newIndex);
    }

    protected override void ClearItems()
    {
        _lookup.Clear();
        base.ClearItems();
    }

    private void AddToLookup(TValue item, int collectionIndex)
    {
        if (GetKey(item) is not { } key)
        {
            return;
        }

        if (!_lookup.TryGetValue(key, out var values))
        {
            values = [];
            _lookup[key] = values;
        }

        var keyIndex = 0;
        for (var i = 0; i < collectionIndex && i < Count; i++)
        {
            if (EqualityComparer<TKey>.Default.Equals(GetKey(this[i]), key))
            {
                keyIndex++;
            }
        }

        values.Insert(keyIndex, item);
    }

    private void RemoveFromLookup(TValue item)
    {
        if (GetKey(item) is not { } key
            || !_lookup.TryGetValue(key, out var values))
        {
            return;
        }

        values.Remove(item);
        if (values.Count == 0)
        {
            _lookup.Remove(key);
        }
    }
}