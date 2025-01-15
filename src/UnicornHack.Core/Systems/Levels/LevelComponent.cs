using System.Buffers;
using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Levels;

[Component(Id = (int)EntityComponent.Level)]
public class LevelComponent : GameComponent, IKeepAliveComponent
{
    private string? _branchName;
    private GameBranch? _branch;
    private int _difficulty;
    private byte _depth;
    private byte _height;
    private byte _width;
    private SimpleRandom? _generationRandom;
    private byte[]? _visibleTerrain;
    private byte[]? _visibleNeighbors;
    private byte[]? _terrain;
    private byte[]? _knownTerrain;
    private byte[]? _wallNeighbors;

    public LevelComponent()
    {
        ComponentId = (int)EntityComponent.Level;
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

    public int Difficulty
    {
        get => _difficulty;
        set => SetWithNotify(value, ref _difficulty);
    }

    public byte Depth
    {
        get => _depth;
        set => SetWithNotify(value, ref _depth);
    }

    public byte Height
    {
        get => _height;
        set => SetWithNotify(value, ref _height);
    }

    public byte Width
    {
        get => _width;
        set => SetWithNotify(value, ref _width);
    }

    public short TileCount => (short)(Height * Width);

    public SimpleRandom GenerationRandom
    {
        get => _generationRandom!;
        set => SetWithNotify(value, ref _generationRandom);
    }

    public byte[] VisibleTerrain
    {
        get => _visibleTerrain!;
        set => SetWithNotify(value, ref _visibleTerrain);
    }

    public byte[]? VisibleTerrainSnapshot
    {
        get;
        set;
    }

    public Dictionary<short, byte>? VisibleTerrainChanges
    {
        get;
        set;
    }

    public byte[] VisibleNeighbors
    {
        get => _visibleNeighbors!;
        set => SetWithNotify(value, ref _visibleNeighbors);
    }

    public bool VisibleNeighborsChanged
    {
        get;
        set;
    }

    public byte[] Terrain
    {
        get => _terrain!;
        set => SetWithNotify(value, ref _terrain);
    }

    public Dictionary<short, byte>? TerrainChanges
    {
        get;
        set;
    } = new();

    public byte[] KnownTerrain
    {
        get => _knownTerrain!;
        set => SetWithNotify(value, ref _knownTerrain);
    }

    public Dictionary<short, byte>? KnownTerrainChanges
    {
        get;
        set;
    } = new();

    // TODO: Track known neighbors as well
    public byte[] WallNeighbors
    {
        get => _wallNeighbors!;
        set => SetWithNotify(value, ref _wallNeighbors);
    }

    public Dictionary<short, byte>? WallNeighborsChanges
    {
        get;
        set;
    } = new();

    public IReadOnlyDictionary<Point, GameEntity> Actors
    {
        get;
    } = new Dictionary<Point, GameEntity>();

    public IReadOnlyDictionary<Point, GameEntity> Items
    {
        get;
    } = new Dictionary<Point, GameEntity>();

    public IReadOnlyDictionary<Point, GameEntity> Connections
    {
        get;
    } = new Dictionary<Point, GameEntity>();

    public IReadOnlyCollection<GameEntity> IncomingConnections
    {
        get;
    } = new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);

    public IReadOnlyDictionary<Point, GameEntity> KnownActors
    {
        get;
    } = new Dictionary<Point, GameEntity>();

    public IReadOnlyDictionary<Point, GameEntity> KnownItems
    {
        get;
    } = new Dictionary<Point, GameEntity>();

    public IReadOnlyDictionary<Point, GameEntity> KnownConnections
    {
        get;
    } = new Dictionary<Point, GameEntity>();

    public BeveledVisibilityCalculator? VisibilityCalculator
    {
        get;
        private set;
    }

    public PathFinder? PathFinder
    {
        get;
        private set;
    }

    public short[,]? PointToIndex
    {
        get;
        private set;
    }

    public Point[]? IndexToPoint
    {
        get;
        private set;
    }

    public Rectangle BoundingRectangle => new(new Point(0, 0), Width, Height);

#pragma warning disable CS8774
    [MemberNotNull("PointToIndex")]
    [MemberNotNull("IndexToPoint")]
    [MemberNotNull("PathFinder")]
    [MemberNotNull("VisibilityCalculator")]
    public void EnsureInitialized()
    {
        if (PointToIndex == null
            && Width != 0)
        {
            (PointToIndex, IndexToPoint) = Rectangle.GetPointIndex(Game.Services.SharedCache, Width, Height);
            PathFinder = new PathFinder(PointToIndex, IndexToPoint);
            VisibilityCalculator = new BeveledVisibilityCalculator(GetVisibleNeighbors, this);
        }
    }
#pragma warning restore CS8774

    private DirectionFlags GetVisibleNeighbors(byte x, byte y) => x < Width && y < Height
        ? (DirectionFlags)VisibleNeighbors[PointToIndex![x, y]]
        : DirectionFlags.None;

    public bool IsValid(Point point)
        => point.X < Width && point.Y < Height; // Since byte is unsigned there is no need to compare with 0

    protected override void Clean()
    {
        if (_visibleTerrain != null)
        {
            ArrayPool<byte>.Shared.Return(_visibleTerrain);
        }

        if (_visibleNeighbors != null)
        {
            ArrayPool<byte>.Shared.Return(_visibleNeighbors);
        }

        if (_terrain != null)
        {
            ArrayPool<byte>.Shared.Return(_terrain);
        }

        if (_knownTerrain != null)
        {
            ArrayPool<byte>.Shared.Return(_knownTerrain);
        }

        if (_wallNeighbors != null)
        {
            ArrayPool<byte>.Shared.Return(_wallNeighbors);
        }

        ((Dictionary<Point, GameEntity>?)Actors)?.Clear();
        ((Dictionary<Point, GameEntity>?)Items)?.Clear();
        ((Dictionary<Point, GameEntity>?)Connections)?.Clear();
        ((HashSet<GameEntity>?)IncomingConnections)?.Clear();
        ((Dictionary<Point, GameEntity>?)KnownActors)?.Clear();
        ((Dictionary<Point, GameEntity>?)KnownItems)?.Clear();
        ((Dictionary<Point, GameEntity>?)KnownConnections)?.Clear();

        _branchName = default;
        _branch = default;
        _difficulty = default;
        _depth = default;
        _height = default;
        _width = default;
        _generationRandom = default;
        _visibleTerrain = default;
        _visibleNeighbors = default;
        _terrain = default;
        _knownTerrain = default;
        _wallNeighbors = default;

        base.Clean();
    }
}
