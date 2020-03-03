using UnicornHack.Utils.MessagingECS;

namespace UnicornHack
{
    public class RemoveComponentMessage : IMessage
    {
        public const string Name = "RemoveComponent";

        public static void Enqueue(GameEntity entity, EntityComponent component, GameManager manager)
        {
            var message = manager.Queue.CreateMessage<RemoveComponentMessage>(Name);
            message.Entity = entity;
            message.Component = component;

            manager.Enqueue(message, lowPriority: true);
        }

        private GameEntity _entity;

        public GameEntity Entity
        {
            get => _entity;
            set
            {
                _entity?.RemoveReference(this);
                _entity = value;
                _entity?.AddReference(this);
            }
        }

        public EntityComponent Component { get; set; }

        string IMessage.MessageName { get; set; }

        public void Clean()
        {
            Entity = default;
            Component = default;
        }
    }
}
