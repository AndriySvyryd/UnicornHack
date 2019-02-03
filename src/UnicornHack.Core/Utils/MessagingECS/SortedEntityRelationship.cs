using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UnicornHack.Utils.MessagingECS
{
    public class SortedEntityRelationship<TEntity, TKey> : EntityRelationshipBase<TEntity>
        where TEntity : Entity, new()
    {
        private readonly IKeyValueGetter<TEntity, TKey> _sortValueGetter;
        private readonly IComparer<TKey> _comparer;
        private static readonly IReadOnlyDictionary<TKey, TEntity> EmptyDictionary = new Dictionary<TKey, TEntity>();

        public SortedEntityRelationship(
            string name,
            IEntityGroup<TEntity> referencingGroup,
            IEntityGroup<TEntity> referencedGroup,
            IKeyValueGetter<TEntity, int> keyValueGetter,
            IKeyValueGetter<TEntity, TKey> sortValueGetter,
            Action<TEntity, TEntity, Component> handleReferencedDeleted,
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
            IKeyValueGetter<TEntity, int> keyValueGetter,
            IKeyValueGetter<TEntity, TKey> sortValueGetter,
            Action<TEntity, TEntity, Component> handleReferencedDeleted,
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

            var added = GetOrAddOrphanedEntities(key).Add(entity);
            Debug.Assert(added);

            return false;
        }

        private bool AddEntity(
            int key, TEntity entity, Component changedComponent, TEntity referenced)
        {
            if (_sortValueGetter.TryGetKey(entity, new IPropertyValueChange[0], getOldValue: false, out var sortKey))
            {
                GetOrAddEntities(key).Add(sortKey, entity);

                OnEntityAdded(entity, changedComponent, referenced);
                return true;
            }

            return false;
        }

        protected override bool TryRemoveEntity(int key, TEntity entity, Component changedComponent)
        {
            if (Index.TryGetValue(key, out var referencingEntities))
            {
                if (!_sortValueGetter.TryGetKey(entity, new IPropertyValueChange[0], getOldValue: false,
                    out var sortKey))
                {
                    sortKey = referencingEntities.Where(p => p.Value == entity).Select(p => p.Key).FirstOrDefault();
                }

                if (sortKey != default
                    && referencingEntities.Remove(sortKey))
                {
                    if (referencingEntities.Count == 0)
                    {
                        Index.Remove(key);
                    }

                    OnEntityRemoved(entity, changedComponent, FindReferenced(key, entity, fallback: true));
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

        public override bool HandlePropertyValuesChanged(
            IReadOnlyList<IPropertyValueChange> changes, TEntity entity, IEntityGroup<TEntity> group)
        {
            if (base.HandlePropertyValuesChanged(changes, entity, group))
            {
                return true;
            }

            Component componentUsed = null;
            for (var i = 0; i < changes.Count; i++)
            {
                var change = changes[i];
                if (_sortValueGetter.PropertyUsed(change.ChangedComponent.ComponentId, change.ChangedPropertyName))
                {
                    componentUsed = change.ChangedComponent;
                    break;
                }

                // The component might have been removed by the previous change listener
                if (!entity.HasComponent(change.ChangedComponent.ComponentId))
                {
                    return true;
                }
            }

            if (componentUsed == null)
            {
                return false;
            }

            if (!KeyValueGetter.TryGetKey(entity, new IPropertyValueChange[0], getOldValue: false, out var key))
            {
                return true;
            }

            var entities = GetOrAddEntities(key);

            if (_sortValueGetter.TryGetKey(entity, changes, getOldValue: true, out var oldSortKey))
            {
                entities.Remove(oldSortKey);
            }

            if (_sortValueGetter.TryGetKey(entity, changes, getOldValue: false, out var newSortKey))
            {
                entities.Add(newSortKey, entity);
            }

            if (entities.Count == 0)
            {
                Index.Remove(key);
            }

            return true;
        }

        public override string ToString() => "SortedRelationship: " + Name;

        public override IEnumerator<TEntity> GetEnumerator()
            => Index.Values.SelectMany(v => v.Values).GetEnumerator();

        private class ReferencedGroupListener : IGroupChangesListener<TEntity>
        {
            private readonly SortedEntityRelationship<TEntity, TKey> _relationship;

            public ReferencedGroupListener(SortedEntityRelationship<TEntity, TKey> relationship)
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
                foreach (var referencingEntity in _relationship[entity.Id].Values.ToList())
                {
                    if (_relationship.KeyValueGetter.TryGetKey(
                        referencingEntity,
                        changes: null,
                        getOldValue: true,
                        out var keyValue))
                    {
                        if (_relationship.OrphanedEntities == null)
                        {
                            _relationship.OrphanedEntities = new Dictionary<int, HashSet<TEntity>>();
                        }

                        // Need to add to orphaned list first, since removing it from the relationship might remove the last reference to it
                        _relationship.GetOrAddOrphanedEntities(keyValue).Add(referencingEntity);
                        _relationship.TryRemoveEntity(keyValue, referencingEntity, changedComponent: null);
                    }

                    _relationship.HandleReferencedEntityRemoved(
                        referencingEntity, entity, removedComponent);
                }
            }

            public bool HandlePropertyValuesChanged(IReadOnlyList<IPropertyValueChange> changes, TEntity entity,
                IEntityGroup<TEntity> group)
                => false;

            public override string ToString() => _relationship.ToString();
        }
    }
}
