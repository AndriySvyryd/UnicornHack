using System;
using System.Collections.Generic;

namespace UnicornHack.Utils.MessagingECS
{
    public class ExternalCollectionAccessor<TEntity, TCollection, TElement> : ICollectionAccessor<TEntity, TCollection, TElement>
        where TEntity : Entity, new()
        where TCollection : class, ICollection<TElement>
    {
        private Func<TCollection> _factory;

        protected Dictionary<int, TCollection> Index { get; } = new();

        public TCollection GetDependents(TEntity principal, Component removedComponent = null)
            => Index.TryGetValue(principal.Id, out var entities)
                ? entities
                : null;

        public TCollection GetOrCreateDependents(TEntity principal)
        {
            var key = principal.Id;
            if (Index.TryGetValue(key, out var value))
            {
                return value;
            }

            value = _factory();
            Index[key] = value;
            return value;
        }

        public void ResetDependents(TEntity principal)
            => Index.Remove(principal.Id);

        public void SetPrincipal(TEntity dependent, TEntity principal)
        {
        }

        public bool TryGetPrincipal(TEntity dependent, out TEntity principal, Component removedComponent = null)
        {
            principal = null;
            return false;
        }

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
