using UnicornHack.Utils.MessagingECS;

namespace UnicornHack
{
    public abstract class GameComponent : Component
    {
        public int GameId { get; set; }
        public int EntityId { get; set; }

        public Game Game => Entity.Game;
        public new GameEntity Entity => (GameEntity)base.Entity;

        protected override void Clean()
        {
            GameId = 0;
            EntityId = 0;
            base.Clean();
        }
    }
}
