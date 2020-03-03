using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Actors
{
    public class DecideNextActionMessage : IMessage
    {
        public const string Name = "DecideNextAction";

        public static void Enqueue(GameEntity aiEntity, GameManager manager)
        {
            var message = manager.Queue.CreateMessage<DecideNextActionMessage>(Name);
            message.Actor = aiEntity;
            manager.Enqueue(message, lowPriority: true);
        }

        private GameEntity _actor;

        public GameEntity Actor
        {
            get => _actor;
            set
            {
                _actor?.RemoveReference(this);
                _actor = value;
                _actor?.AddReference(this);
            }
        }

        string IMessage.MessageName { get; set; }

        public void Clean() => Actor = null;
    }
}
