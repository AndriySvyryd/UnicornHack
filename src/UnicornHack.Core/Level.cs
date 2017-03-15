using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using UnicornHack.Generation.Map;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class Level
    {
        #region State

        private int[,] _pointToIndex;
        private Point[] _indexToPoint;
        private PathFinder _pathFinder;
        private readonly Func<Point, int, int?> _canMoveDelegate;

        public virtual string BranchName { get; private set; }
        public virtual Branch Branch { get; set; }
        public virtual byte Depth { get; set; }
        public byte Height { get; set; }
        public byte Width { get; set; }
        public virtual byte[] Layout { get; set; }
        public virtual byte[] WallNeighbours { get; set; }

        public virtual int CurrentTick { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public virtual int GameId { get; private set; }
        public virtual Game Game { get; set; }
        public virtual ICollection<Item> Items { get; private set; } = new HashSet<Item>();

        public virtual PriorityQueue<Actor> Actors { get; private set; } =
            new PriorityQueue<Actor>(Actor.TickComparer.Instance);

        public virtual ICollection<Stairs> Stairs { get; private set; } = new HashSet<Stairs>();
        public virtual ICollection<Stairs> IncomingStairs { get; private set; } = new HashSet<Stairs>();
        public virtual IEnumerable<Player> Players => Actors.OfType<Player>();

        // Order matters
        public static readonly Vector[] MovementDirections =
        {
            new Vector(x: 0, y: -1), new Vector(x: 1, y: 0),
            new Vector(x: 0, y: 1), new Vector(-1, y: 0),
            new Vector(x: -1, y: -1), new Vector(x: 1, y: -1),
            new Vector(x: 1, y: 1), new Vector(-1, y: 1)
        };

        public static readonly byte[] OppositeDirectionIndexes =
        {
            2, 3,
            0, 1,
            6, 7,
            4, 5
        };

        #endregion

        #region Creation

        public Level()
        {
            _canMoveDelegate = CanMove;
        }

        public Level(Branch branch, byte depth)
            : this()
        {
            BranchName = branch.Name;
            Game = branch.Game;
            Game.Levels.Add(this);
            Branch = branch;
            branch.Levels.Add(this);
            Depth = depth;
            Layout = new byte[0];
            WallNeighbours = new byte[0];
        }

        private void EnsureInitialized()
        {
            if (_pathFinder == null)
            {
                var lookupTables = Game.Services.SharedCache.GetOrCreate(
                    typeof(Level).GetHashCode() ^ (Width << 8 + Height), e =>
                    {
                        var pointToIndex = new int[Width, Height];
                        var indexToPoint = new Point[Width * Height];
                        var i = 0;
                        for (byte y = 0; y < Height; y++)
                        {
                            for (byte x = 0; x < Width; x++)
                            {
                                pointToIndex[x, y] = i;
                                indexToPoint[i++] = new Point(x, y);
                            }
                        }

                        return Tuple.Create(pointToIndex, indexToPoint);
                    });

                _pointToIndex = lookupTables.Item1;
                _indexToPoint = lookupTables.Item2;
                _pathFinder = new PathFinder(_canMoveDelegate, _pointToIndex, _indexToPoint);
            }
        }

        // 1. Choose an encompassing fragment
        // 2. For every existing connection to this level generate a destination fragment.
        // 3. Choose up to 3 source fragments, depending on amount of space left to fill.
        //     At least 1 to the next branch level if not final or another branch if final.
        //     At most 1 per branch if it's not the current branch.
        // 4. Fill the layout cells depending on specified size, tag, number of connections, position on the map
        // 5. Fill the empty space with snapping fragments and connect them to ovewritable fragments

        public bool EnsureGenerated()
        {
            if (Width != 0
                || Depth == 0)
            {
                return false;
            }

            var fragment = Game.Pick(
                EncompassingMapFragment.GetAllEncompassingMapFragments().ToList(),
                f => f.GetWeight(BranchName, Depth));

            Height = fragment.LevelHeight;
            Width = fragment.LevelWidth;
            Layout = new byte[Height * Width];
            WallNeighbours = new byte[Height * Width];

            EnsureInitialized();

            PlaceFragment(fragment);

            return true;
        }

        private void PlaceFragment(MapFragment fragment)
        {
            if (fragment.PayloadArea.Width > Width)
            {
                throw new InvalidOperationException(
                    $"The fragment's width is {fragment.PayloadArea.Width}, but the level's is {Width}");
            }
            if (fragment.PayloadArea.Height > Height)
            {
                throw new InvalidOperationException(
                    $"The fragment's height is {fragment.PayloadArea.Height}, but the level's is {Height}");
            }

            var layout = fragment.ByteMap;
            byte originX = 0, originY = 0;
            byte x = originX, y = originY;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < layout.Length; i++)
            {
                var mapPoint = (char)layout[i];
                var feature = MapFeature.Default;
                switch (mapPoint)
                {
                    case ' ':
                        break;
                    case '.':
                        feature = MapFeature.Floor;
                        break;
                    case ',':
                        feature = MapFeature.Corridor;
                        break;
                    case '#':
                        feature = MapFeature.Wall;
                        AddNeighbours(WallNeighbours, x, y);
                        break;
                    case '=':
                        feature = MapFeature.Pool;
                        break;
                    case '<':
                        feature = MapFeature.Floor;
                        Stairs.Add(UnicornHack.Stairs.CreateUpStairs(Game, this, x, y));
                        break;
                    case '>':
                        feature = MapFeature.Floor;
                        Stairs.Add(UnicornHack.Stairs.CreateDownStairs(Game, this, x, y));
                        break;
                    case '$':
                        feature = MapFeature.Floor;
                        Item.Get("gold coin").Instantiate(new LevelCell(this, x, y), quantity: 9);
                        break;
                    case '%':
                        feature = MapFeature.Floor;
                        Item.Get("carrot").Instantiate(new LevelCell(this, x, y));
                        break;
                    case ')':
                        feature = MapFeature.Floor;
                        break;
                    case '[':
                        feature = MapFeature.Floor;
                        break;
                    case 'b':
                        feature = MapFeature.Floor;
                        Creature.Get("lightning bug").Instantiate(this, x, y);
                        break;
                    case 'B':
                        feature = MapFeature.Floor;
                        Creature.Get("firefly").Instantiate(this, x, y);
                        break;
                    case '~':
                    default:
                        throw new InvalidOperationException($"Unsupported map character '{mapPoint}' at {x},{y}");
                }
                Layout[x + Width * y] = (byte)feature;
                x++;

                if (x == fragment.Width + originX)
                {
                    x = 0;
                    y++;
                }
            }

            IncrementInstanceCounts(fragment);
        }

        private void IncrementInstanceCounts(MapFragment fragment)
        {
            // Increment fragment instance count on level, branch, game
            // Increment each tag instance count on level, branch, game
        }

        private void AddNeighbours(byte[] neighbours, byte x, byte y)
        {
            for (var directionIndex = 0; directionIndex < 8; directionIndex++)
            {
                var direction = MovementDirections[directionIndex];
                var newLocationX = (byte)(x + direction.X);
                var newLocationY = (byte)(y + direction.Y);

                if (newLocationX >= Width || newLocationY >= Height)
                {
                    continue;
                }

                var newLocationIndex = _pointToIndex[newLocationX, newLocationY];
                neighbours[newLocationIndex] |= (byte)(1 << OppositeDirectionIndexes[directionIndex]);
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

            var newLocationIndex = _pointToIndex[newLocationX, newLocationY];
            return CanMoveTo(newLocationIndex) ? (int?)newLocationIndex : null;
        }

        public bool CanMoveTo(Point newLocation)
        {
            // Since byte is unsigned there is no need to compare with 0
            if (newLocation.X >= Width || newLocation.Y >= Height)
            {
                return false;
            }

            var newLocationIndex = _pointToIndex[newLocation.X, newLocation.Y];
            return CanMoveTo(newLocationIndex);
        }

        private bool CanMoveTo(int newLocationIndex)
        {
            // TODO: Use locomotion type
            // TODO: block if directionIndex > 3 (diagonal) and path is too narrow to squeeze through
            // TODO: Also avoid actors (at least adjacent ones)
            return ((MapFeature)Layout[newLocationIndex]).CanMoveTo();
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
                itemOrStack.LevelName = BranchName;
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
            item.LevelName = null;
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