using UnicornHack.Utils;

namespace UnicornHack
{
    public abstract class EntityKnowledge
    {
        protected EntityKnowledge()
        {
        }

        protected EntityKnowledge(Game game) => Game = game;

        public virtual void Delete()
        {
        }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public virtual int Id { get; private set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int GameId { get; private set; }

        public Game Game { get; set; }

        public string BranchName { get; set; }
        public byte? LevelDepth { get; set; }
        public Level Level { get; set; }
        public byte LevelX { get; set; }
        public byte LevelY { get; set; }

        public Point LevelCell => new Point(LevelX, LevelY);
    }
}