using System;
using System.Collections;
using System.Collections.Generic;

namespace UnicornHack.Utils
{
    public class SortedListAdapter<TKey, TValue> : IList<TValue>, ISnapshotableCollection<TValue>
    {
        private readonly Func<TValue, TKey> _keyAccessor;
        private HashSet<TValue> _snapshot;

        public SortedListAdapter(SortedList<TKey, TValue> list, Func<TValue, TKey> keyAccessor)
        {
            List = list;
            _keyAccessor = keyAccessor;
        }

        public SortedList<TKey, TValue> List { get; }
        public int Count => List.Count;
        public bool IsReadOnly => List.Values.IsReadOnly;
        public virtual HashSet<TValue> Snapshot => _snapshot;

        public virtual HashSet<TValue> CreateSnapshot()
        {
            if (_snapshot == null)
            {
                _snapshot = new HashSet<TValue>(List.Values);
            }
            else
            {
                _snapshot.Clear();
                _snapshot.AddRange(List.Values);
            }

            return _snapshot;
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => List.Values.GetEnumerator();
        public IEnumerator GetEnumerator() => List.Values.GetEnumerator();
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
}