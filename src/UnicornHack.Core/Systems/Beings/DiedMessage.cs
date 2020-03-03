using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Beings
{
    public class DiedMessage : IMessage
    {
        public const string Name = "Died";

        public static void Enqueue(GameEntity entity, GameManager manager)
        {
            var died = manager.Queue.CreateMessage<DiedMessage>(Name);
            died.BeingEntity = entity;
            manager.Enqueue(died);
        }

        private GameEntity _beingEntity;

        public GameEntity BeingEntity
        {
            get => _beingEntity;
            set
            {
                _beingEntity?.RemoveReference(this);
                _beingEntity = value;
                _beingEntity?.AddReference(this);
            }
        }

        string IMessage.MessageName { get; set; }

        public void Clean() => BeingEntity = default;
    }
}
