using System;
using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class Branch : ILoadable
    {
        public virtual string Name { get; set; }
        public virtual byte Length { get; set; }
        public virtual int GameId { get; set; }
        public virtual Game Game { get; set; }
        public virtual int Difficulty { get; set; }

        public virtual Weight GenerationWeight { get; set; }
        // TODO: Fragment, item and creature generation weight and distribution modifiers
        // TODO: default terrain type for floor/wall/empty space

        public virtual ICollection<Level> Levels { get; } = new HashSet<Level>();

        #region Creation

        public Branch()
        {
        }

        protected Branch(Game game, string name, byte length)
            : this()
        {
            Game = game;
            Name = name;
            Length = length;
            Game.Branches.Add(this);
            Game.Repository.Add(this);
        }

        public virtual Branch Instantiate(Game game)
        {
            if (Game != null)
            {
                throw new InvalidOperationException("This branch is already part of a game.");
            }

            return new Branch(game, Name, Length);
        }

        void ILoadable.OnLoad()
        {
        }

        #endregion

        #region Serialization

        public static readonly CSScriptLoader<Branch> Loader = new CSScriptLoader<Branch>(@"data\branches\");

        public static Branch Get(string name) => Loader.Get(name);

        public static IReadOnlyList<Branch> GetAllBranches() => Loader.GetAll();

        #endregion
    }
}