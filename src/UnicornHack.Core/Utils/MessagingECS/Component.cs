using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnicornHack.Utils.Caching;

namespace UnicornHack.Utils.MessagingECS;

public abstract class Component :
    IReferenceable, ITrackable, IPoolable, INotifyPropertyChanged, INotifyPropertyChanging
{
    public static int NullId = int.MinValue;
    public event PropertyChangedEventHandler? PropertyChanged;
    public event PropertyChangingEventHandler? PropertyChanging;

#if DEBUG
    private readonly List<object> _owners = new();
    private object? _tracker;
#endif
    private int _referenceCount;
    private IObjectPool? _pool;
    private bool _tracked;
    private Entity? _entity;

    public int ComponentId
    {
        get;
        protected set;
    }

    public Entity Entity
    {
        get => _entity!;
        set
        {
            if (_entity != value)
            {
                Debug.Assert(_entity == null || value == null!);

                if (value?.Manager != null)
                {
                    // Done here because reference count is not persisted
                    ((IReferenceable)this).AddReference(value);

                    Manager = value.Manager;
                }

                _entity = value;
            }
        }
    }

    protected internal IEntityManager? Manager
    {
        get;
        set;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void SetWithNotify<T>(
        T? value,
        [NotNullIfNotNull("value")] ref T? field,
        [CallerMemberName] string propertyName = "")
    {
        if (NotifyChanging(value, ref field, propertyName, out var oldValue))
        {
            NotifyChanged(propertyName);
            Entity?.Manager.OnPropertyValueChanged(propertyName, oldValue, value, ComponentId, this);
        }
    }

    protected void OnCollectionChanged<TItem>(
        NotifyCollectionChangedEventArgs args,
        string propertyName)
        where TItem : NotificationCollectionItem
    {
        if (args.Action is NotifyCollectionChangedAction.Reset or NotifyCollectionChangedAction.Move)
        {
            return;
        }

        if (args.OldItems != null)
        {
            foreach (TItem item in args.OldItems)
            {
                item.SetContainingComponent(null, null);
                RaiseCollectionChanged(
                    propertyName,
                    new EntityCollectionChange(EntityCollectionChangeAction.Remove, item),
                    default);
            }
        }

        if (args.NewItems != null)
        {
            foreach (TItem item in args.NewItems)
            {
                Debug.Assert(item.ContainingComponent == this
                             && item.ContainingPropertyName == propertyName);
                RaiseCollectionChanged(
                    propertyName,
                    default,
                    new EntityCollectionChange(EntityCollectionChangeAction.Add, item));
            }
        }
    }

    internal void OnNestedPropertyValueChanged<T>(
        string propertyName, string nestedPropertyName, object item, T oldValue, T newValue)
    {
        Entity?.Manager.OnNestedPropertyValueChanged(
            this, propertyName, nestedPropertyName, item, oldValue, newValue);
    }

    private void RaiseCollectionChanged(
        string propertyName,
        in EntityCollectionChange oldValue,
        in EntityCollectionChange newValue)
        => Entity?.Manager.OnCollectionChanged(this, propertyName, oldValue, newValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool NotifyChanging<T>(
        T? value,
        ref T? field,
        string propertyName,
        out T? oldValue)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            oldValue = default;
            return false;
        }

        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        oldValue = field;
        field = value;

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void NotifyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    void ITrackable.StartTracking(object tracker)
    {
        // Tracking is idempotent for the same tracker (an ORM may re-raise tracking for a component it already
        // tracks, e.g. split-query fixup re-materialising the same instance); only a second, different tracker
        // taking over an already-tracked component is a double-tracking bug.
#if DEBUG
        Debug.Assert(!_tracked || ReferenceEquals(_tracker, tracker),
            $"Component {GetType().Name} is already tracked by a different tracker");
        _tracker = tracker;
#endif
        _tracked = true;
    }

    void ITrackable.StopTracking(object tracker)
    {
        Debug.Assert(_tracked, $"Component {GetType().Name} is not tracked by {tracker}");

        _tracked = false;
#if DEBUG
        _tracker = null;
#endif
        ((IReferenceable)this).AddReference(tracker);
        ((IReferenceable)this).RemoveReference(tracker);
    }

    void IReferenceable.AddReference(object owner)
    {
#if DEBUG
        _owners.Add(owner);
#endif
        _referenceCount++;
    }

    void IReferenceable.RemoveReference(object owner)
    {
#if DEBUG
        _owners.Remove(owner);
#endif
        if (--_referenceCount > 0)
        {
            return;
        }

        if (_referenceCount == 0)
        {
            if (_tracked)
            {
                Manager!.RemoveFromSecondaryTracker(this);
            }
            else
            {
                Clean();
            }
        }
        else
        {
            throw new InvalidOperationException($"Component {ComponentId} is not referenced by object {owner}");
        }
    }

    protected virtual void Clean()
    {
        Debug.Assert(Entity?.FindComponent(ComponentId) != this
                     && _referenceCount == 0
                     && !_tracked
                     && PropertyChanged == null
                     && PropertyChanging == null);

        Entity = default!;

        _pool?.Return(this);
    }

    void IPoolable.SetPool(IObjectPool pool)
        => _pool = pool;
}
