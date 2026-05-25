using UnicornHack.Utils;

namespace UnicornHack.Systems.Levels;

public class GameLevel : NotificationEntity
{
    private int _gameId;
    private Game? _game;
    private string? _branchName;
    private GameBranch? _branch;
    private byte _depth;
    private int _difficulty;
    private SimpleRandom? _generationRandom;
    private LevelGenerationStats? _generationStats;

    public int GameId
    {
        get => _gameId;
        set => SetWithNotify(value, ref _gameId);
    }

    public Game Game
    {
        get => _game!;
        set => SetWithNotify(value, ref _game);
    }

    public string BranchName
    {
        get => _branchName!;
        set => SetWithNotify(value, ref _branchName);
    }

    public GameBranch Branch
    {
        get => _branch!;
        set => SetWithNotify(value, ref _branch);
    }

    public byte Depth
    {
        get => _depth;
        set => SetWithNotify(value, ref _depth);
    }

    public int Difficulty
    {
        get => _difficulty;
        set => SetWithNotify(value, ref _difficulty);
    }

    public SimpleRandom GenerationRandom
    {
        get => _generationRandom!;
        set => SetWithNotify(value, ref _generationRandom);
    }

    /// <summary>
    ///     Diagnostic counters captured during level generation. Populated by
    ///     <see cref="LevelGenerator" /> and accessed by weight functions.
    /// </summary>
    public LevelGenerationStats GenerationStats
    {
        get => _generationStats ??= new LevelGenerationStats();
        set => SetWithNotify(value, ref _generationStats);
    }

    public ICollection<RoomComponent> Rooms
    {
        get;
    } = new ObservableHashSet<RoomComponent>();
}
