using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Utils;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Levels
{
    [Component(Id = (int)EntityComponent.Level)]
    public class LevelComponent : GameComponent, IKeepAliveComponent
    {
        private string _branchName;
        private GameBranch _branch;
        private int _difficulty;
        private byte _depth;
        private byte _height;
        private byte _width;
        private SimpleRandom _generationRandom;
        private byte[] _visibleTerrain;
        private byte[] _visibleNeighbours;
        private byte[] _terrain;
        private byte[] _knownTerrain;
        private byte[] _wallNeighbors;

        public LevelComponent()
            => ComponentId = (int)EntityComponent.Level;

        public string BranchName
        {
            get => _branchName;
            set => SetWithNotify(value, ref _branchName);
        }

        public GameBranch Branch
        {
            get => _branch;
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

        public SimpleRandom GenerationRandom
        {
            get => _generationRandom;
            set => SetWithNotify(value, ref _generationRandom);
        }

        public byte[] VisibleTerrain
        {
            get => _visibleTerrain;
            set => SetWithNotify(value, ref _visibleTerrain);
        }

        public byte[] VisibleNeighbours
        {
            get => _visibleNeighbours;
            set => SetWithNotify(value, ref _visibleNeighbours);
        }

        public byte[] Terrain
        {
            get => _terrain;
            set => SetWithNotify(value, ref _terrain);
        }

        public byte[] KnownTerrain
        {
            get => _knownTerrain;
            set => SetWithNotify(value, ref _knownTerrain);
        }

        // TODO: Track known neighbours as well
        public byte[] WallNeighbors
        {
            get => _wallNeighbors;
            set => SetWithNotify(value, ref _wallNeighbors);
        }

        // Untracked properties
        public byte[] VisibleTerrainSnapshot { get; set; }
        public Dictionary<int, byte> VisibleTerrainChanges { get; set; }
        public bool VisibleNeighboursChanged { get; set; }
        public Dictionary<int, byte> TerrainChanges { get; set; } = new Dictionary<int, byte>();
        public Dictionary<int, byte> KnownTerrainChanges { get; set; } = new Dictionary<int, byte>();
        public Dictionary<int, byte> WallNeighboursChanges { get; set; } = new Dictionary<int, byte>();

        public BeveledVisibilityCalculator VisibilityCalculator { get; private set; }
        public PathFinder PathFinder { get; private set; }
        public int[,] PointToIndex { get; private set; }
        public Point[] IndexToPoint { get; private set; }
        public Rectangle BoundingRectangle => new Rectangle(new Point(0, 0), Width, Height);

        public void EnsureInitialized()
        {
            if (PointToIndex == null
                && Width != 0)
            {
                (PointToIndex, IndexToPoint) = Rectangle.GetPointIndex(Game.Services.SharedCache, Width, Height);
                PathFinder = new PathFinder(PointToIndex, IndexToPoint);
                VisibilityCalculator = new BeveledVisibilityCalculator(GetVisibleNeighbours, this);
            }
        }

        private DirectionFlags GetVisibleNeighbours(byte x, byte y) => x < Width && y < Height
            ? (DirectionFlags)VisibleNeighbours[PointToIndex[x, y]]
            : DirectionFlags.None;

        public bool IsValid(Point point)
            => point.X < Width && point.Y < Height; // Since byte is unsigned there is no need to compare with 0

        protected override void Clean()
        {
            _branchName = default;
            _branch = default;
            _difficulty = default;
            _depth = default;
            _height = default;
            _width = default;
            _generationRandom = default;
            _visibleTerrain = default;
            _visibleNeighbours = default;
            _terrain = default;
            _knownTerrain = default;
            _wallNeighbors = default;

            base.Clean();
        }
    }
}
