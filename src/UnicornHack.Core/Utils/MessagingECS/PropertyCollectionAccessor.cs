using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace UnicornHack.Utils.MessagingECS
{
    public class PropertyCollectionAccessor<TEntity, TCollection, TElement> : ICollectionAccessor<TEntity, TCollection, TElement>
        where TEntity : Entity, new()
        where TCollection : class, ICollection<TElement>
    {
        private readonly Func<TEntity, TCollection> _getDependent;
        private readonly Func<Component, TCollection> _componentGetDependent;
        private readonly Action<TEntity, TCollection> _setDependent;
        private readonly Func<TEntity, TEntity> _getPrincipal;
        private readonly Func<Component, TEntity> _componentGetPrincipal;
        private readonly Action<TEntity, TEntity> _setPrincipal;
        private Func<TCollection> _factory;

        public PropertyCollectionAccessor(
            Expression<Func<TEntity, TCollection>> getDependent,
            Expression<Func<TEntity, TEntity>> getPrincipal = null)
        {
            var (get, componentGet, set) = getDependent.GetPropertyAccessors();
            _getDependent = (Func<TEntity, TCollection>)get;
            _componentGetDependent = (Func<Component, TCollection>)componentGet;
            _setDependent = (Action<TEntity, TCollection>)set;

            if (getPrincipal != null)
            {
                (get, componentGet, set) = getPrincipal.GetPropertyAccessors();
                _getPrincipal = (Func<TEntity, TEntity>)get;
                _componentGetPrincipal = (Func<Component, TEntity>)componentGet;
                _setPrincipal = (Action<TEntity, TEntity>)set;
            }
        }

        public PropertyCollectionAccessor(
            Func<TEntity, TCollection> getDependent,
            Action<TEntity, TCollection> setDependent,
            Func<Component, TCollection> componentGetDependent,
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

        public TCollection GetDependents(TEntity principal, Component removedComponent = null)
            => (removedComponent != null ? _componentGetDependent(removedComponent) : null)
               ?? _getDependent(principal);

        public TCollection GetOrCreateDependents(TEntity principal)
        {
            var value = GetDependents(principal);
            if (value != null)
            {
                return value;
            }

            if (_setDependent == null)
            {
                throw new InvalidOperationException("null collection and no setter");
            }

            value = _factory();

            _setDependent(principal, value);
            return value;
        }

        public void ResetDependents(TEntity principal)
            => GetDependents(principal)?.Clear();

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

            principal = _getPrincipal.Invoke(dependent);
            return true;
        }

        public void SetPrincipal(TEntity dependent, TEntity principal)
            => _setPrincipal?.Invoke(dependent, principal);

        public void SetDefaultFactory(Func<TCollection> factory)
        {
            if (_factory != null)
            {
                throw new InvalidOperationException("Factory already set on this accessor");
            }

            _factory = factory;
        }
    }
}
