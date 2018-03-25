using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnicornHack.Utils.MessagingECS
{
    public class UniqueEntityRelationship<TEntity> : EntityRelationshipBase<TEntity>
        where TEntity : Entity
    {
        public UniqueEntityRelationship(
            string name,
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

        protected Dictionary<int, TEntity> Index { get; } = new Dictionary<int, TEntity>();
        protected Dictionary<int, TEntity> OrphanedEntities { get; private set; }
        public TEntity this[int key] => Index.TryGetValue(key, out var entity) ? entity : null;
        public override int Count => Index.Count;

        protected override bool TryAddEntity(int key, TEntity entity, int changedComponentId,
            Component changedComponent)
        {
            var referenced = FindReferenced(key, entity, fallback: false);
            if (referenced != null)
            {
                return AddEntity(key, entity, changedComponentId, changedComponent, referenced);
            }

            if (OrphanedEntities == null)
            {
                OrphanedEntities = new Dictionary<int, TEntity>();
            }

            Debug.Assert(!OrphanedEntities.TryGetValue(key, out _));
            OrphanedEntities[key] = entity;

            return false;
        }

        private bool AddEntity(
            int key, TEntity entity, int changedComponentId, Component changedComponent, TEntity referenced)
        {
            Debug.Assert(!Index.TryGetValue(key, out _));
            Index[key] = entity;
            OnEntityAdded(entity, changedComponentId, changedComponent, referenced);
            return true;
        }


        protected override bool TryRemoveEntity(
            int key, TEntity entity, int changedComponentId, Component changedComponent)
        {
            if (Index.Remove(key))
            {
                OnEntityRemoved(entity, changedComponentId, changedComponent,
                    FindReferenced(key, entity, fallback: true));
                return true;
            }

            OrphanedEntities?.Remove(key);

            return false;
        }

        public override IEnumerator<TEntity> GetEnumerator()
            => Index.Values.GetEnumerator();

        private class ReferencedGroupListener : IGroupChangesListener<TEntity>
        {
            private readonly UniqueEntityRelationship<TEntity> _relationship;

            public ReferencedGroupListener(UniqueEntityRelationship<TEntity> relationship)
                => _relationship = relationship;

            public void HandleEntityAdded
                (TEntity entity, int addedComponentId, Component addedComponent, IEntityGroup<TEntity> group)
            {
                if (_relationship.OrphanedEntities != null
                    && _relationship.OrphanedEntities.TryGetValue(entity.Id, out var orphanedEntity))
                {
                    _relationship.OrphanedEntities.Remove(entity.Id);
                    _relationship.AddEntity(
                        entity.Id, orphanedEntity, Component.NullId, changedComponent: null, entity);
                }
            }

            public void HandleEntityRemoved(
                TEntity entity, int removedComponentId, Component removedComponent, IEntityGroup<TEntity> group)
            {
                var referencingEntity = _relationship[entity.Id];
                if (referencingEntity != null)
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
