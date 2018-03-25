namespace UnicornHack.Utils.MessagingECS
{
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IGroupChangesListener<TEntity> where TEntity : Entity
    {
        void HandleEntityAdded(
            TEntity entity, int addedComponentId, Component addedComponent, IEntityGroup<TEntity> group);

        void HandleEntityRemoved(
            TEntity entity, int removedComponentId, Component removedComponent, IEntityGroup<TEntity> group);

        bool HandlePropertyValueChanged<T>(string propertyName, T oldValue, T newValue, int componentId,
            Component component, TEntity entity, IEntityGroup<TEntity> group);
    }
}
