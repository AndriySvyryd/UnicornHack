using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Generation.Map;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class Level
    {
        private PathFinder _pathFinder;
        private BeveledFOV _fov;

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string BranchName { get; private set; }
        public Branch Branch { get; set; }
        public byte Depth { get; set; }
        public int Difficulty { get; set; }
        public byte Height { get; set; }
        public byte Width { get; set; }
        public Rectangle BoundingRectangle => new Rectangle(new Point(0, 0), Width, Height);
        public byte[] VisibleTerrain { get; set; }
        public Dictionary<int, byte> VisibleTerrainChanges { get; set; } = new Dictionary<int, byte>();
        public byte[] VisibleNeighbours { get; set; }
        public bool VisibleNeighboursChanged { get; set; }
        public byte[] Terrain { get; set; }
        public Dictionary<int, byte> TerrainChanges { get; set; } = new Dictionary<int, byte>();
        public byte[] WallNeighbours { get; set; }
        public Dictionary<int, byte> WallNeighboursChanges { get; set; } = new Dictionary<int, byte>();
        public int NextRoomId { get; set; }
        public int CurrentTick { get; set; }
        public SimpleRandom GenerationRandom { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public virtual int GameId { get; private set; }

        public virtual Game Game { get; set; }
        public virtual ObservableSnapshotHashSet<Room> Rooms { get; } = new ObservableSnapshotHashSet<Room>();
        public virtual ObservableSnapshotHashSet<Item> Items { get; } = new ObservableSnapshotHashSet<Item>();
        public virtual PriorityQueue<Actor> Actors { get; } = new PriorityQueue<Actor>(Actor.TickComparer.Instance);
        public virtual ObservableSnapshotHashSet<Connection> Connections { get; } = new ObservableSnapshotHashSet<Connection>();
        public virtual ObservableSnapshotHashSet<Connection> IncomingConnections { get; } = new ObservableSnapshotHashSet<Connection>();
        public virtual IEnumerable<Player> Players => Actors.OfType<Player>();

        public int[,] PointToIndex { get; private set; }
        public Point[] IndexToPoint { get; private set; }

        public static readonly byte MaxDifficulty = 30;

        // Order matters, see Direction.cs
        public static readonly Vector[] MovementDirections =
        {
            new Vector(x: 1, y: 0), new Vector(x: 1, y: -1), new Vector(x: 0, y: -1), new Vector(x: -1, y: -1),
            new Vector(x: -1, y: 0), new Vector(x: -1, y: 1), new Vector(x: 0, y: 1), new Vector(x: 1, y: 1)
        };

        public static readonly byte[] OppositeDirectionIndexes = {4, 5, 6, 7, 0, 1, 2, 3};

        public Level()
        {
        }

        public Level(Branch branch, byte depth, int seed) : this()
        {
            Game = branch.Game;
            Game.Levels.Add(this);
            Branch = branch;
            branch.Levels.Add(this);
            Depth = depth;
            if (branch.Length < depth)
            {
                throw new InvalidOperationException("Level created beyond branch length");
            }
            Game.Repository.Add(this);
            Terrain = new byte[0];
            WallNeighbours = new byte[0];
            VisibleTerrain = new byte[0];
            VisibleNeighbours = new byte[0];
            GenerationRandom = new SimpleRandom {Seed = seed};
            Difficulty = branch.Difficulty + depth;
            if (Difficulty > MaxDifficulty)
            {
                throw new InvalidOperationException($"Difficulty {Difficulty} greater than max {MaxDifficulty}");
            }
        }

        public void EnsureInitialized()
        {
            if (_pathFinder == null)
            {
                (PointToIndex, IndexToPoint) = Game.GetPointIndex(Width, Height);
                _pathFinder = new PathFinder(CanMoveTo, PointToIndex, IndexToPoint);
                _fov = new BeveledFOV(BlocksLight, GetVisibleNeighbours);
            }
        }

        public bool EnsureGenerated()
        {
            if (Width != 0)
            {
                return false;
            }

            try
            {
                // TODO: Log parameters if failed
                var fragment = GenerationRandom.Pick(DefiningMapFragment.Loader.GetAsList(),
                    f => f.GetWeight(BranchName, Depth));

                Height = fragment.LevelHeight;
                Width = fragment.LevelWidth;

                Terrain = new byte[Height * Width];
                WallNeighbours = new byte[Height * Width];
                VisibleTerrain = new byte[Height * Width];
                VisibleNeighbours = new byte[Height * Width];

                EnsureInitialized();

                fragment.Layout.Fill(this, fragment);
                fragment.CreatureGenerator.Fill(this);
                fragment.ItemGenerator.Fill(this);

                return true;
            }
            catch (Exception e)
            {
                var msg = $"Error while generating level '{BranchName}:{Depth}', initial seed {Game.InitialSeed}:\r\n";

                Console.WriteLine(msg);
                Console.WriteLine(e);
                throw new Exception(msg, e);
            }
        }

        public void IncrementInstanceCounts(MapFragment fragment)
        {
            // Increment fragment instance count on level, branch, game
            // Increment each tag instance count on level, branch, game
        }

        public void SetTerrain(Point point, byte feature)
        {
            var index = PointToIndex[point.X, point.Y];
            Terrain[index] = feature;
            if (TerrainChanges != null)
            {
                TerrainChanges[index] = feature;
            }
        }

        public void AddNeighbours(MapFeature feature, Point point)
        {
            switch (feature)
            {
                case MapFeature.Pool:
                case MapFeature.RockFloor:
                case MapFeature.StoneFloor:
                    ModifyNeighbours(VisibleNeighbours, null, point, add: true);
                    VisibleNeighboursChanged = true;
                    break;
                case MapFeature.StoneArchway:
                case MapFeature.StoneWall:
                    ModifyNeighbours(WallNeighbours, WallNeighboursChanges, point, add: true);
                    break;
            }
        }

        private void ModifyNeighbours(byte[] neighbours, Dictionary<int, byte> changes, Point point, bool add)
        {
            for (var directionIndex = 0; directionIndex < 8; directionIndex++)
            {
                var direction = MovementDirections[directionIndex];
                var newLocation = point.Translate(direction);

                if (!IsValid(newLocation))
                {
                    continue;
                }

                var newLocationIndex = PointToIndex[newLocation.X, newLocation.Y];
                var neighbourBit = (byte)(1 << OppositeDirectionIndexes[directionIndex]);
                if (add)
                {
                    neighbours[newLocationIndex] |= neighbourBit;
                }
                else
                {
                    neighbours[newLocationIndex] &= (byte)~neighbourBit;
                }

                if (changes != null)
                {
                    changes[newLocationIndex] = neighbours[newLocationIndex];
                }
            }
        }

        public virtual Actor Turn()
        {
            EnsureInitialized();

            while (Actors.Count > 0)
            {
                var actor = Actors.Peek();
                if (actor.NextActionTick > Game.NextPlayerTick)
                {
                    // Avoid infinite loop if no players are present
                    break;
                }
                Debug.Assert(actor.Level == this && actor.IsAlive);

                CurrentTick = actor.NextActionTick;
                if (!actor.Act())
                {
                    return actor;
                }

                if (actor == Actors.Peek())
                {
                    if (CurrentTick == actor.NextActionTick)
                    {
                        Debug.Assert(false, nameof(Actor.NextActionTick) + " hasn't been updated!");
                        actor.NextActionTick += Actor.DefaultActionDelay;
                    }

                    var position = 0;
                    while (Actors.Count > position + 1 && actor.NextActionTick == Actors[position + 1].NextActionTick)
                    {
                        // Make sure actors alternate if they were to act at the same time
                        actor.NextActionTick++;

                        position = Actors.Update(position);
                    }

                    if (position == 0)
                    {
                        Actors.Update(position);
                    }
                }
            }

            return null;
        }

        public static byte GridDistance(Actor origin, Actor target)
        {
            var xDelta = target.LevelX - origin.LevelX;
            var yDelta = target.LevelY - origin.LevelY;
            return (byte)Math.Max(Math.Abs(xDelta), Math.Abs(yDelta));
        }

        public virtual Direction? GetFirstStepFromShortestPath(Actor origin, Actor target)
        {
            var firstPoint = new Point(origin.LevelX, origin.LevelY);
            var nextPoint = new Point(target.LevelX, target.LevelY);

            var path = GetShortestPath(origin, target);
            if (path == null)
            {
                return null;
            }

            if (path.Count != 0)
            {
                nextPoint = path[path.Count - 1];
            }

            Debug.Assert(firstPoint.DistanceTo(nextPoint) <= 1);

            return firstPoint.DirectionTo(nextPoint).AsDirection();
        }

        public List<Point> GetShortestPath(Actor origin, Actor target)
        {
            var firstPoint = new Point(origin.LevelX, origin.LevelY);
            var lastPoint = new Point(target.LevelX, target.LevelY);

            return GetShortestPath(firstPoint, lastPoint, origin.Heading);
        }

        public List<Point> GetShortestPath(Point start, Point target, Direction initialDirection) =>
            _pathFinder.FindPath(start, target, initialDirection);

        private int? CanMoveTo(byte currentLocationX, byte currentLocationY, int directionIndex)
        {
            var direction = MovementDirections[directionIndex];
            var newLocationX = (byte)(currentLocationX + direction.X);
            var newLocationY = (byte)(currentLocationY + direction.Y);

            if (newLocationX >= Width || newLocationY >= Height)
            {
                return null;
            }

            var newLocationIndex = PointToIndex[newLocationX, newLocationY];
            return ((MapFeature)Terrain[newLocationIndex]).CanMoveTo() ? (int?)newLocationIndex : null;
        }

        public bool IsValid(Point point) =>
            point.X < Width && point.Y < Height; // Since byte is unsigned there is no need to compare with 0

        // TODO: Use locomotion type
        // TODO: block if directionIndex > 3 (diagonal) and path is too narrow to squeeze through
        // TODO: Also avoid actors (at least adjacent ones)
        public bool CanMoveTo(Point location) =>
            IsValid(location) && ((MapFeature)Terrain[PointToIndex[location.X, location.Y]]).CanMoveTo();

        public int? CanMoveTo(byte locationX, byte locationY)
        {
            if (locationX >= Width || locationY >= Height)
            {
                return null;
            }

            var index = PointToIndex[locationX, locationY];
            return ((MapFeature)Terrain[index]).CanMoveTo() ? (int?)index : null;
        }

        public void RecomputeVisibility(Point location, byte visibilityFalloff)
        {
            var oldVisibleTerrain = VisibleTerrain;
            VisibleTerrain = new byte[VisibleTerrain.Length];
            _fov.Compute(location, 24, visibilityFalloff);
            DetectVisibilityChanges(oldVisibleTerrain);
        }

        public void RecomputeVisibility(Point location, Direction heading, byte primaryFOV, byte secondaryFOV)
        {
            var oldVisibleTerrain = VisibleTerrain;
            VisibleTerrain = new byte[VisibleTerrain.Length];
            _fov.Compute(location, heading, primaryFOV, 16, secondaryFOV, 8);
            DetectVisibilityChanges(oldVisibleTerrain);
        }

        private void DetectVisibilityChanges(byte[] oldVisibleTerrain)
        {
            if (VisibleTerrainChanges != null)
            {
                VisibleTerrainChanges.Clear();
                for (int i = 0; i < VisibleTerrain.Length; i++)
                {
                    var newValue = VisibleTerrain[i];
                    if (newValue != oldVisibleTerrain[i])
                    {
                        VisibleTerrainChanges.Add(i, newValue);
                    }
                }
            }
        }

        private bool BlocksLight(byte x, byte y, byte visibility, int rangeFalloff)
        {
            if (x < Width && y < Height)
            {
                switch ((MapFeature)Terrain[PointToIndex[x, y]])
                {
                    // This is inlined for perf
                    case MapFeature.Default:
                    case MapFeature.StoneWall:
                    case MapFeature.RockWall:
                        visibility = byte.MaxValue;
                        visibility = visibility < rangeFalloff ? (byte)0 : (byte)(visibility - rangeFalloff);
                        VisibleTerrain[PointToIndex[x, y]] = visibility;
                        return true;
                    default:
                        visibility = visibility < rangeFalloff ? (byte)0 : (byte)(visibility - rangeFalloff);
                        VisibleTerrain[PointToIndex[x, y]] = visibility;
                        return false;
                }
            }

            return true;
        }

        private DirectionFlags GetVisibleNeighbours(byte x, byte y) => x < Width && y < Height
            ? (DirectionFlags)VisibleNeighbours[PointToIndex[x, y]]
            : DirectionFlags.None;

        public bool CanPlaceCorridor(Point location)
        {
            var feature = (MapFeature)Terrain[PointToIndex[location.X, location.Y]];
            switch (feature)
            {
                case MapFeature.Default:
                case MapFeature.RockFloor:
                case MapFeature.StoneFloor:
                case MapFeature.RockWall:
                    return true;
                default:
                    return false;
            }
        }

        public virtual IReadOnlyList<Direction> GetPossibleMovementDirections(Point currentLocation, bool safe)
        {
            var availableDirections = new List<Direction>();
            for (var i = 0; i < 8; i++)
            {
                if (CanMoveTo(currentLocation.X, currentLocation.Y, i) == null)
                {
                    continue;
                }

                var direction = MovementDirections[i];
                if (Actors.Any(a =>
                    a.LevelX == currentLocation.X + direction.X && a.LevelY == currentLocation.Y + direction.Y))
                {
                    continue;
                }

                availableDirections.Add((Direction)i);
            }

            return availableDirections;
        }

        public virtual bool TryAdd(Item item, byte x, byte y)
        {
            if (!CanAdd(item, x, y))
            {
                return false;
            }

            var itemOrStack = item.StackWith(Items.Where(i => i.LevelX == x && i.LevelY == y));
            if (itemOrStack != null)
            {
                itemOrStack.BranchName = BranchName;
                itemOrStack.LevelDepth = Depth;
                itemOrStack.Level = this;
                itemOrStack.LevelX = x;
                itemOrStack.LevelY = y;
                Items.Add(itemOrStack);
                itemOrStack.AddReference();
            }

            return true;
        }

        public virtual bool CanAdd(Item item, byte x, byte y) => true;

        public virtual bool Remove(Item item)
        {
            item.BranchName = null;
            item.LevelDepth = null;
            item.Level = null;
            item.LevelX = 0;
            item.LevelY = 0;
            if (Items.Remove(item))
            {
                item.RemoveReference();
                return true;
            }
            return false;
        }
    }
}