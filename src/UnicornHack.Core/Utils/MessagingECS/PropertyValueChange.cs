namespace UnicornHack.Utils.MessagingECS
{
    public class PropertyValueChange<T> : IPropertyValueChange
    {
        public PropertyValueChange(Component changedComponent)
        {
            ChangedComponent = changedComponent;
            ChangedPropertyName = null;
            OldValue = default;
            NewValue = default;
        }

        public PropertyValueChange(Component changedComponent, string changedPropertyName, T oldValue, T newValue)
        {
            ChangedComponent = changedComponent;
            ChangedPropertyName = changedPropertyName;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public Component ChangedComponent { get; }
        public string ChangedPropertyName { get; }
        public T OldValue { get; }
        public T NewValue { get; }

        public void EnqueuePropertyValueChangedMessage<TEntity>(string messageName, IEntityManager manager)
            where TEntity : Entity, new()
        {
            var message = manager.Queue.CreateMessage<PropertyValueChangedMessage<TEntity, T>>(messageName);
            message.ChangedPropertyName = ChangedPropertyName;
            message.OldValue = OldValue;
            message.NewValue = NewValue;
            message.ChangedComponent = ChangedComponent;
            message.Entity = (TEntity)ChangedComponent.Entity;

            manager.Queue.Enqueue(message);
        }
    }
}
