namespace UnicornHack.Utils.MessagingECS
{
    public class PropertyValueChangedMessage<TEntity, T> : IMessage
        where TEntity : Entity
    {
        public string Property { get; set; }
        public T OldValue { get; set; }
        public T NewValue { get; set; }
        public int ComponentId { get; set; }
        public Component Component { get; set; }
        public TEntity Entity { get; set; }

        string IMessage.MessageName { get; set; }

        public void Dispose()
        {
        }
    }
}
