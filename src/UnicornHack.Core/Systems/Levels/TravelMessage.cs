using UnicornHack.Primitives;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Levels
{
    public class TravelMessage : IMessage
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

        public Direction TargetHeading { get; set; }
        public Point TargetCell { get; set; }

        string IMessage.MessageName { get; set; }

        public void Dispose() => Entity = null;
    }
}
