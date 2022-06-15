using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace UnicornHack.Utils.DataStructures;

public class ObservableHashSet<T> :
    ObservableHashSet, ISet<T>, IReadOnlyCollection<T>,
    INotifyCollectionChanged, INotifyPropertyChanged, INotifyPropertyChanging
{
    public ObservableHashSet()
        : this(EqualityComparer<T>.Default)
    {
    }

    public ObservableHashSet(IEqualityComparer<T> comparer)
    {
        Set = new HashSet<T>(comparer);
    }

    public ObservableHashSet(IEnumerable<T> collection)
        : this(collection, EqualityComparer<T>.Default)
    {
    }

    public ObservableHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
    {
        Set = new HashSet<T>(collection, comparer);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public event PropertyChangingEventHandler PropertyChanging;
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    public int Count => Set.Count;
    public bool IsReadOnly => ((ICollection<T>)Set).IsReadOnly;
    public IEqualityComparer<T> Comparer => Set.Comparer;

    protected HashSet<T> Set
    {
        get;
        private set;
    }

    void ICollection<T>.Add(T item) => Add(item);

    public bool Add(T item)
    {
        if (Set.Contains(item))
        {
            return false;
        }

        OnCountPropertyChanging();

        Set.Add(item);

        OnCollectionChanged(NotifyCollectionChangedAction.Add, item);

        OnCountPropertyChanged();

        return true;
    }

    public void Clear()
    {
        if (Set.Count == 0)
        {
            return;
        }

        OnCountPropertyChanging();

        var removed = this.ToList();

        Set.Clear();

        OnCollectionChanged(Empty, removed);

        OnCountPropertyChanged();
    }

    public bool Contains(T item) => Set.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => Set.CopyTo(array, arrayIndex);
    public void CopyTo(T[] array) => Set.CopyTo(array);

    public void CopyTo(T[] array, int arrayIndex, int count)
        => Set.CopyTo(array, arrayIndex, count);

    public bool Remove(T item)
    {
        if (!Set.Contains(item))
        {
            return false;
        }

        OnCountPropertyChanging();

        Set.Remove(item);

        OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);

        OnCountPropertyChanged();

        return true;
    }

    public int RemoveWhere(Predicate<T> match)
    {
        var copy = new HashSet<T>(Set, Set.Comparer);

        var removedCount = copy.RemoveWhere(match);

        if (removedCount == 0)
        {
            return 0;
        }

        var removed = Set.Where(i => !copy.Contains(i)).ToList();

        OnCountPropertyChanging();

        Set = copy;

        OnCollectionChanged(Empty, removed);

        OnCountPropertyChanged();

        return removedCount;
    }

    public HashSet<T>.Enumerator GetEnumerator() => Set.GetEnumerator();
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void ExceptWith(IEnumerable<T> other)
    {
        var copy = new HashSet<T>(Set, Set.Comparer);

        copy.ExceptWith(other);

        if (copy.Count == Set.Count)
        {
            return;
        }

        var removed = Set.Where(i => !copy.Contains(i)).ToList();

        OnCountPropertyChanging();

        Set = copy;

        OnCollectionChanged(Empty, removed);

        OnCountPropertyChanged();
    }

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        var copy = new HashSet<T>(Set, Set.Comparer);

        copy.SymmetricExceptWith(other);

        var removed = Set.Where(i => !copy.Contains(i)).ToList();
        var added = copy.Where(i => !Set.Contains(i)).ToList();

        if (removed.Count == 0
            && added.Count == 0)
        {
            return;
        }

        OnCountPropertyChanging();

        Set = copy;

        OnCollectionChanged(added, removed);

        OnCountPropertyChanged();
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        var copy = new HashSet<T>(Set, Set.Comparer);

        copy.IntersectWith(other);

        if (copy.Count == Set.Count)
        {
            return;
        }

        var removed = Set.Where(i => !copy.Contains(i)).ToList();

        OnCountPropertyChanging();

        Set = copy;

        OnCollectionChanged(Empty, removed);

        OnCountPropertyChanged();
    }

    public void UnionWith(IEnumerable<T> other)
    {
        var copy = new HashSet<T>(Set, Set.Comparer);

        copy.UnionWith(other);

        if (copy.Count == Set.Count)
        {
            return;
        }

        var added = copy.Where(i => !Set.Contains(i)).ToList();

        OnCountPropertyChanging();

        Set = copy;

        OnCollectionChanged(added, Empty);

        OnCountPropertyChanged();
    }

    public bool IsSubsetOf(IEnumerable<T> other) => Set.IsSubsetOf(other);
    public bool IsProperSubsetOf(IEnumerable<T> other) => Set.IsProperSubsetOf(other);
    public bool IsSupersetOf(IEnumerable<T> other) => Set.IsSupersetOf(other);
    public bool IsProperSupersetOf(IEnumerable<T> other) => Set.IsProperSupersetOf(other);
    public bool Overlaps(IEnumerable<T> other) => Set.Overlaps(other);
    public bool SetEquals(IEnumerable<T> other) => Set.SetEquals(other);
    public void TrimExcess() => Set.TrimExcess();

    protected void OnPropertyChanged(PropertyChangedEventArgs e)
        => PropertyChanged?.Invoke(this, e);

    protected void OnPropertyChanging(PropertyChangingEventArgs e)
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
    protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        => CollectionChanged?.Invoke(this, e);
}

public class ObservableHashSet
{
    protected static readonly object[] Empty = new object[0];
}
