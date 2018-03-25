namespace UnicornHack.Utils.MessagingECS
{
    public abstract class EntityIndexBase<TEntity, TKey> : IGroupChangesListener<TEntity>
        where TEntity : Entity
    {
        protected EntityIndexBase(
            IEntityGroup<TEntity> group,
            KeyValueGetter<TEntity, TKey> keyValueGetter)
        {
            KeyValueGetter = keyValueGetter;
            group.AddListener(this);
            Group = group;
        }

        protected KeyValueGetter<TEntity, TKey> KeyValueGetter { get; }
        protected IEntityGroup<TEntity> Group { get; }

        public void HandleEntityAdded(
            TEntity entity, int addedComponentId, Component addedComponent, IEntityGroup<TEntity> group)
        {
            if (KeyValueGetter.TryGetKey(entity, addedComponentId, addedComponent, null, default, out var keyValue))
            {
                TryAddEntity(keyValue, entity, addedComponentId, addedComponent);
            }
        }

        public void HandleEntityRemoved(
            TEntity entity, int removedComponentId, Component removedComponent, IEntityGroup<TEntity> group)
        {
            if (KeyValueGetter.TryGetKey(entity, removedComponentId, removedComponent, null, default, out var keyValue))
            {
                TryRemoveEntity(keyValue, entity, removedComponentId, removedComponent);
            }
        }

        public virtual bool HandlePropertyValueChanged<T>(string propertyName, T oldValue, T newValue, int componentId,
            Component component, TEntity entity, IEntityGroup<TEntity> group)
        {
            if (!KeyValueGetter.PropertyUsed(componentId, propertyName))
            {
                return false;
            }

            if (KeyValueGetter.TryGetKey(entity, componentId, component, propertyName, oldValue, out var oldKey))
            {
                TryRemoveEntity(oldKey, entity, componentId, component);
            }

            if (KeyValueGetter.TryGetKey(entity, componentId, component, propertyName, newValue, out var newKey))
            {
                TryAddEntity(newKey, entity, componentId, component);
            }

            return true;
        }

        protected abstract bool TryAddEntity(
            TKey key, TEntity entity, int changedComponentId, Component changedComponent);

        protected abstract bool TryRemoveEntity(
            TKey key, TEntity entity, int changedComponentId, Component changedComponent);
    }
}
