using UnicornHack.Services;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils;

namespace UnicornHack;

public class Game : NotificationEntity
{
    private int _id;
    private int _currentTick;
    private int _nextEntityId;
    private int _nextLogId;
    private int _nextCommandId;
    private SimpleRandom _random = null!;
    private GameOptions _options = new();
    private GenerationStats? _generationStats;

    public int Id
    {
        get => _id;
        // ReSharper disable once UnusedMember.Local
        private set => SetWithNotify(value, ref _id);
    }

    public int CurrentTick
    {
        get => _currentTick;
        set => SetWithNotify(value, ref _currentTick);
    }

    public int NextEntityId
    {
        get => _nextEntityId;
        set => SetWithNotify(value, ref _nextEntityId);
    }

    public int NextLogId
    {
        get => _nextLogId;
        set => SetWithNotify(value, ref _nextLogId);
    }

    public int NextCommandId
    {
        get => _nextCommandId;
        set => SetWithNotify(value, ref _nextCommandId);
    }

    public SimpleRandom Random
    {
        get => _random;
        // ReSharper disable once UnusedMember.Local
        set => SetWithNotify(value, ref _random);
    }

    public GameOptions Options
    {
        get => _options;
        set => SetWithNotify(value, ref _options);
    }

    public ICollection<GameBranch> Branches
    {
        get;
        set;
    } = new ObservableHashSet<GameBranch>(ReferenceEqualityComparer.Instance);

    /// <summary>
    ///     Game-wide cumulative generation statistics. Populated alongside the per-level
    ///     <see cref="GameLevel.GenerationStats" /> on every successful fragment / creature
    ///     / item placement and persisted as an owned entity. Backs the
    ///     <c>$globalInstances</c>, <c>$globalTagInstances</c>, and
    ///     <c>$globalFeatureInstances</c> DSL parameters so weight expressions can
    ///     throttle fragments / tags / room features that have already appeared in
    ///     earlier levels of this run.
    /// </summary>
    public GenerationStats GenerationStats
    {
        get => _generationStats ??= new GenerationStats();
        set => SetWithNotify(value, ref _generationStats);
    }

    // Unmapped properties
    public GameServices Services
    {
        get;
        set;
    } = null!;

    public GameManager Manager
    {
        get;
        set;
    } = null!;

    public IRepository Repository
    {
        get;
        set;
    } = null!;

    public GameBranch? GetBranch(string branchName)
        => Branches.FirstOrDefault(b => b.Name == branchName)
           ?? Repository.Find<GameBranch>(Id, branchName);

    public GameEntity? LoadRoom(int roomId)
    {
        var roomEntity = Manager.FindEntity(roomId);
        if (roomEntity == null
            || LevelGenerator.EnsureGenerated(roomEntity.Room!))
        {
            return roomEntity;
        }

        var roomsToLoad = roomEntity.Room!.Connections.Values
            .Select(c => c.Connection!.TargetRoomId)
            .Where(id => Manager.FindEntity(id) == null)
            .ToList();
        if (roomsToLoad.Count > 0)
        {
            Repository.LoadRooms(roomsToLoad);
        }

        return roomEntity;
    }

    public override bool Equals(object? obj)
        => obj is Game otherGame
           && (ReferenceEquals(this, otherGame) || Id == otherGame.Id);

    public override int GetHashCode() => Id.GetHashCode();
}
