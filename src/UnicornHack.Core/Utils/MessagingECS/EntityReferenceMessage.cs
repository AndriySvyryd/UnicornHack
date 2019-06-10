namespace UnicornHack.Utils.MessagingECS
{
    /// <summary>
    ///     Keeps a reference to the entity in the queue. This message doesn't need to be handled.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EntityReferenceMessage<TEntity> : IMessage
        where TEntity : Entity
    {
        public const string Name = "EntityReference";

        public static void Enqueue(GameEntity entity, GameManager manager)
        {
            var message = manager.Queue.CreateMessage<EntityReferenceMessage<GameEntity>>(Name);
            message.Entity = entity;

            manager.Enqueue(message, lowPriority: true);
        }

        private TEntity _entity;

        public TEntity Entity
        {
            get => _entity;
            set
            {
                _entity?.RemoveReference(this);
                _entity = value;
                _entity?.AddReference(this);
            }
        }

        string IMessage.MessageName { get; set; }

        public void Dispose() => Entity = default;
    }
}
