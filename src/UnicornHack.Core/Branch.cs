using System.Collections.Generic;

namespace UnicornHack
{
    public class Branch
    {
        public virtual string Name { get; set; }
        public virtual byte Length { get; set; }
        public virtual int GameId { get; set; }
        public virtual Game Game { get; set; }
        public virtual int Difficulty { get; set; }

        public virtual ICollection<Level> Levels { get; } = new HashSet<Level>();

        public Branch()
        {
        }

        public Branch(Game game, string name) : this()
        {
            Game = game;
            Name = name;
            Game.Branches.Add(this);
            Game.Repository.Add(this);
        }
    }
}