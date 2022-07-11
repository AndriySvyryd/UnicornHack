using UnicornHack.Data.Branches;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataLoading;

namespace UnicornHack.Generation;

public class Branch : ILoadable
{
    public string Name
    {
        get;
        set;
    } = null!;

    public byte Length
    {
        get;
        set;
    }

    public int Difficulty
    {
        get;
        set;
    }

    private string? _generationWeight;
    
    public string? GenerationWeight
    {
        get => _generationWeight;
        set
        {
            _generationWeight = value;
            //_weightFunction = null;
        }
    }

    // TODO: default terrain type for floor/wall/empty space

    public GameBranch Instantiate(Game game)
        => new() { Game = game, Name = Name, Length = Length, Difficulty = Difficulty };

    public static readonly CSScriptLoader<Branch> Loader =
        new(@"Data\Branches\", typeof(BranchData));
}
