using System.Buffers;
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
            if (value == null)
            {
                BranchName = null!;
                Depth = 0;
                _level = null;
                return;
            }

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

    /// <summary>
    ///     Transient generation-only bitmask of <see cref="MapFragment.PlaceableFeatureBits" />
    ///     entries that have already been assigned to a chunk in this room — either through
    ///     a concrete feature glyph or through a wildcard binding performed by
    ///     <see cref="MapFragment.TryPlace(RoomComponent, in LotPlacementContext)" />. Reset
    ///     by <see cref="UnicornHack.Generation.Map.LevelLayout.AllocateTerrain" /> at the start
    ///     of room generation and not persisted.
    /// </summary>
    public RoomFeatures BoundPlaceableFeatures
    {
        get;
        set;
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

    // The dictionaries are sorted for deterministic behavior
    public IReadOnlyDictionary<Point, GameEntity> Actors
    {
        get;
    } = new SortedDictionary<Point, GameEntity>();

    public IReadOnlyDictionary<Point, GameEntity> Items
    {
        get;
    } = new SortedDictionary<Point, GameEntity>();

    public IReadOnlyDictionary<Point, GameEntity> Connections
    {
        get;
    } = new SortedDictionary<Point, GameEntity>();

    public IReadOnlyCollection<GameEntity> IncomingConnections
    {
        get;
    } = new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);

    public IReadOnlyDictionary<Point, GameEntity> KnownActors
    {
        get;
    } = new SortedDictionary<Point, GameEntity>();

    public IReadOnlyDictionary<Point, GameEntity> KnownItems
    {
        get;
    } = new SortedDictionary<Point, GameEntity>();

    public IReadOnlyDictionary<Point, GameEntity> KnownConnections
    {
        get;
    } = new SortedDictionary<Point, GameEntity>();

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
            (PointToIndex, IndexToPoint) = Rectangle.GetPointIndex(Width, Height);
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

    /// <summary>
    ///     Reverts this room to its pre-allocation placeholder state: returns terrain
    ///     arrays to their pools, clears generated content (actors / items / outgoing
    ///     connections / known terrain), and resets <see cref="Width" />/<see cref="Height" />
    ///     to zero so <see cref="Map.LevelLayout.GetEmptyEntranceRoom" /> recognises it
    ///     as reusable on the next generation attempt. Crucially, <see cref="IncomingConnections" />
    ///     is left intact: it's the only handle a neighboring level has on this room
    ///     and re-creating it requires re-running the neighbor's generation. Used by
    ///     <see cref="LevelGenerator.ResetGeneratedState" /> when a generation attempt
    ///     fails and the level needs a clean slate without orphaning cross-level links.
    /// </summary>
    public void Reset()
    {
        if (Terrain.Length > 0)
        {
            ArrayPool<MapFeature>.Shared.Return(Terrain);
            Terrain = Array.Empty<MapFeature>();
        }

        if (Neighbors.Length > 0)
        {
            ArrayPool<DirectionFlags>.Shared.Return(Neighbors);
            Neighbors = Array.Empty<DirectionFlags>();
        }

        if (KnownNeighbors.Length > 0)
        {
            ArrayPool<DirectionFlags>.Shared.Return(KnownNeighbors);
            KnownNeighbors = Array.Empty<DirectionFlags>();
        }

        if (VisibleTerrain.Length > 0)
        {
            ArrayPool<byte>.Shared.Return(VisibleTerrain);
            VisibleTerrain = Array.Empty<byte>();
        }

        if (KnownTerrain.Length > 0)
        {
            ArrayPool<MapFeature>.Shared.Return(KnownTerrain);
            KnownTerrain = Array.Empty<MapFeature>();
        }

        Width = 0;
        Height = 0;
        BoundPlaceableFeatures = 0;
        Features = 0;
        TerrainChanges = null;
        KnownTerrainChanges = null;
        NeighborChanges = null;
        PointToIndex = null;
        IndexToPoint = null;
        PathFinder = null;
        VisibilityCalculator = null;

        ((SortedDictionary<Point, GameEntity>?)Actors)?.Clear();
        ((SortedDictionary<Point, GameEntity>?)Items)?.Clear();
        ((SortedDictionary<Point, GameEntity>?)Connections)?.Clear();
        ((SortedDictionary<Point, GameEntity>?)KnownActors)?.Clear();
        ((SortedDictionary<Point, GameEntity>?)KnownItems)?.Clear();
        ((SortedDictionary<Point, GameEntity>?)KnownConnections)?.Clear();
        // IncomingConnections intentionally preserved.
    }

    protected override void Clean()
    {
        // Reset intentionally preserves IncomingConnections because it's
        // called between generation attempts where the cross-level link must survive;
        // Clean runs at full component disposal so it additionally clears that and the
        // identity / position fields that uniquely tie the component to a level.
        Reset();

        ((HashSet<GameEntity>?)IncomingConnections)?.Clear();

        _branchName = default;
        _depth = default;
        _level = default;
        _reveal = default;
        _gridX = default;
        _gridY = default;
        _isVisited = default;

        base.Clean();
    }
}
