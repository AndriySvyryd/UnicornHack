namespace UnicornHack.Utils.MessagingECS;

public abstract class DependentEntityGroupBase<TEntity> : EntityGroupBase<TEntity>
    where TEntity : Entity, new()
{
    private List<IDependentEntityChangeListener<TEntity>>? _dependentChangeListeners;

    protected DependentEntityGroupBase(string name)
        : base(name)
    {
    }

    public override bool HasPropertyValueSubscribers
        => base.HasPropertyValueSubscribers || _dependentChangeListeners is { Count: > 0 };

    public void AddDependentListener(IDependentEntityChangeListener<TEntity> listener)
    {
        _dependentChangeListeners ??= [];
        _dependentChangeListeners.Add(listener);
    }

    public void RemoveDependentListener(IDependentEntityChangeListener<TEntity> listener)
    {
        _dependentChangeListeners?.Remove(listener);
    }

    protected void OnDependentAdded(in EntityChange<TEntity> entityChange, TEntity principal)
    {
        OnAdded(entityChange, principal);

        if (_dependentChangeListeners != null)
        {
            foreach (var listener in _dependentChangeListeners)
            {
                listener.OnEntityAdded(entityChange, principal);
            }
        }
    }

    protected void OnDependentRemoved(in EntityChange<TEntity> entityChange, TEntity principal)
    {
        OnRemoved(entityChange, principal);

        if (_dependentChangeListeners != null)
        {
            foreach (var listener in _dependentChangeListeners)
            {
                listener.OnEntityRemoved(entityChange, principal);
            }
        }
    }

    protected void OnDependentPropertyValuesChanged(in EntityChange<TEntity> entityChange, TEntity principal)
    {
        OnPropertyValuesChanged(entityChange);

        if (_dependentChangeListeners != null)
        {
            foreach (var listener in _dependentChangeListeners)
            {
                listener.OnPropertyValuesChanged(entityChange, principal);
            }
        }
    }
}
