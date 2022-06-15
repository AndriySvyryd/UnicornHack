using System;
using System.Linq.Expressions;

namespace UnicornHack.Utils.MessagingECS;

public class PropertyReferenceAccessor<TEntity> : IReferenceAccessor<TEntity>
    where TEntity : Entity, new()
{
    private readonly Func<TEntity, TEntity> _getDependent;
    private readonly Func<Component, TEntity> _componentGetDependent;
    private readonly Action<TEntity, TEntity> _setDependent;
    private readonly Func<TEntity, TEntity> _getPrincipal;
    private readonly Func<Component, TEntity> _componentGetPrincipal;
    private readonly Action<TEntity, TEntity> _setPrincipal;

    public PropertyReferenceAccessor(
        Expression<Func<TEntity, TEntity>> getDependent,
        Expression<Func<TEntity, TEntity>> getPrincipal = null)
    {
        var (get, componentGet, set) = getDependent.GetPropertyAccessors();
        _getDependent = (Func<TEntity, TEntity>)get;
        _componentGetDependent = (Func<Component, TEntity>)componentGet;
        _setDependent = (Action<TEntity, TEntity>)set;

        if (getPrincipal != null)
        {
            (get, componentGet, set) = getPrincipal.GetPropertyAccessors();
            _getPrincipal = (Func<TEntity, TEntity>)get;
            _componentGetPrincipal = (Func<Component, TEntity>)componentGet;
            _setPrincipal = (Action<TEntity, TEntity>)set;
        }
    }

    public PropertyReferenceAccessor(
        Func<TEntity, TEntity> getDependent,
        Action<TEntity, TEntity> setDependent,
        Func<Component, TEntity> componentGetDependent,
        Func<TEntity, TEntity> getPrincipal = null,
        Action<TEntity, TEntity> setPrincipal = null,
        Func<Component, TEntity> componentGetPrincipal = null)
    {
        _getDependent = getDependent;
        _setDependent = setDependent;
        _componentGetDependent = componentGetDependent;
        _getPrincipal = getPrincipal;
        _setPrincipal = setPrincipal;
        _componentGetPrincipal = componentGetPrincipal;
    }

    public TEntity GetDependent(TEntity principal, Component removedComponent = null)
        => (removedComponent != null ? _componentGetDependent(removedComponent) : null)
           ?? _getDependent(principal);

    public void SetDependent(TEntity principal, TEntity dependent)
        => _setDependent(principal, dependent);

    public bool TryGetPrincipal(TEntity dependent, out TEntity principal, Component removedComponent = null)
    {
        if (_getPrincipal == null)
        {
            principal = null;
            return false;
        }

        if (removedComponent != null)
        {
            principal = _componentGetPrincipal(removedComponent);
            if (principal != null)
            {
                return true;
            }
        }

        principal = _getPrincipal(dependent);
        return true;
    }

    public void SetPrincipal(TEntity dependent, TEntity principal)
        => _setPrincipal?.Invoke(dependent, principal);
}
