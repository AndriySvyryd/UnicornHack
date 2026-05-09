using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Levels;

[Component(Id = (int)EntityComponent.Room)]
public class RoomComponent : GameComponent, IKeepAliveComponent
{
    private string? _branchName;
    private byte _depth;
    private GameLevel? _level;
    private RoomFeatures _features;
    private RoomReveal _reveal;
    private byte _gridX;
    private byte _gridY;
    private bool _isVisited;
    private byte _height;
    private byte _width;
    private byte[] _visibleTerrain = Array.Empty<byte>();
    private DirectionFlags[] _knownNeighbors = Array.Empty<DirectionFlags>();
    private MapFeature[] _terrain = Array.Empty<MapFeature>();
    private MapFeature[] _knownTerrain = Array.Empty<MapFeature>();
    private DirectionFlags[] _neighbors = Array.Empty<DirectionFlags>();

    public RoomComponent()
    {
        ComponentId = (int)EntityComponent.Room;
    }

    public string BranchName
    {
        get => _branchName!;
        set
        {
            _level = null;
            SetWithNotify(value, ref _branchName);
        }
    }

    public byte Depth
    {
        get => _depth;
        set
        {
            _level = null;
            SetWithNotify(value, ref _depth);
        }
    }

    public GameLevel Level
    {
        get
        {
            if (_level != null)
            {
                return _level;
            }

            if (_branchName == null)
            {
                return null!;
            }

            var entity = Entity;
            var branch = entity?.Game?.GetBranch(_branchName);
            if (branch == null)
            {
                return null!;
            }

            foreach (var l in branch.Levels)
            {
                if (l.Depth == _depth)
                {
                    return _level = l;
                }
            }

            return null!;
        }
        set
        {
            BranchName = value.BranchName;
            Depth = value.Depth;
            _level = value;
        }
    }

    public RoomFeatures Features
    {
        get => _features;
        set => SetWithNotify(value, ref _features);
    }

    public RoomReveal Reveal
    {
        get => _reveal;
        set => SetWithNotify(value, ref _reveal);
    }

    public byte GridX
    {
        get => _gridX;
        set => SetWithNotify(value, ref _gridX);
    }

    public byte GridY
    {
        get => _gridY;
        set => SetWithNotify(value, ref _gridY);
    }

    public bool IsVisited
    {
        get => _isVisited;
        set => SetWithNotify(value, ref _isVisited);
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

    public byte[] VisibleTerrain
    {
        get => _visibleTerrain;
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

    public MapFeature[] Terrain
    {
        get => _terrain;
        set => SetWithNotify(value, ref _terrain);
    }

    public Dictionary<short, MapFeature>? TerrainChanges
    {
        get;
        set;
    } = new();

    public MapFeature[] KnownTerrain
    {
        get => _knownTerrain;
        set => SetWithNotify(value, ref _knownTerrain);
    }

    public Dictionary<short, MapFeature>? KnownTerrainChanges
    {
        get;
        set;
    } = new();

    public DirectionFlags[] Neighbors
    {
        get => _neighbors;
        set => SetWithNotify(value, ref _neighbors);
    }

    public Dictionary<short, DirectionFlags>? NeighborChanges
    {
        get;
        set;
    } = new();

    public DirectionFlags[] KnownNeighbors
    {
        get => _knownNeighbors;
        set => SetWithNotify(value, ref _knownNeighbors);
    }

    public bool KnownNeighborsChanged
    {
        get;
        set;
    }

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
        ? KnownNeighbors[PointToIndex![x, y]]
        : DirectionFlags.None;

    public bool IsValid(Point point)
        => point.X < Width && point.Y < Height; // Since byte is unsigned there is no need to compare with 0

    protected override void Clean()
    {
        LevelLayout.ReleaseTerrain(this);

        ((Dictionary<Point, GameEntity>?)Actors)?.Clear();
        ((Dictionary<Point, GameEntity>?)Items)?.Clear();
        ((Dictionary<Point, GameEntity>?)Connections)?.Clear();
        ((HashSet<GameEntity>?)IncomingConnections)?.Clear();
        ((Dictionary<Point, GameEntity>?)KnownActors)?.Clear();
        ((Dictionary<Point, GameEntity>?)KnownItems)?.Clear();
        ((Dictionary<Point, GameEntity>?)KnownConnections)?.Clear();

        _branchName = default;
        _depth = default;
        _level = default;
        _features = default;
        _reveal = default;
        _gridX = default;
        _gridY = default;
        _isVisited = default;

        base.Clean();
    }
}
