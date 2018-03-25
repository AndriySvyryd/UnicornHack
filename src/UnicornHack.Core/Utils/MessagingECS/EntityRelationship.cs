using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UnicornHack.Utils.MessagingECS
{
    public class EntityRelationship<TEntity> : EntityRelationshipBase<TEntity>
        where TEntity : Entity
    {
        public EntityRelationship(string name,
            IEntityGroup<TEntity> referencingGroup,
            IEntityGroup<TEntity> referencedGroup,
            KeyValueGetter<TEntity, int> keyValueGetter,
            Action<TEntity, TEntity, int, Component> handleReferencedDeleted,
            bool referencedKeepAlive = false,
            bool referencingKeepAlive = false)
            : base(
                name,
                referencingGroup,
                referencedGroup,
                keyValueGetter,
                handleReferencedDeleted,
                referencedKeepAlive,
                referencingKeepAlive) => referencedGroup.AddListener(new ReferencedGroupListener(this));

        protected Dictionary<int, HashSet<TEntity>> Index { get; } = new Dictionary<int, HashSet<TEntity>>();

        protected Dictionary<int, HashSet<TEntity>> OrphanedEntities { get; private set; }

        public IReadOnlyCollection<TEntity> this[int key]
            => Index.TryGetValue(key, out var entities)
                ? entities
                : (IReadOnlyCollection<TEntity>)Array.Empty<TEntity>();

        public override int Count => Index.Count;

        private HashSet<TEntity> GetOrAddEntities(int key, Dictionary<int, HashSet<TEntity>> index)
        {
            if (!index.TryGetValue(key, out var entities))
            {
                entities = new HashSet<TEntity>(EntityEqualityComparer<TEntity>.Instance);
                index.Add(key, entities);
            }

            return entities;
        }

        protected override bool TryAddEntity(
            int key, TEntity entity, int changedComponentId, Component changedComponent)
        {
            var referenced = FindReferenced(key, entity, fallback: false);
            if (referenced != null)
            {
                return AddEntity(key, entity, changedComponentId, changedComponent, referenced);
            }

            if (OrphanedEntities == null)
            {
                OrphanedEntities = new Dictionary<int, HashSet<TEntity>>();
            }

            var added = GetOrAddEntities(key, OrphanedEntities).Add(entity);
            Debug.Assert(added);

            return false;
        }

        private bool AddEntity(
            int key, TEntity entity, int changedComponentId, Component changedComponent, TEntity referenced)
        {
            var added = GetOrAddEntities(key, Index).Add(entity);
            Debug.Assert(added);

            OnEntityAdded(entity, changedComponentId, changedComponent, referenced);
            return true;
        }

        protected override bool TryRemoveEntity(
            int key, TEntity entity, int changedComponentId, Component changedComponent)
        {
            if (GetOrAddEntities(key, Index).Remove(entity))
            {
                OnEntityRemoved(entity, changedComponentId, changedComponent,
                    FindReferenced(key, entity, fallback: true));
                return true;
            }

            if (OrphanedEntities != null)
            {
                GetOrAddEntities(key, OrphanedEntities).Remove(entity);
            }

            return false;
        }

        public override IEnumerator<TEntity> GetEnumerator()
            => Index.Values.SelectMany(v => v).GetEnumerator();

        private class ReferencedGroupListener : IGroupChangesListener<TEntity>
        {
            private readonly EntityRelationship<TEntity> _relationship;

            public ReferencedGroupListener(EntityRelationship<TEntity> relationship)
                => _relationship = relationship;

            public void HandleEntityAdded
                (TEntity entity, int addedComponentId, Component addedComponent, IEntityGroup<TEntity> group)
            {
                if (_relationship.OrphanedEntities != null
                    && _relationship.OrphanedEntities.TryGetValue(entity.Id, out var orphanedEntities))
                {
                    _relationship.OrphanedEntities.Remove(entity.Id);

                    foreach (var orphanedEntity in orphanedEntities)
                    {
                        _relationship.AddEntity(
                            entity.Id, orphanedEntity, Component.NullId, changedComponent: null, entity);
                    }
                }
            }

            public void HandleEntityRemoved(
                TEntity entity, int removedComponentId, Component removedComponent, IEntityGroup<TEntity> group)
            {
                foreach (var referencingEntity in _relationship[entity.Id].ToList())
                {
                    _relationship.HandleReferencedEntityRemoved(
                        referencingEntity, entity, removedComponentId, removedComponent);
                }

                if (_relationship.OrphanedEntities != null
                    && _relationship.OrphanedEntities.ContainsKey(entity.Id))
                {
                    _relationship.OrphanedEntities.Remove(entity.Id);
                }
            }

            public bool HandlePropertyValueChanged<T>(string propertyName, T oldValue, T newValue,
                int componentId, Component component, TEntity entity, IEntityGroup<TEntity> group)
                => false;
        }
    }
}
