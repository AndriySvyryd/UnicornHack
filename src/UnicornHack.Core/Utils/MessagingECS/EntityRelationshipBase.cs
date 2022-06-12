using System;

namespace UnicornHack.Utils.MessagingECS
{
    public abstract class EntityRelationshipBase<TEntity> : EntityIndexBase<TEntity, int>, IEntityRelationshipBase<TEntity> where TEntity : Entity, new()
    {
        private readonly Action<TEntity, EntityChange<TEntity>> _handlePrincipalDeleted;
        private readonly bool _keepPrincipalAlive;
        private readonly bool _keepDependentAlive;

        protected EntityRelationshipBase(
            string name,
            IEntityGroup<TEntity> dependentGroup,
            IEntityGroup<TEntity> principalGroup,
            IKeyValueGetter<TEntity, int> keyValueGetter,
            Action<TEntity, EntityChange<TEntity>> handlePrincipalDeleted,
            bool keepPrincipalAlive,
            bool keepDependentAlive)
            : base(name, dependentGroup, keyValueGetter)
        {
            DependentGroup = dependentGroup;
            PrincipalGroup = principalGroup;
            _handlePrincipalDeleted = handlePrincipalDeleted;
            _keepPrincipalAlive = keepPrincipalAlive;
            _keepDependentAlive = keepDependentAlive;
        }

        protected IEntityGroup<TEntity> PrincipalGroup { get; }
        protected IEntityGroup<TEntity> DependentGroup { get; }

        protected TEntity FindPrincipal(int key, TEntity entity, bool fallback)
        {
            var principal = PrincipalGroup.FindEntity(key);
            if (principal != null)
            {
                return principal;
            }

            var manager = entity.Manager;
            if (fallback
                && manager.FindEntity(key) == null)
            {
                // TODO: If loaded entity is to be deleted load all components as well to delete recursively
                manager.LoadEntity(key);
                principal = PrincipalGroup.FindEntity(key);
            }

            if (principal == null
                && !manager.IsLoading
                && !fallback)
            {
                throw new InvalidOperationException(
                    $"Couldn't find entity '{key}' in '{PrincipalGroup.Name}' referenced from entity '{entity.Id}'"
                    + $" in '{DependentGroup.Name}' through '{Name}'");
            }

            return principal;
        }

        protected void HandlePrincipalEntityRemoved(TEntity dependentEntity, EntityChange<TEntity> principalEntityChange)
            => _handlePrincipalDeleted(dependentEntity, principalEntityChange);

        void IEntityRelationshipBase<TEntity>.OnEntityAdded(TEntity dependent, TEntity principal)
        {
            if (_keepPrincipalAlive)
            {
                principal.AddReference(dependent);
            }

            if (_keepDependentAlive)
            {
                dependent.AddReference(principal);
            }
        }

        void IEntityRelationshipBase<TEntity>.OnEntityRemoved(TEntity dependent, TEntity principal)
        {
            if (principal?.Manager != null
                && _keepPrincipalAlive)
            {
                principal.RemoveReference(dependent);
            }

            if (_keepDependentAlive)
            {
                dependent.RemoveReference(principal);
            }
        }

        /// <summary>
        ///     Tries to find a referencing entity with the given <paramref name="id" />.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TEntity IEntityRelationshipBase<TEntity>.FindEntity(int id)
        {
            var entity = DependentGroup.FindEntity(id);
            if (entity != null && ContainsEntity(entity))
            {
                return entity;
            }

            return null;
        }

        /// <summary>
        ///     Returns <c>true</c> if there is a referencing entity with the given <paramref name="id" />.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool IEntityRelationshipBase<TEntity>.ContainsEntity(int id)
        {
            var entity = DependentGroup.FindEntity(id);
            return entity != null && ContainsEntity(entity);
        }

        private bool ContainsEntity(TEntity entity)
            => KeyValueGetter.TryGetKey(new EntityChange<TEntity>(entity), ValueType.Current, out _)
               && !entity.Manager.IsLoading;
    }
}
