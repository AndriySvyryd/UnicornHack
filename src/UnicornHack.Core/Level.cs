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
        #region State

        private PathFinder _pathFinder;
        private readonly Func<Point, int, int?> _canMoveDelegate;

        public virtual string BranchName { get; private set; }
        public virtual Branch Branch { get; set; }
        public virtual byte Depth { get; set; }
        public virtual int Difficulty { get; set; }
        public virtual byte Height { get; set; }
        public virtual byte Width { get; set; }
        public virtual Rectangle BoundingRectangle => new Rectangle(new Point(0, 0), Width, Height);
        public virtual byte[] Terrain { get; set; }
        public virtual byte[] WallNeighbours { get; set; }
        public virtual int NextRoomId { get; set; }
        public virtual int CurrentTick { get; set; }
        public virtual SimpleRandom GenerationRandom { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public virtual int GameId { get; private set; }

        public virtual Game Game { get; set; }
        public virtual ICollection<Room> Rooms { get; private set; } = new HashSet<Room>();
        public virtual ICollection<Item> Items { get; private set; } = new HashSet<Item>();

        public virtual PriorityQueue<Actor> Actors { get; private set; } =
            new PriorityQueue<Actor>(Actor.TickComparer.Instance);

        public virtual ICollection<Connection> Connections { get; private set; } = new HashSet<Connection>();
        public virtual ICollection<Connection> IncomingConnections { get; private set; } = new HashSet<Connection>();
        public virtual IEnumerable<Player> Players => Actors.OfType<Player>();

        public int[,] PointToIndex { get; private set; }
        public Point[] IndexToPoint { get; private set; }

        public static readonly byte MaxDifficulty = 30;

        // Order matters, see Direction.cs
        public static readonly Vector[] MovementDirections =
        {
            new Vector(x: 0, y: -1), new Vector(x: 1, y: 0), new Vector(x: 0, y: 1), new Vector(-1, y: 0),
            new Vector(x: -1, y: -1), new Vector(x: 1, y: -1), new Vector(x: 1, y: 1), new Vector(-1, y: 1)
        };

        public static readonly byte[] OppositeDirectionIndexes =
        {
            2, 3, 0, 1,
            6, 7, 4, 5
        };

        #endregion

        #region Creation

        public Level()
        {
            _canMoveDelegate = CanMove;
        }

        public Level(Branch branch, byte depth, int seed)
            : this()
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
                _pathFinder = new PathFinder(_canMoveDelegate, PointToIndex, IndexToPoint);
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
                var fragment = GenerationRandom.Pick(
                    DefiningMapFragment.GetAllDefiningMapFragments(), f => f.GetWeight(BranchName, Depth));

                Height = fragment.LevelHeight;
                Width = fragment.LevelWidth;

                Terrain = new byte[Height * Width];
                WallNeighbours = new byte[Height * Width];

                EnsureInitialized();

                fragment.Layout.Fill(this, fragment);
                fragment.CreatureGenerator.Fill(this);

                return true;
            }
            catch (Exception e)
            {
                var msg = $"Error while generating level, initial seed {Game.InitialSeed}";

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

        public void AddNeighbours(MapFeature feature, Point point)
        {
            switch (feature)
            {
                case MapFeature.StoneWall:
                    ModifyNeighbours(WallNeighbours, point, add: true);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported feature for neighbour tracking: {feature}");
            }
        }

        public void RemoveNeighbours(MapFeature feature, Point point)
        {
            switch (feature)
            {
                case MapFeature.StoneWall:
                    ModifyNeighbours(WallNeighbours, point, add: false);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported feature for neighbour tracking: {feature}");
            }
        }

        private void ModifyNeighbours(byte[] neighbours, Point point, bool add)
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
            }
        }

        #endregion

        #region Actions

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
                    while (Actors.Count > position + 1
                           && actor.NextActionTick == Actors[position + 1].NextActionTick)
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

        public virtual Vector? GetFirstStepFromShortestPath(Actor origin, Actor target)
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

            return firstPoint.DirectionTo(nextPoint);
        }

        public virtual List<Point> GetShortestPath(Actor origin, Actor target)
        {
            var firstPoint = new Point(origin.LevelX, origin.LevelY);
            var nextPoint = new Point(target.LevelX, target.LevelY);

            return _pathFinder.FindPath(firstPoint, nextPoint);
        }

        private int? CanMove(Point currentLocation, int directionIndex)
        {
            var direction = MovementDirections[directionIndex];
            var newLocationX = (byte)(currentLocation.X + direction.X);
            var newLocationY = (byte)(currentLocation.Y + direction.Y);

            if (newLocationX >= Width || newLocationY >= Height)
            {
                return null;
            }

            var newLocationIndex = PointToIndex[newLocationX, newLocationY];
            return CanMoveTo(newLocationIndex) ? (int?)newLocationIndex : null;
        }

        public bool IsValid(Point point)
            => point.X < Width && point.Y < Height; // Since byte is unsigned there is no need to compare with 0

        public bool CanMoveTo(Point location)
            => IsValid(location)
               && CanMoveTo(PointToIndex[location.X, location.Y]);

        private bool CanMoveTo(int locationIndex)
        {
            // TODO: Use locomotion type
            // TODO: block if directionIndex > 3 (diagonal) and path is too narrow to squeeze through
            // TODO: Also avoid actors (at least adjacent ones)
            return ((MapFeature)Terrain[locationIndex]).CanMoveTo();
        }

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

        public virtual IReadOnlyList<Vector> GetPossibleMovementDirections(Point currentLocation, bool safe)
        {
            var availableDirections = new List<Vector>();
            for (var i = 0; i < 8; i++)
            {
                if (CanMove(currentLocation, i) == null)
                {
                    continue;
                }

                var direction = MovementDirections[i];
                if (Actors.Any(a =>
                    a.LevelX == currentLocation.X + direction.X &&
                    a.LevelY == currentLocation.Y + direction.Y))
                {
                    continue;
                }

                availableDirections.Add(MovementDirections[i]);
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

        public virtual bool CanAdd(Item item, byte x, byte y)
            => true;

        public virtual bool Remove(Item item)
        {
            item.BranchName = null;
            item.LevelDepth = null;
            item.Level = null;
            item.LevelX = null;
            item.LevelY = null;
            if (Items.Remove(item))
            {
                item.RemoveReference();
                return true;
            }
            return false;
        }

        #endregion
    }
}