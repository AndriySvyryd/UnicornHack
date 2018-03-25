using UnicornHack.Primitives;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Items
{
    public class ItemActivatedMessage : IMessage
    {
        private GameEntity _activatorEntity;
        private GameEntity _targetEntity;
        private GameEntity _itemEntity;

        public GameEntity ActivatorEntity
        {
            get => _activatorEntity;
            set
            {
                _activatorEntity?.RemoveReference(this);
                _activatorEntity = value;
                _activatorEntity?.AddReference(this);
            }
        }

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

        public GameEntity TargetEntity
        {
            get => _targetEntity;
            set
            {
                _targetEntity?.RemoveReference(this);
                _targetEntity = value;
                _targetEntity?.AddReference(this);
            }
        }

        public ActivationType ActivationType { get; set; }
        public Point? TargetCell { get; set; }
        public bool Successful { get; set; }
        public int Delay { get; set; }

        string IMessage.MessageName { get; set; }

        public void Dispose()
        {
            ActivatorEntity = null;
            ItemEntity = null;
            TargetEntity = null;
        }
    }
}
