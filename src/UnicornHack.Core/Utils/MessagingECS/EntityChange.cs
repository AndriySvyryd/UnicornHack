namespace UnicornHack.Utils.MessagingECS
{
    public readonly struct EntityChange<TEntity>
        where TEntity : Entity, new()
    {
        public EntityChange(
            TEntity entity,
            IPropertyValueChanges propertyChanges = null,
            Component removedComponent = null)
        {
            Entity = entity;
            PropertyChanges = propertyChanges ?? PropertyValueChanges.Instance;
            RemovedComponent = removedComponent;
        }

        public TEntity Entity { get; }
        public IPropertyValueChanges PropertyChanges { get; }
        public Component RemovedComponent { get; }
    }
}
