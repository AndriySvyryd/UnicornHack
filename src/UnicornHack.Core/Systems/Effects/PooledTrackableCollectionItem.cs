using UnicornHack.Utils;
using UnicornHack.Utils.Caching;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Effects;

public abstract class PooledTrackableCollectionItem : NotificationCollectionItem, ITrackable, IPoolable
{
    private IObjectPool? _pool;
    private bool _tracked;
    private bool _retired;
#if DEBUG
    private object? _tracker;
#endif

    protected bool IsTracked => _tracked;

    internal void Attach(ActiveEffectsComponent owner)
    {
        if (ContainingComponent != null && !ReferenceEquals(ContainingComponent, owner))
        {
            throw new InvalidOperationException(
                $"{GetType().Name} already belongs to a component.");
        }

        Debug.Assert(_pool != null, $"{GetType().Name} is not assigned to an object pool.");
    }

    internal void Detach(GameManager manager)
    {
        if (_retired)
        {
            throw new InvalidOperationException(
                $"{GetType().Name} is already awaiting retirement.");
        }

        if (_pool == null)
        {
            throw new InvalidOperationException($"{GetType().Name} is not assigned to an object pool.");
        }

        _retired = true;
        manager.AddToReleaseQueue(this);
    }

    internal void Release(IEntityManager manager)
    {
        if (!_retired)
        {
            throw new InvalidOperationException($"{GetType().Name} was not retired.");
        }

        if (ContainingComponent != null)
        {
            throw new InvalidOperationException("A retired collection item cannot still belong to a component.");
        }

        _retired = false;
        if (_tracked)
        {
            manager.RemoveFromSecondaryTracker(this);
        }
        else
        {
            Clean();
        }
    }

    void ITrackable.StartTracking(object tracker)
    {
        if (_tracked)
        {
#if DEBUG
            if (!ReferenceEquals(_tracker, tracker))
            {
                throw new InvalidOperationException(
                    $"{GetType().Name} is already tracked by a different tracker.");
            }
#endif
            return;
        }

        _tracked = true;
#if DEBUG
        _tracker = tracker;
#endif
    }

    void ITrackable.StopTracking(object tracker)
    {
        if (!_tracked)
        {
            throw new InvalidOperationException($"{GetType().Name} is not tracked by {tracker}.");
        }

        _tracked = false;
#if DEBUG
        _tracker = null;
#endif
        if (ContainingComponent == null && !_retired)
        {
            Clean();
        }
    }

    void IPoolable.SetPool(IObjectPool pool)
    {
        if (_pool != null && !ReferenceEquals(_pool, pool))
        {
            throw new InvalidOperationException($"{GetType().Name} cannot move between object pools.");
        }

        _pool = pool;
    }

    private void Clean()
    {
        Debug.Assert(!_tracked && ContainingComponent == null);

        Reset();
        ContainingPropertyName = null;
        ClearNotificationHandlers();
        (_pool ?? throw new InvalidOperationException(
            $"{GetType().Name} was not created by or attached to an object pool."))
            .Return(this);
    }

    protected abstract void Reset();
}