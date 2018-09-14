namespace UnicornHack.Utils.MessagingECS
{
    public class EntityRemovedMessage<TEntity> : IMessage
        where TEntity : Entity
    {
        public TEntity Entity { get; set; }
        public Component ChangedComponent { get; set; }

        string IMessage.MessageName { get; set; }

        public void Dispose()
        {
        }
    }
}
