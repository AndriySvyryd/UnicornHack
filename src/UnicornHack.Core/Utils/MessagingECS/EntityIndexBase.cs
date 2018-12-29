using System.Collections.Generic;

namespace UnicornHack.Utils.MessagingECS
{
    public abstract class EntityIndexBase<TEntity, TKey> : IGroupChangesListener<TEntity>
        where TEntity : Entity
    {
        protected EntityIndexBase(
            string name,
            IEntityGroup<TEntity> group,
            IKeyValueGetter<TEntity, TKey> keyValueGetter)
        {
            Name = name;
            KeyValueGetter = keyValueGetter;
            group.AddListener(this);
            Group = group;
        }

        public string Name { get; }
        protected IKeyValueGetter<TEntity, TKey> KeyValueGetter { get; }
        protected IEntityGroup<TEntity> Group { get; }

        public void HandleEntityAdded(
            TEntity entity, Component addedComponent, IEntityGroup<TEntity> group)
        {
            if (KeyValueGetter.TryGetKey(
                entity,
                new IPropertyValueChange[] {new PropertyValueChange<object>(addedComponent)},
                getOldValue: false,
                out var keyValue))
            {
                TryAddEntity(keyValue, entity, addedComponent);
            }
        }

        public void HandleEntityRemoved(
            TEntity entity, Component removedComponent, IEntityGroup<TEntity> group)
        {
            if (KeyValueGetter.TryGetKey(
                entity,
                new IPropertyValueChange[] {new PropertyValueChange<object>(removedComponent)},
                getOldValue: false,
                out var keyValue))
            {
                TryRemoveEntity(keyValue, entity, removedComponent);
            }
        }

        public virtual bool HandlePropertyValuesChanged(
            IReadOnlyList<IPropertyValueChange> changes, TEntity entity, IEntityGroup<TEntity> group)
        {
            Component componentUsed = null;
            for (var i = 0; i < changes.Count; i++)
            {
                var change = changes[i];
                if (KeyValueGetter.PropertyUsed(change.ChangedComponent.ComponentId, change.ChangedPropertyName))
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

            if (KeyValueGetter.TryGetKey(entity, changes, getOldValue: true, out var oldKey))
            {
                TryRemoveEntity(oldKey, entity, componentUsed);
            }

            if (KeyValueGetter.TryGetKey(entity, changes, getOldValue: false, out var newKey))
            {
                TryAddEntity(newKey, entity, componentUsed);
            }

            return true;
        }

        protected abstract bool TryAddEntity(TKey key, TEntity entity, Component changedComponent);

        protected abstract bool TryRemoveEntity(TKey key, TEntity entity, Component changedComponent);
    }
}
