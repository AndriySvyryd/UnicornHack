using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Items
{
    public class ItemMovedMessage : IMessage
    {
        private GameEntity _itemEntity;
        private GameEntity _initialContainer;

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

        public GameEntity InitialContainer
        {
            get => _initialContainer;
            set
            {
                _initialContainer?.RemoveReference(this);
                _initialContainer = value;
                _initialContainer?.AddReference(this);
            }
        }

        public Point? InitialLevelCell { get; set; }
        public int InitialCount { get; set; }
        public bool Successful { get; set; }
        public int Delay { get; set; }
        public bool SuppressLog { get; set; }

        string IMessage.MessageName { get; set; }

        public void Dispose()
        {
            ItemEntity = default;
            InitialContainer = default;
            InitialLevelCell = default;
            InitialCount = default;
            Successful = default;
            Delay = default;
            SuppressLog = default;
        }
    }
}
