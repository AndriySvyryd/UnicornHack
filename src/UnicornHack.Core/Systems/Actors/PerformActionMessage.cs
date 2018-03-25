using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Actors
{
    public class PerformActionMessage : IMessage
    {
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

        public void Dispose() => Actor = null;
    }
}
