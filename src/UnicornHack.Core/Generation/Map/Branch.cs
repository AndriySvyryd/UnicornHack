using System;
using System.Collections.Generic;
using UnicornHack.Utils;

namespace UnicornHack.Generation.Map
{
    public class Branch : ILoadable
    {
        public virtual string Name { get; set; }
        public virtual int Length { get; set; }
        public virtual int GameId { get; private set; }
        public virtual Game Game { get; set; }
        public virtual Weight GenerationWeight { get; set; }
        // TODO: Fragment, item and creature generation weight and distribution modifiers
        // TODO: default terrain type for floor/wall/empty space

        public virtual ICollection<Level> Levels { get; private set; } = new HashSet<Level>();

        #region Creation

        public Branch()
        {
        }

        protected Branch(Game game)
            : this()
        {
            Game = game;
            Game.Branches.Add(this);
        }

        public virtual Branch Instantiate(Game game)
        {
            if (Game != null)
            {
                throw new InvalidOperationException("This branch is already part of a game.");
            }

            var branchInstance = new Branch(game)
            {
                Name = Name
            };

            return branchInstance;
        }

        void ILoadable.OnLoad()
        {
        }

        #endregion

        #region Serialization

        public static readonly CSScriptLoader<Branch> Loader = new CSScriptLoader<Branch>(@"data\branches\");

        public static Branch Get(string name) => Loader.Get(name);

        public static IEnumerable<Branch> GetAllBranches() => Loader.GetAll();

        #endregion
    }
}