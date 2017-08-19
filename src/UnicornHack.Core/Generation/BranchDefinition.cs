using UnicornHack.Data.Branches;
using UnicornHack.Utils;

namespace UnicornHack.Generation
{
    public class BranchDefinition : ILoadable
    {
        public virtual string Name { get; set; }
        public virtual byte Length { get; set; }
        public virtual int Difficulty { get; set; }
        public virtual Weight GenerationWeight { get; set; }

        // TODO: Fragment, item and creature generation weight and distribution modifiers
        // TODO: default terrain type for floor/wall/empty space

        public virtual Branch Instantiate(Game game)
            => new Branch(game, Name) {Length = Length, Difficulty = Difficulty};

        public static readonly CSScriptLoader<BranchDefinition> Loader =
            new CSScriptLoader<BranchDefinition>(@"Data\Branches\", typeof(BranchDefinitionData));
    }
}