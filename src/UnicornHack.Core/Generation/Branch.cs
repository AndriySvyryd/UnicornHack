using UnicornHack.Data.Branches;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataLoading;

namespace UnicornHack.Generation
{
    public class Branch : ILoadable
    {
        public string Name { get; set; }
        public byte Length { get; set; }
        public int Difficulty { get; set; }
        public Weight GenerationWeight { get; set; }

        // TODO: Fragment, item and creature generation weight and distribution modifiers
        // TODO: default terrain type for floor/wall/empty space

        public GameBranch Instantiate(Game game)
            => new GameBranch
            {
                Game = game,
                Name = Name,
                Length = Length,
                Difficulty = Difficulty
            };

        public static readonly CSScriptLoader<Branch> Loader =
            new CSScriptLoader<Branch>(@"Data\Branches\", typeof(BranchData));
    }
}
