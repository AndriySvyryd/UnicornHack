using UnicornHack.Primitives;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Items
{
    public class EquipItemMessage : IMessage
    {
        private GameEntity _itemEntity;
        private GameEntity _actorEntity;

        public GameEntity ItemEntity
        {
            get => _itemEntity;
            set
            {
                _itemEntity?.RemoveReference(this);
                _itemEntity = value;
                _itemEntity?.AddReference(this);
            }
        }

        public GameEntity ActorEntity
        {
            get => _actorEntity;
            set
            {
                _actorEntity?.RemoveReference(this);
                _actorEntity = value;
                _actorEntity?.AddReference(this);
            }
        }

        public EquipmentSlot Slot { get; set; }

        string IMessage.MessageName { get; set; }

        public void Dispose()
        {
            ItemEntity = null;
            ActorEntity = null;
        }
    }
}
