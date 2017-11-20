using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;

namespace UnicornHack.Utils
{
    public class ObservableSnapshotHashSet<T> : ObservableSnapshotHashSet,
        ISet<T>, IReadOnlyCollection<T>, ISnapshotableCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged, INotifyPropertyChanging
    {
        private HashSet<T> _set;
        private HashSet<T> _snapshot;

        public ObservableSnapshotHashSet()
            : this(EqualityComparer<T>.Default)
        {
        }

        public ObservableSnapshotHashSet([NotNull] IEqualityComparer<T> comparer)
            => _set = new HashSet<T>(comparer);

        public ObservableSnapshotHashSet([NotNull] IEnumerable<T> collection)
            : this(collection, EqualityComparer<T>.Default)
        {
        }

        public ObservableSnapshotHashSet([NotNull] IEnumerable<T> collection, [NotNull] IEqualityComparer<T> comparer)
            => _set = new HashSet<T>(collection, comparer);

        public virtual event PropertyChangedEventHandler PropertyChanged;
        public virtual event PropertyChangingEventHandler PropertyChanging;
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        public virtual int Count => _set.Count;
        public virtual bool IsReadOnly => ((ICollection<T>)_set).IsReadOnly;
        public virtual IEqualityComparer<T> Comparer => _set.Comparer;
        public virtual HashSet<T> Snapshot => _snapshot;

        public virtual HashSet<T> CreateSnapshot()
        {
            if (_snapshot == null)
            {
                _snapshot = new HashSet<T>(_set, Comparer);
            }
            else
            {
                _snapshot.Clear();
                _snapshot.AddRange(_set);
            }

            return _snapshot;
        }

        void ICollection<T>.Add(T item) => Add(item);

        public virtual bool Add(T item)
        {
            if (_set.Contains(item))
            {
                return false;
            }

            OnCountPropertyChanging();

            _set.Add(item);

            OnCollectionChanged(NotifyCollectionChangedAction.Add, item);

            OnCountPropertyChanged();

            return true;
        }

        public virtual void Clear()
        {
            if (_set.Count == 0)
            {
                return;
            }

            OnCountPropertyChanging();

            var removed = this.ToList();

            _set.Clear();

            OnCollectionChanged(Empty, removed);

            OnCountPropertyChanged();
        }

        public virtual bool Contains(T item) => _set.Contains(item);
        public virtual void CopyTo(T[] array, int arrayIndex) => _set.CopyTo(array, arrayIndex);
        public virtual void CopyTo([NotNull] T[] array) => _set.CopyTo(array);

        public virtual void CopyTo([NotNull] T[] array, int arrayIndex, int count)
            => _set.CopyTo(array, arrayIndex, count);

        public virtual bool Remove(T item)
        {
            if (!_set.Contains(item))
            {
                return false;
            }

            OnCountPropertyChanging();

            _set.Remove(item);

            OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);

            OnCountPropertyChanged();

            return true;
        }

        public virtual int RemoveWhere([NotNull] Predicate<T> match)
        {
            var copy = new HashSet<T>(_set, _set.Comparer);

            var removedCount = copy.RemoveWhere(match);

            if (removedCount == 0)
            {
                return 0;
            }

            var removed = _set.Where(i => !copy.Contains(i)).ToList();

            OnCountPropertyChanging();

            _set = copy;

            OnCollectionChanged(Empty, removed);

            OnCountPropertyChanged();

            return removedCount;
        }

        public virtual HashSet<T>.Enumerator GetEnumerator() => _set.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public virtual void ExceptWith(IEnumerable<T> other)
        {
            var copy = new HashSet<T>(_set, _set.Comparer);

            copy.ExceptWith(other);

            if (copy.Count == _set.Count)
            {
                return;
            }

            var removed = _set.Where(i => !copy.Contains(i)).ToList();

            OnCountPropertyChanging();

            _set = copy;

            OnCollectionChanged(Empty, removed);

            OnCountPropertyChanged();
        }

        public virtual void SymmetricExceptWith(IEnumerable<T> other)
        {
            var copy = new HashSet<T>(_set, _set.Comparer);

            copy.SymmetricExceptWith(other);

            var removed = _set.Where(i => !copy.Contains(i)).ToList();
            var added = copy.Where(i => !_set.Contains(i)).ToList();

            if (removed.Count == 0
                && added.Count == 0)
            {
                return;
            }

            OnCountPropertyChanging();

            _set = copy;

            OnCollectionChanged(added, removed);

            OnCountPropertyChanged();
        }

        public virtual void IntersectWith(IEnumerable<T> other)
        {
            var copy = new HashSet<T>(_set, _set.Comparer);

            copy.IntersectWith(other);

            if (copy.Count == _set.Count)
            {
                return;
            }

            var removed = _set.Where(i => !copy.Contains(i)).ToList();

            OnCountPropertyChanging();

            _set = copy;

            OnCollectionChanged(Empty, removed);

            OnCountPropertyChanged();
        }

        public virtual void UnionWith(IEnumerable<T> other)
        {
            var copy = new HashSet<T>(_set, _set.Comparer);

            copy.UnionWith(other);

            if (copy.Count == _set.Count)
            {
                return;
            }

            var added = copy.Where(i => !_set.Contains(i)).ToList();

            OnCountPropertyChanging();

            _set = copy;

            OnCollectionChanged(added, Empty);

            OnCountPropertyChanged();
        }

        public virtual bool IsSubsetOf(IEnumerable<T> other) => _set.IsSubsetOf(other);
        public virtual bool IsProperSubsetOf(IEnumerable<T> other) => _set.IsProperSubsetOf(other);
        public virtual bool IsSupersetOf(IEnumerable<T> other) => _set.IsSupersetOf(other);
        public virtual bool IsProperSupersetOf(IEnumerable<T> other) => _set.IsProperSupersetOf(other);
        public virtual bool Overlaps(IEnumerable<T> other) => _set.Overlaps(other);
        public virtual bool SetEquals(IEnumerable<T> other) => _set.SetEquals(other);
        public virtual void TrimExcess() => _set.TrimExcess();

        protected virtual void OnPropertyChanged([NotNull] PropertyChangedEventArgs e)
            => PropertyChanged?.Invoke(this, e);

        protected virtual void OnPropertyChanging([NotNull] PropertyChangingEventArgs e)
            => PropertyChanging?.Invoke(this, e);

        private void OnCountPropertyChanged() => OnPropertyChanged(NotificationEntity.CountPropertyChanged);

        private void OnCountPropertyChanging() => OnPropertyChanging(NotificationEntity.CountPropertyChanging);

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item)
            => OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item));

        private void OnCollectionChanged(IList newItems, IList oldItems)
            => OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems));

        /// <summary>
        ///     Raises the <see cref="CollectionChanged" /> event.
        /// </summary>
        /// <param name="e"> Details of the change. </param>
        protected virtual void OnCollectionChanged([NotNull] NotifyCollectionChangedEventArgs e)
            => CollectionChanged?.Invoke(this, e);
    }

    public class ObservableSnapshotHashSet
    {
        protected static readonly object[] Empty = new object[0];
    }
}