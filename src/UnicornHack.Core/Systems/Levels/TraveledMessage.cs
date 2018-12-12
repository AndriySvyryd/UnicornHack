using UnicornHack.Primitives;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Levels
{
    public class TraveledMessage : IMessage
    {
        private GameEntity _entity;
        private GameEntity _initialLevel;

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

        public GameEntity InitialLevel
        {
            get => _initialLevel;
            set
            {
                _initialLevel?.RemoveReference(this);
                _initialLevel = value;
                _initialLevel?.AddReference(this);
            }
        }

        public Direction InitialHeading { get; set; }
        public Point InitialLevelCell { get; set; }
        public bool Successful { get; set; }
        public int Delay { get; set; }

        string IMessage.MessageName { get; set; }

        public void Dispose()
        {
            Entity = default;
            InitialLevel = default;
            InitialHeading = default;
            InitialLevelCell = default;
            Successful = default;
            Delay = default;
        }
    }
}
