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
        public string GenerationWeight { get; set; }

        // TODO: default terrain type for floor/wall/empty space

        public GameBranch Instantiate(Game game)
            => new()
            {
                Game = game,
                Name = Name,
                Length = Length,
                Difficulty = Difficulty
            };

        public static readonly CSScriptLoader<Branch> Loader =
            new(@"Data\Branches\", typeof(BranchData));
    }
}
