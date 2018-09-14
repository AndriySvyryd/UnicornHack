using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UnicornHack.Utils.MessagingECS
{
    public class EntityRelationship<TEntity> : EntityRelationshipBase<TEntity>
        where TEntity : Entity, new()
    {
        public EntityRelationship(string name,
            IEntityGroup<TEntity> referencingGroup,
            IEntityGroup<TEntity> referencedGroup,
            IKeyValueGetter<TEntity, int> keyValueGetter,
            Action<TEntity, TEntity, Component> handleReferencedDeleted,
            bool referencedKeepAlive = false,
            bool referencingKeepAlive = false)
            : base(
                name,
                referencingGroup,
                referencedGroup,
                keyValueGetter,
                handleReferencedDeleted,
                referencedKeepAlive,
                referencingKeepAlive)
            => referencedGroup.AddListener(new ReferencedGroupListener(this));

        protected Dictionary<int, HashSet<TEntity>> Index { get; } = new Dictionary<int, HashSet<TEntity>>();

        protected Dictionary<int, HashSet<TEntity>> OrphanedEntities { get; private set; }

        public IReadOnlyCollection<TEntity> this[int key]
            => Index.TryGetValue(key, out var entities)
                ? entities
                : (IReadOnlyCollection<TEntity>)Array.Empty<TEntity>();

        public override int Count => Index.Values.Sum(v => v.Count);

        private HashSet<TEntity> GetOrAddEntities(int key, Dictionary<int, HashSet<TEntity>> index)
        {
            if (!index.TryGetValue(key, out var entities))
            {
                entities = new HashSet<TEntity>(EntityEqualityComparer<TEntity>.Instance);
                index.Add(key, entities);
            }

            return entities;
        }

        protected override bool TryAddEntity(int key, TEntity entity, Component changedComponent)
        {
            var referenced = FindReferenced(key, entity, fallback: false);
            if (referenced != null)
            {
                return AddEntity(key, entity, changedComponent, referenced);
            }

            if (OrphanedEntities == null)
            {
                OrphanedEntities = new Dictionary<int, HashSet<TEntity>>();
            }

            var added = GetOrAddEntities(key, OrphanedEntities).Add(entity);
            Debug.Assert(added);

            return false;
        }

        private bool AddEntity(int key, TEntity entity, Component changedComponent, TEntity referenced)
        {
            var added = GetOrAddEntities(key, Index).Add(entity);
            Debug.Assert(added);

            OnEntityAdded(entity, changedComponent, referenced);
            return true;
        }

        protected override bool TryRemoveEntity(int key, TEntity entity, Component changedComponent)
        {
            if (Index.TryGetValue(key, out var referencingEntities)
                && referencingEntities.Remove(entity))
            {
                if (referencingEntities.Count == 0)
                {
                    Index.Remove(key);
                }

                OnEntityRemoved(entity, changedComponent, FindReferenced(key, entity, fallback: true));
                return true;
            }

            if (OrphanedEntities != null
                && OrphanedEntities.TryGetValue(key, out var orphanedEntities))
            {
                orphanedEntities.Remove(entity);
                if (orphanedEntities.Count == 0)
                {
                    OrphanedEntities.Remove(key);
                }
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

            public void HandleEntityAdded(TEntity entity, Component addedComponent, IEntityGroup<TEntity> group)
            {
                if (_relationship.OrphanedEntities != null
                    && _relationship.OrphanedEntities.TryGetValue(entity.Id, out var orphanedEntities))
                {
                    _relationship.OrphanedEntities.Remove(entity.Id);

                    foreach (var orphanedEntity in orphanedEntities)
                    {
                        _relationship.AddEntity(
                            entity.Id, orphanedEntity, changedComponent: null, entity);
                    }
                }
            }

            public void HandleEntityRemoved(TEntity entity, Component removedComponent, IEntityGroup<TEntity> group)
            {
                foreach (var referencingEntity in _relationship[entity.Id].ToList())
                {
                    _relationship.HandleReferencedEntityRemoved(
                        referencingEntity, entity, removedComponent);
                }
            }

            public bool HandlePropertyValuesChanged(IReadOnlyList<IPropertyValueChange> changes, TEntity entity, IEntityGroup<TEntity> group)
                => false;
        }
    }
}
