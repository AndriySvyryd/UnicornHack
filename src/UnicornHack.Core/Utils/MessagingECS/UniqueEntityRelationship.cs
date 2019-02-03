using System;
using System.Collections.Generic;

namespace UnicornHack.Utils.MessagingECS
{
    public class UniqueEntityRelationship<TEntity> : EntityRelationshipBase<TEntity>
        where TEntity : Entity, new()
    {
        public UniqueEntityRelationship(
            string name,
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

        protected Dictionary<int, TEntity> Index { get; } = new Dictionary<int, TEntity>();
        protected Dictionary<int, TEntity> OrphanedEntities { get; private set; }
        public TEntity this[int key] => Index.TryGetValue(key, out var entity) ? entity : null;
        public override int Count => Index.Count;

        protected override bool TryAddEntity(int key, TEntity entity, Component changedComponent)
        {
            var referenced = FindReferenced(key, entity, fallback: false);
            if (referenced != null)
            {
                return AddEntity(key, entity, changedComponent, referenced);
            }

            if (OrphanedEntities == null)
            {
                OrphanedEntities = new Dictionary<int, TEntity>();
            }
            else if (OrphanedEntities.ContainsKey(key))
            {
                throw new InvalidOperationException(
                    $"The key {key} already exists in the unique relationship for {Group.Name}");
            }

            OrphanedEntities[key] = entity;

            return false;
        }

        private bool AddEntity(int key, TEntity entity, Component changedComponent, TEntity referenced)
        {
            if (Index.ContainsKey(key))
            {
                throw new InvalidOperationException(
                    $"The key {key} already exists in the unique relationship for {Group.Name}");
            }

            Index[key] = entity;
            OnEntityAdded(entity, changedComponent, referenced);
            return true;
        }

        protected override bool TryRemoveEntity(int key, TEntity entity, Component changedComponent)
        {
            if (Index.Remove(key))
            {
                OnEntityRemoved(entity, changedComponent, FindReferenced(key, entity, fallback: true));
                return true;
            }

            OrphanedEntities?.Remove(key);

            return false;
        }

        public override string ToString() => "UniqueRelationship: " + Name;

        public override IEnumerator<TEntity> GetEnumerator()
            => Index.Values.GetEnumerator();

        private class ReferencedGroupListener : IGroupChangesListener<TEntity>
        {
            private readonly UniqueEntityRelationship<TEntity> _relationship;

            public ReferencedGroupListener(UniqueEntityRelationship<TEntity> relationship)
                => _relationship = relationship;

            public void HandleEntityAdded(TEntity entity, Component addedComponent, IEntityGroup<TEntity> group)
            {
                if (_relationship.OrphanedEntities != null
                    && _relationship.OrphanedEntities.TryGetValue(entity.Id, out var orphanedEntity))
                {
                    _relationship.OrphanedEntities.Remove(entity.Id);
                    _relationship.AddEntity(entity.Id, orphanedEntity, changedComponent: null, entity);
                }
            }

            public void HandleEntityRemoved(TEntity entity, Component removedComponent, IEntityGroup<TEntity> group)
            {
                var referencingEntity = _relationship[entity.Id];
                if (referencingEntity != null)
                {
                    if (_relationship.KeyValueGetter.TryGetKey(
                        referencingEntity,
                        changes: null,
                        getOldValue: true,
                        out var keyValue))
                    {
                        if (_relationship.OrphanedEntities == null)
                        {
                            _relationship.OrphanedEntities = new Dictionary<int, TEntity>();
                        }

                        // Need to add to orphaned list first, since removing it from the relationship might remove the last reference to it
                        _relationship.OrphanedEntities[keyValue] = entity;
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
