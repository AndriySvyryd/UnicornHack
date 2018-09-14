using System;
using System.Collections;
using System.Collections.Generic;

namespace UnicornHack.Utils.MessagingECS
{
    public abstract class EntityRelationshipBase<TEntity> : EntityIndexBase<TEntity, int>, IEntityGroup<TEntity>
        where TEntity : Entity, new()
    {
        private readonly Action<TEntity, TEntity, Component> _handleReferencedDeleted;
        private readonly bool _referencedKeepAlive;
        private readonly bool _referencingKeepAlive;

        private readonly List<IGroupChangesListener<TEntity>> _changeListeners
            = new List<IGroupChangesListener<TEntity>>();

        private string _entityAddedMessageName;
        private string _entityRemovedMessageName;
        private Dictionary<string, string> _propertyValueChangedMessageNames;

        private const string EntityAddedMessageSuffix = "_EntityAdded";
        private const string EntityRemovedMessageSuffix = "_EntityRemoved";
        private const string PropertyValueChangedMessageSuffix = "_PropertyValueChanged_";

        protected EntityRelationshipBase(
            string name,
            IEntityGroup<TEntity> referencingGroup,
            IEntityGroup<TEntity> referencedGroup,
            IKeyValueGetter<TEntity, int> keyValueGetter,
            Action<TEntity, TEntity, Component> handleReferencedDeleted,
            bool referencedKeepAlive,
            bool referencingKeepAlive)
            : base(referencingGroup, keyValueGetter)
        {
            Name = name;
            ReferencingGroup = referencingGroup;
            ReferencedGroup = referencedGroup;
            _handleReferencedDeleted = handleReferencedDeleted;
            _referencedKeepAlive = referencedKeepAlive;
            _referencingKeepAlive = referencingKeepAlive;
        }

        public string Name { get; }
        protected IEntityGroup<TEntity> ReferencedGroup { get; }
        protected IEntityGroup<TEntity> ReferencingGroup { get; }
        public bool IsLoading => ReferencedGroup.IsLoading || ReferencingGroup.IsLoading;
        public abstract int Count { get; }

        protected TEntity FindReferenced(int key, TEntity entity, bool fallback)
        {
            var referenced = ReferencedGroup.FindEntity(key);
            if (referenced == null)
            {
                if (fallback)
                {
                    var manager = entity.Manager;
                    // TODO: If loaded entity is to be deleted load all components as well
                    referenced = (manager.FindEntity(key) ?? manager.LoadEntity(key)) as TEntity;
                }
                else if (!ReferencedGroup.IsLoading)
                {
                    throw new InvalidOperationException(
                        $"Couldn't find entity '{key}' in '{ReferencedGroup.Name}' referenced from entity '{entity.Id}'"
                        + $" in '{ReferencingGroup.Name}' through '{Name}'");
                }
            }

            return referenced;
        }

        protected virtual void OnEntityAdded(TEntity entity, Component changedComponent, TEntity referenced)
        {
            if (_referencedKeepAlive)
            {
                referenced.AddReference(entity);
            }

            if (_referencingKeepAlive)
            {
                entity.AddReference(referenced);
            }

            foreach (var entityIndex in _changeListeners)
            {
                entityIndex.HandleEntityAdded(entity, changedComponent, this);
            }

            if (_entityAddedMessageName != null
                && !IsLoading)
            {
                var message = entity.Manager.Queue.CreateMessage<EntityAddedMessage<TEntity>>(_entityAddedMessageName);
                message.Entity = entity;
                message.ChangedComponent = changedComponent;

                entity.Manager.Queue.Enqueue(message);
            }
        }

        protected void OnEntityRemoved(TEntity entity, Component changedComponent, TEntity referenced)
        {
            var manager = entity.Manager;
            if (referenced != null
                && _referencedKeepAlive)
            {
                referenced.RemoveReference(entity);
            }

            if (_referencingKeepAlive)
            {
                entity.RemoveReference(referenced);
            }

            foreach (var entityIndex in _changeListeners)
            {
                entityIndex.HandleEntityRemoved(entity, changedComponent, this);
            }

            if (_entityRemovedMessageName != null)
            {
                var message = manager.Queue.CreateMessage<EntityRemovedMessage<TEntity>>(_entityRemovedMessageName);
                message.Entity = entity;
                message.ChangedComponent = changedComponent;

                manager.Queue.Enqueue(message);
            }
        }

        public override bool HandlePropertyValuesChanged(
            IReadOnlyList<IPropertyValueChange> changes, TEntity entity, IEntityGroup<TEntity> group)
        {
            if (base.HandlePropertyValuesChanged(changes, entity, group)
                || !ContainsEntity(entity))
            {
                return true;
            }

            foreach (var entityIndex in _changeListeners)
            {
                entityIndex.HandlePropertyValuesChanged(changes, entity, this);
            }

            if (_propertyValueChangedMessageNames != null)
            {
                foreach (var change in changes)
                {
                    if (_propertyValueChangedMessageNames.TryGetValue(change.ChangedPropertyName, out var messageName)
                        && entity.HasComponent(change.ChangedComponent.ComponentId))
                    {
                        change.EnqueuePropertyValueChangedMessage<TEntity>(messageName, entity.Manager);
                    }
                }
            }

            return false;
        }

        protected void HandleReferencedEntityRemoved(
            TEntity referencingEntity, TEntity referencedEntity, Component removedComponent)
            => _handleReferencedDeleted(referencingEntity, referencedEntity, removedComponent);

        public string GetEntityAddedMessageName()
            => _entityAddedMessageName
               ?? (_entityAddedMessageName = Name + EntityAddedMessageSuffix);

        public string GetEntityRemovedMessageName()
            => _entityRemovedMessageName
               ?? (_entityRemovedMessageName = Name + EntityRemovedMessageSuffix);

        public string GetPropertyValueChangedMessageName(string propertyName)
        {
            if (_propertyValueChangedMessageNames == null)
            {
                _propertyValueChangedMessageNames = new Dictionary<string, string>();
            }

            if (!_propertyValueChangedMessageNames.TryGetValue(propertyName, out var messageName))
            {
                messageName = Name + PropertyValueChangedMessageSuffix + propertyName;
                _propertyValueChangedMessageNames[propertyName] = messageName;
            }

            return messageName;
        }

        public void AddListener(IGroupChangesListener<TEntity> index)
            => _changeListeners.Add(index);

        /// <summary>
        ///     Tries to find a referencing entity with the given <paramref name="id" />.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TEntity FindEntity(int id)
        {
            var entity = ReferencingGroup.FindEntity(id);
            if (entity != null
                && ContainsEntity(entity))
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
        public bool ContainsEntity(int id)
        {
            var entity = ReferencingGroup.FindEntity(id);
            return entity != null && ContainsEntity(entity);
        }

        public bool ContainsEntity(TEntity entity)
            => KeyValueGetter.TryGetKey(entity, new IPropertyValueChange[0], getOldValue: false, out var referencedId)
               && ReferencedGroup.ContainsEntity(referencedId);

        public abstract IEnumerator<TEntity> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
