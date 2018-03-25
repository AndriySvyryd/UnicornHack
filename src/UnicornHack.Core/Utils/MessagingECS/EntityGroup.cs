using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

// ReSharper disable StaticMemberInGenericType
namespace UnicornHack.Utils.MessagingECS
{
    public class EntityGroup<TEntity> : IEntityGroup<TEntity>
        where TEntity : Entity, new()
    {
        private readonly EntityManager<TEntity> _manager;
        private readonly Dictionary<int, TEntity> _entities = new Dictionary<int, TEntity>();

        private readonly List<IGroupChangesListener<TEntity>> _changeListeners
            = new List<IGroupChangesListener<TEntity>>();

        private string _entityAddedMessageName;
        private string _entityRemovedMessageName;
        private Dictionary<string, string> _propertyValueChangedMessageNames;

        private const string EntityAddedMessageSuffix = "_EntityAdded";
        private const string EntityRemovedMessageSuffix = "_EntityRemoved";
        private const string PropertyValueChangedMessageSuffix = "_PropertyValueChanged_";

        public EntityGroup(string name, EntityMatcher<TEntity> matcher, EntityManager<TEntity> manager)
        {
            Name = name;
            Matcher = matcher;
            _manager = manager;
        }

        public string Name { get; }
        public EntityMatcher<TEntity> Matcher { get; }
        public int Count => _entities.Count;

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

        public TEntity FindEntity(int id) => _entities.TryGetValue(id, out var entity) ? entity : null;
        public bool ContainsEntity(int id) => _entities.ContainsKey(id);
        public bool IsLoading => _manager.IsLoading;

        public void HandleEntityComponentChanged(TEntity entity, int changedComponentId, Component changedComponent)
        {
            if (_entities.ContainsKey(entity.Id))
            {
                if (!Matcher.Matches(entity))
                {
                    RemoveEntity(entity, changedComponentId, changedComponent);
                }
            }
            else
            {
                if (Matcher.Matches(entity))
                {
                    AddEntity(entity, changedComponentId, changedComponent);
                }
            }
        }

        private void AddEntity(TEntity entity, int changedComponentId, Component changedComponent)
        {
            Debug.Assert(!_entities.ContainsKey(entity.Id));

            _entities[entity.Id] = entity;

            foreach (var entityIndex in _changeListeners)
            {
                entityIndex.HandleEntityAdded(entity, changedComponentId, changedComponent, this);
            }

            if (_entityAddedMessageName != null
                && !IsLoading)
            {
                var message = entity.Manager.Queue.CreateMessage<EntityAddedMessage<TEntity>>(_entityAddedMessageName);
                message.Entity = entity;
                message.ChangedComponentId = changedComponentId;
                message.ChangedComponent = changedComponent;

                entity.Manager.Queue.Enqueue(message);
            }
        }

        private void RemoveEntity(TEntity entity, int changedComponentId, Component changedComponent)
        {
            Debug.Assert(_entities.ContainsKey(entity.Id));

            var manager = entity.Manager;
            _entities.Remove(entity.Id);

            foreach (var entityIndex in _changeListeners)
            {
                entityIndex.HandleEntityRemoved(entity, changedComponentId, changedComponent, this);
            }

            if (_entityRemovedMessageName != null)
            {
                var message = manager.Queue.CreateMessage<EntityRemovedMessage<TEntity>>(_entityRemovedMessageName);
                message.Entity = entity;
                message.ChangedComponentId = changedComponentId;
                message.ChangedComponent = changedComponent;

                manager.Queue.Enqueue(message);
            }
        }

        public void HandlePropertyValueChanged<T>(
            string propertyName, T oldValue, T newValue, int componentId, Component component, TEntity entity)
        {
            if (Matcher.Matches(entity))
            {
                foreach (var entityIndex in _changeListeners)
                {
                    entityIndex.HandlePropertyValueChanged(propertyName, oldValue, newValue, componentId, component,
                        entity, this);
                }

                if (_propertyValueChangedMessageNames != null
                    && _propertyValueChangedMessageNames.TryGetValue(propertyName, out var messageName))
                {
                    var message =
                        entity.Manager.Queue.CreateMessage<PropertyValueChangedMessage<TEntity, T>>(messageName);
                    message.Property = propertyName;
                    message.OldValue = oldValue;
                    message.NewValue = newValue;
                    message.ComponentId = componentId;
                    message.Component = component;
                    message.Entity = entity;

                    entity.Manager.Queue.Enqueue(message);
                }
            }
        }

        public IEnumerator<TEntity> GetEnumerator() => _entities.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
