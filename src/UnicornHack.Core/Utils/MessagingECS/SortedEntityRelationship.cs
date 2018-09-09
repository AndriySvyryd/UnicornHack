using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UnicornHack.Utils.MessagingECS
{
    public class SortedEntityRelationship<TEntity, TKey> : EntityRelationshipBase<TEntity>
        where TEntity : Entity
    {
        private readonly KeyValueGetter<TEntity, TKey> _sortValueGetter;
        private readonly IComparer<TKey> _comparer;
        private static readonly IReadOnlyDictionary<TKey, TEntity> EmptyDictionary = new Dictionary<TKey, TEntity>();

        public SortedEntityRelationship(
            string name,
            IEntityGroup<TEntity> referencingGroup,
            IEntityGroup<TEntity> referencedGroup,
            KeyValueGetter<TEntity, int> keyValueGetter,
            KeyValueGetter<TEntity, TKey> sortValueGetter,
            Action<TEntity, TEntity, int, Component> handleReferencedDeleted,
            bool referencedKeepAlive = false,
            bool referencingKeepAlive = false)
            : this(
                name,
                referencingGroup,
                referencedGroup,
                keyValueGetter,
                sortValueGetter,
                handleReferencedDeleted,
                referencedKeepAlive,
                referencingKeepAlive,
                comparer: null)
        {
        }

        public SortedEntityRelationship(string name,
            IEntityGroup<TEntity> referencingGroup,
            IEntityGroup<TEntity> referencedGroup,
            KeyValueGetter<TEntity, int> keyValueGetter,
            KeyValueGetter<TEntity, TKey> sortValueGetter,
            Action<TEntity, TEntity, int, Component> handleReferencedDeleted,
            bool referencedKeepAlive,
            bool referencingKeepAlive,
            IComparer<TKey> comparer)
            : base(
                name,
                referencingGroup,
                referencedGroup,
                keyValueGetter,
                handleReferencedDeleted,
                referencedKeepAlive,
                referencingKeepAlive)
        {
            _sortValueGetter = sortValueGetter;
            _comparer = comparer;
            referencedGroup.AddListener(new ReferencedGroupListener(this));
        }

        protected Dictionary<int, SortedDictionary<TKey, TEntity>> Index { get; }
            = new Dictionary<int, SortedDictionary<TKey, TEntity>>();

        protected Dictionary<int, HashSet<TEntity>> OrphanedEntities { get; private set; }

        public IReadOnlyDictionary<TKey, TEntity> this[int key]
            => Index.TryGetValue(key, out var entities)
                ? entities
                : EmptyDictionary;

        public override int Count => Index.Values.Sum(v => v.Count);

        private SortedDictionary<TKey, TEntity> GetOrAddEntities(int key)
        {
            if (!Index.TryGetValue(key, out var entities))
            {
                entities = _comparer == null
                    ? new SortedDictionary<TKey, TEntity>()
                    : new SortedDictionary<TKey, TEntity>(_comparer);
                Index.Add(key, entities);
            }

            return entities;
        }

        private HashSet<TEntity> GetOrAddOrphanedEntities(int key)
        {
            if (!OrphanedEntities.TryGetValue(key, out var entities))
            {
                entities = new HashSet<TEntity>(EntityEqualityComparer<TEntity>.Instance);
                OrphanedEntities.Add(key, entities);
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

            var added = GetOrAddOrphanedEntities(key).Add(entity);
            Debug.Assert(added);

            return false;
        }

        private bool AddEntity(
            int key, TEntity entity, int changedComponentId, Component changedComponent, TEntity referenced)
        {
            if (_sortValueGetter.TryGetKey(entity, Component.NullId, null, null, default, out var sortKey))
            {
                GetOrAddEntities(key).Add(sortKey, entity);

                OnEntityAdded(entity, changedComponentId, changedComponent, referenced);
                return true;
            }

            return false;
        }

        protected override bool TryRemoveEntity(
            int key, TEntity entity, int changedComponentId, Component changedComponent)
        {
            if (Index.TryGetValue(key, out var referencingEntities))
            {
                if (!_sortValueGetter.TryGetKey(entity, Component.NullId, null, null, default, out var sortKey))
                {
                    sortKey = referencingEntities.Where(p => p.Value == entity).Select(p => p.Key).FirstOrDefault();
                }

                if (sortKey != default
                    && referencingEntities.Remove(sortKey))
                {
                    if(referencingEntities.Count == 0)
                    {
                        Index.Remove(key);
                    }

                    OnEntityRemoved(entity, changedComponentId, changedComponent,
                        FindReferenced(key, entity, fallback: true));
                    return true;
                }
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

        public override bool HandlePropertyValueChanged<T>(string propertyName, T oldValue, T newValue, int componentId,
            Component component, TEntity entity, IEntityGroup<TEntity> group)
        {
            if (base.HandlePropertyValueChanged(propertyName, oldValue, newValue, componentId, component, entity,
                group))
            {
                return true;
            }

            if (!_sortValueGetter.PropertyUsed(componentId, propertyName))
            {
                return false;
            }

            if (!KeyValueGetter.TryGetKey(entity, Component.NullId, null, null, default, out var key))
            {
                return true;
            }

            var entities = GetOrAddEntities(key);

            if (_sortValueGetter.TryGetKey(entity, componentId, component, propertyName, oldValue, out var oldSortKey))
            {
                entities.Remove(oldSortKey);
            }

            if (_sortValueGetter.TryGetKey(entity, componentId, component, propertyName, newValue, out var newSortKey))
            {
                entities.Add(newSortKey, entity);
            }

            return true;
        }

        public override IEnumerator<TEntity> GetEnumerator()
            => Index.Values.SelectMany(v => v.Values).GetEnumerator();

        private class ReferencedGroupListener : IGroupChangesListener<TEntity>
        {
            private readonly SortedEntityRelationship<TEntity, TKey> _relationship;

            public ReferencedGroupListener(SortedEntityRelationship<TEntity, TKey> relationship)
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
                foreach (var referencingEntity in _relationship[entity.Id].Values.ToList())
                {
                    _relationship.HandleReferencedEntityRemoved(
                        referencingEntity, entity, removedComponentId, removedComponent);
                }
            }

            public bool HandlePropertyValueChanged<T>(string propertyName, T oldValue, T newValue,
                int componentId, Component component, TEntity entity, IEntityGroup<TEntity> group)
                => false;
        }
    }
}
