using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Senses
{
    public class VisibleTerrainChangedMessage : IMessage
    {
        private GameEntity _levelEntity;

        public GameEntity LevelEntity
        {
            get => _levelEntity;
            set
            {
                _levelEntity?.RemoveReference(this);
                _levelEntity = value;
                _levelEntity?.AddReference(this);
            }
        }

        public int TilesExplored { get; set; }

        string IMessage.MessageName { get; set; }

        public void Dispose() => LevelEntity = null;
    }
}
