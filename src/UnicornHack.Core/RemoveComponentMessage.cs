using UnicornHack.Utils.MessagingECS;

namespace UnicornHack
{
    public class RemoveComponentMessage : IMessage
    {
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

        public void Dispose()
        {
            Entity = default;
            Component = default;
        }
    }
}
