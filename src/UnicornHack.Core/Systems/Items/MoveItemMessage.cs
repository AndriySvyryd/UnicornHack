using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Items
{
    public class MoveItemMessage : IMessage
    {
        private GameEntity _itemEntity;
        private GameEntity _targetContainerEntity;
        private GameEntity _targetLevelEntity;

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

        public GameEntity TargetContainerEntity
        {
            get => _targetContainerEntity;
            set
            {
                _targetContainerEntity?.RemoveReference(this);
                _targetContainerEntity = value;
                _targetContainerEntity?.AddReference(this);
            }
        }

        public GameEntity TargetLevelEntity
        {
            get => _targetLevelEntity;
            set
            {
                _targetLevelEntity?.RemoveReference(this);
                _targetLevelEntity = value;
                _targetLevelEntity?.AddReference(this);
            }
        }

        public Point? TargetCell { get; set; }
        public bool SuppressLog { get; set; }

        string IMessage.MessageName { get; set; }

        public void Dispose()
        {
            ItemEntity = default;
            TargetContainerEntity = default;
            TargetLevelEntity = default;
            TargetCell = default;
            SuppressLog = default;
        }
    }
}
