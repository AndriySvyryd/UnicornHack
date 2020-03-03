using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Senses
{
    public class VisibleTerrainChangedMessage : IMessage
    {
        public const string Name = "VisibleTerrainChanged";

        public static VisibleTerrainChangedMessage Create(GameManager manager)
            => manager.Queue.CreateMessage<VisibleTerrainChangedMessage>(Name);

        public static void Enqueue(GameEntity levelEntity, GameManager manager)
        {
            var terrainChanged = Create(manager);
            terrainChanged.LevelEntity = levelEntity;

            manager.Enqueue(terrainChanged);
        }

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

        string IMessage.MessageName { get; set; }

        public void Clean()
        {
            LevelEntity = default;
        }
    }
}
