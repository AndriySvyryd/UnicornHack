namespace UnicornHack.Utils.MessagingECS
{
    public class PropertyValueChangedMessage<TEntity, T> : IMessage
        where TEntity : Entity
    {
        public string ChangedPropertyName { get; set; }
        public T OldValue { get; set; }
        public T NewValue { get; set; }
        public Component ChangedComponent { get; set; }
        public TEntity Entity { get; set; }

        string IMessage.MessageName { get; set; }

        public void Dispose()
        {
        }
    }
}
