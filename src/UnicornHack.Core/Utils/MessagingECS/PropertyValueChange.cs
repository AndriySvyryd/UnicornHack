namespace UnicornHack.Utils.MessagingECS
{
    public readonly struct PropertyValueChange<T> : IPropertyValueChange
    {
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
        public bool IsEmpty => ChangedComponent == null;

        public static readonly PropertyValueChange<T> Empty = new(null, null, default, default);

        public void EnqueuePropertyValueChangedMessage<TEntity>(string messageName)
            where TEntity : Entity, new()
        {
            var entity = (TEntity)ChangedComponent.Entity;
            var manager = entity.Manager;
            var message = manager.Queue.CreateMessage<PropertyValueChangedMessage<TEntity, T>>(messageName);
            message.ChangedPropertyName = ChangedPropertyName;
            message.OldValue = OldValue;
            message.NewValue = NewValue;
            message.ChangedComponent = ChangedComponent;
            message.Entity = entity;

            manager.Queue.Enqueue(message);
        }
    }
}
