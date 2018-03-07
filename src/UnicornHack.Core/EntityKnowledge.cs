using UnicornHack.Utils;

namespace UnicornHack
{
    public abstract class EntityKnowledge
    {
        protected EntityKnowledge()
        {
        }

        protected EntityKnowledge(Game game) => Game = game;

        public virtual void Delete() => Game.Repository.Delete(this);

        public virtual int Id { get; set; }

        public int GameId { get; set; }

        public Game Game { get; set; }

        public virtual int? EntityId { get; set; }

        public string BranchName { get; set; }
        public byte? LevelDepth { get; set; }
        public Level Level { get; set; }
        public byte LevelX { get; set; }
        public byte LevelY { get; set; }

        public Point LevelCell => new Point(LevelX, LevelY);
    }
}