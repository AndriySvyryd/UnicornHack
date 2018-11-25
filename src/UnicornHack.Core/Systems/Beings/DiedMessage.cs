using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Beings
{
    public class DiedMessage : IMessage
    {
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

        public void Dispose() => BeingEntity = default;
    }
}
