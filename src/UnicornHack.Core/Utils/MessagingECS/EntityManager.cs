using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Utils.Caching;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Utils.MessagingECS
{
    public abstract class EntityManager<TEntity> : IEntityManager
        where TEntity : Entity, new()
    {
        private readonly DictionaryAdapter<int, TEntity> _entities =
            new DictionaryAdapter<int, TEntity>(new Dictionary<int, TEntity>(), e => e.Id);

        private readonly List<EntityGroup<TEntity>>[] _groupsByComponentId;
        private readonly ListObjectPool<TEntity> _entityPool;
        private readonly IObjectPool[] _componentPools;

        protected EntityManager(int componentCount, int entityPoolSize)
        {
            ComponentCount = componentCount;

            _entityPool =
                new ListObjectPool<TEntity>(() => new TEntity(), entityPoolSize, entityPoolSize, entityPoolSize);
            _componentPools = new IObjectPool[ComponentCount];

            _groupsByComponentId = new List<EntityGroup<TEntity>>[ComponentCount];
        }

        public int ComponentCount { get; }
        public IMessageQueue Queue { get; private set; }
        public bool IsLoading { get; set; }

        public EntityManager<TEntity> Initialize(IMessageQueue queue)
        {
            Debug.Assert(Queue == null);
            Debug.Assert(_entities.Count == 0);

            Queue = queue;
            InitializeSystems(queue);

            Debug.Assert(_componentPools.All(p => p != null));

            return this;
        }

        protected abstract void InitializeSystems(IMessageQueue queue);

        public void Enqueue<TMessage>(TMessage message, bool lowPriority = false)
            where TMessage : class, IMessage, new()
            => Queue.Enqueue(message, lowPriority);

        protected virtual void Add<TComponent>(int componentId, int poolSize)
            where TComponent : Component, new() => _componentPools[componentId] =
            new ListObjectPool<TComponent>(() => new TComponent(), poolSize, poolSize, poolSize);

        protected EntityGroup<TEntity> CreateGroup(string name, EntityMatcher<TEntity> matcher)
        {
            var group = new EntityGroup<TEntity>(name, matcher, this);

            foreach (var id in matcher.GetAllIds())
            {
                (_groupsByComponentId[id] ?? (_groupsByComponentId[id] = new List<EntityGroup<TEntity>>()))
                    .Add(group);
            }

            return group;
        }

        public abstract int GetNextEntityId();

        ITransientReference<Entity> IEntityManager.CreateEntity()
            => CreateEntity();

        public OwnerTransientReference<TEntity, EntityManager<TEntity>> CreateEntity()
            => new OwnerTransientReference<TEntity, EntityManager<TEntity>>(CreateEntityNoReference(), this);

        protected virtual TEntity CreateEntityNoReference()
        {
            var entity = _entityPool.Get();
            entity.Initialize(GetNextEntityId(), ComponentCount, this);
            _entities[entity.Id] = entity;
            return entity;
        }

        /// <summary>
        ///     <paramref name="entity"/> should be empty
        /// </summary>
        /// <param name="entity"></param>
        public void AddEntity(TEntity entity)
        {
            _entities[entity.Id] = entity;
            entity.Initialize(entity.Id, ComponentCount, this);
        }

        /// <summary>
        ///     Should only be called from Entity
        /// </summary>
        /// <param name="entity"></param>
        void IEntityManager.RemoveEntity(Entity entity)
        {
            Debug.Assert(_entities.Dictionary.ContainsKey(entity.Id));
            Debug.Assert(!entity.HasAnyComponent(Enumerable.Range(0, ComponentCount).ToArray()));

            _entities.Dictionary.Remove(entity.Id);
        }

        public IEnumerable<TEntity> GetEntities() => _entities;

        public TEntity FindEntity(int id) => _entities.Dictionary.TryGetValue(id, out var entity) ? entity : null;
        public virtual Entity LoadEntity(int id) => null;

        Entity IEntityManager.FindEntity(int id) => FindEntity(id);

        public TComponent CreateComponent<TComponent>(int componentId)
            where TComponent : Component, new() => ((ListObjectPool<TComponent>)_componentPools[componentId]).Get();

        public void HandleComponentAdded(int id, Component component)
            => HandleAddedOrRemoved(component.Entity, id, component);

        public void HandleComponentRemoved(int id, Component component)
            => HandleAddedOrRemoved(component.Entity, id, component);

        private void HandleAddedOrRemoved(Entity entity, int id, Component component)
        {
            var groups = _groupsByComponentId[id];
            if (groups != null)
            {
                var typedEntity = (TEntity)entity;

                foreach (var group in groups)
                {
                    group.HandleEntityComponentChanged(typedEntity, id, component);
                }
            }
        }

        public void HandlePropertyValueChanged<T>(
            string propertyName, T oldValue, T newValue, int componentId, Component component)
        {
            var groups = _groupsByComponentId[componentId];
            if (groups != null)
            {
                var typedEntity = (TEntity)component.Entity;

                foreach (var group in groups)
                {
                    group.HandlePropertyValueChanged(propertyName, oldValue, newValue, componentId, component,
                        typedEntity);
                }
            }
        }

        public virtual void RemoveFromSecondaryTracker(ITrackable trackable)
        {
        }
    }
}
