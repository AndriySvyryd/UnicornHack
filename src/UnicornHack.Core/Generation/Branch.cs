using System.Linq.Expressions;
using CSharpScriptSerialization;
using UnicornHack.Data.Branches;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataLoading;

namespace UnicornHack.Generation;

public class Branch : ILoadable, ICSScriptSerializable
{
    public const byte DefaultRoomWidth = 25;
    public const byte DefaultRoomHeight = 16;

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
    private Func<string, int, float>? _weightFunction;

    private static readonly ParameterExpression BranchParameter =
        Expression.Parameter(typeof(string), name: "branch");

    private static readonly ParameterExpression DepthParameter =
        Expression.Parameter(typeof(int), name: "depth");

    private static readonly UnicornExpressionVisitor _translator =
        new([BranchParameter, DepthParameter]);

    /// <summary>
    ///     Weight used to pick this branch as the destination of an inter-branch
    ///     connection. Evaluated with <c>$branch</c> (the source branch's name) and
    ///     <c>$depth</c> (the source level's depth) so authors can express rules like
    ///     "never pick surface as a target". Defaults to <c>"1"</c> — every branch is a
    ///     valid candidate. Returning <c>0</c> excludes the branch from random selection.
    /// </summary>
    public string? GenerationWeight
    {
        get => _generationWeight;
        set
        {
            _generationWeight = value;
            _weightFunction = null;
        }
    }

    /// <summary>
    ///     Returns this branch's weight as a destination of an inter-branch connection
    ///     sourced from <paramref name="sourceBranchName" /> at <paramref name="sourceDepth" />.
    /// </summary>
    public float GetWeight(string sourceBranchName, int sourceDepth)
    {
        if (Name == sourceBranchName)
        {
            return 0;
        }

        if (_weightFunction == null)
        {
            try
            {
                _weightFunction = _translator.Translate<Func<string, int, float>, float>(
                    GenerationWeight ?? "1");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    "Error while parsing the GenerationWeight for branch " + Name, e);
            }
        }

        try
        {
            return _weightFunction(sourceBranchName, sourceDepth);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                "Error while evaluating the GenerationWeight for branch " + Name, e);
        }
    }

    /// <summary>
    ///     Default <see cref="LevelLayout" /> for levels in this branch. Individual fragments do
    ///     not override the layout — it is purely a level-scope concern.
    /// </summary>
    public LevelLayout Layout
    {
        get;
        set;
    } = new EmptyLevelLayout();

    /// <summary>
    ///     Default <see cref="Map.RoomLayout" /> used to fill each <see cref="RoomComponent" />
    ///     created on levels in this branch. Branches that produce a single room per level
    ///     (e.g. paired with <see cref="EmptyLevelLayout" />) typically override this with
    ///     <see cref="EmptyRoomLayout" />.
    /// </summary>
    public RoomLayout RoomLayout
    {
        get;
        set;
    } = new UniformRoomLayout();

    /// <summary>
    ///     Default <see cref="CreatureGenerator" /> for rooms in this branch. Fragments may
    ///     override per-fragment via <see cref="DefiningMapFragment.CreatureGenerator" />.
    /// </summary>
    public CreatureGenerator CreatureGenerator
    {
        get;
        set;
    } = new();

    /// <summary>
    ///     Default <see cref="ItemGenerator" /> for rooms in this branch. Fragments may override
    ///     per-fragment via <see cref="DefiningMapFragment.ItemGenerator" />.
    /// </summary>
    public ItemGenerator ItemGenerator
    {
        get;
        set;
    } = new();

    /// <summary>
    ///     Default room width (tiles) for this branch. Fragments may override per-fragment via
    ///     <see cref="DefiningMapFragment.RoomWidth" />.
    /// </summary>
    public byte RoomWidth
    {
        get;
        set;
    } = DefaultRoomWidth;

    /// <summary>
    ///     Default room height (tiles) for this branch. Fragments may override per-fragment via
    ///     <see cref="DefiningMapFragment.RoomHeight" />.
    /// </summary>
    public byte RoomHeight
    {
        get;
        set;
    } = DefaultRoomHeight;

    /// <summary>
    ///     When <c>true</c>, the branch's terminal level (Depth == <see cref="Length" />)
    ///     ends the branch — its exit room sources no inter-branch connection. When
    ///     <c>false</c> (the default) the terminal level's exit room sources a connection
    ///     to a randomly-picked next branch (weighted by each candidate branch's
    ///     <see cref="GenerationWeight" />).
    /// </summary>
    public bool IsTerminal
    {
        get;
        set;
    }

    /// <summary>
    ///     Default fill <see cref="MapFeature" /> for empty terrain in this branch. Fragments
    ///     may override per-fragment via <see cref="DefiningMapFragment.DefaultTerrain" />.
    /// </summary>
    public MapFeature DefaultTerrain
    {
        get;
        set;
    }

    /// <summary>
    ///     Default <see cref="MapFeature" /> for path/corridor tiles in this branch.
    /// </summary>
    public MapFeature DefaultPathTerrain
    {
        get;
        set;
    }

    /// <summary>
    ///     Creates a per-game <see cref="GameBranch" /> for this template and registers it on
    ///     <paramref name="game" /> so subsequent <see cref="Game.GetBranch" /> calls can find it.
    /// </summary>
    public GameBranch Instantiate(Game game)
    {
        var branch = new GameBranch
        {
            Game = game,
            Name = Name,
            Length = Length,
            Difficulty = Difficulty,
            IsTerminal = IsTerminal
        };
        game.Branches.Add(branch);
        return branch;
    }

    public static readonly CSScriptLoader<Branch> Loader =
        new(@"Data\Branches\", typeof(BranchData));

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Branch>(GetPropertyConditions<Branch>());

    public ICSScriptSerializer GetSerializer() => Serializer;

    /// <summary>
    ///     Conditions deciding whether each property is emitted by the serializer. Properties
    ///     are skipped when they're equal to the field-initializer default. Complex types
    ///     (Layout / CreatureGenerator / ItemGenerator) are emitted when their concrete type
    ///     differs from the default OR when their state differs from a fresh instance.
    /// </summary>
    protected static Dictionary<string, Func<TBranch, object?, bool>> GetPropertyConditions<TBranch>()
        where TBranch : Branch => new()
    {
        { nameof(Length), (_, v) => (byte)v! != 0 },
        { nameof(Difficulty), (_, v) => (int)v! != 0 },
        { nameof(GenerationWeight), (_, v) => v != null },
        { nameof(Layout), (_, v) => v is not EmptyLevelLayout },
        { nameof(RoomLayout), (_, v) => v is not UniformRoomLayout || v.GetType() != typeof(UniformRoomLayout) },
        {
            nameof(CreatureGenerator), (_, v)
                => v is not CreatureGenerator { ExpectedInitialCount: 8 } cg
                    || cg.GetType() != typeof(CreatureGenerator)
        },
        {
            nameof(ItemGenerator), (_, v)
                => v is not ItemGenerator { ExpectedInitialCount: 3 } ig
                    || ig.GetType() != typeof(ItemGenerator)
        },
        { nameof(RoomWidth), (_, v) => (byte)v! != DefaultRoomWidth },
        { nameof(RoomHeight), (_, v) => (byte)v! != DefaultRoomHeight },
        { nameof(DefaultTerrain), (_, v) => (MapFeature)v! != MapFeature.Default },
        { nameof(DefaultPathTerrain), (_, v) => (MapFeature)v! != MapFeature.Default },
        { nameof(IsTerminal), (_, v) => (bool)v! }
    };
}
