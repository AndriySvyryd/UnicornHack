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

        public virtual int Id { get; private set; }
        public virtual string Name { get; set; }
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

        public virtual ICollection<Stairs> UpStairs { get; private set; } = new HashSet<Stairs>();
        public virtual ICollection<Stairs> DownStairs { get; private set; } = new HashSet<Stairs>();
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

        public Level(Game game, string branchName, byte depth, byte height, byte width)
            : this()
        {
            Id = game.NextLevelId++;
            Game = game;
            Name = branchName;
            Depth = depth;
            Height = height;
            Width = width;
            Layout = new byte[height * width];
            WallNeighbours = new byte[height * width];

            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_pathFinder == null)
            {
                var lookupTables = Game.Services.SharedCache.GetOrCreate(Width << 8 + Height, e =>
                {
                    var pointToIndex = new int[Width, Height];
                    var indexToPoint = new Point[Width*Height];
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

        public static Level CreateLevel(Game game, string branchName, byte depth)
        {
            var fragment = (EncompassingMapFragment)MapFragment.Get("D" + depth);
            var height = fragment.LevelHeight;
            var width = fragment.LevelWidth;
            var level = new Level(game, branchName, depth, height, width);
            game.Levels.Add(level);
            level.PlaceFragment(fragment);

            return level;
        }

        private void PlaceFragment(MapFragment fragment)
        {
            if (fragment.PayloadArea.Width > Width)
            {
                throw new InvalidOperationException($"The fragment's width is {fragment.PayloadArea.Width}, but the level's is {Width}");
            }
            if (fragment.PayloadArea.Height > Height)
            {
                throw new InvalidOperationException($"The fragment's height is {fragment.PayloadArea.Height}, but the level's is {Height}");
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
                        UpStairs.Add(Stairs.CreateUpStairs(Game, this, x, y));
                        break;
                    case '>':
                        feature = MapFeature.Floor;
                        DownStairs.Add(Stairs.CreateDownStairs(Game, this, x, y));
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
                Layout[x + Width*y] = (byte)feature;
                x++;

                if (x == fragment.Width + originX)
                {
                    x = 0;
                    y++;
                }
            }
        }

        private void AddNeighbours(byte[] neighbours, byte x, byte y)
        {
            for (int directionIndex = 0; directionIndex < 8; directionIndex++)
            {
                var direction = MovementDirections[directionIndex];
                var newLocationX = (byte)(x + direction.X);
                var newLocationY = (byte)(y + direction.Y);

                if ((newLocationX >= Width) || (newLocationY >= Height))
                {
                    continue;
                }

                var newLocationIndex = _pointToIndex[newLocationX, newLocationY];
                neighbours[newLocationIndex] |= (byte)(1 << OppositeDirectionIndexes[directionIndex]);
            }
        }

        public Level CreateUpLevel(Stairs stairs)
        {
            var level = CreateLevel(Game, stairs.BranchName, (byte)(stairs.Down.Depth - 1));
            var connectingStairs = level.DownStairs.Single(s => s.BranchName == stairs.Down.Name);
            stairs.Up = level;
            stairs.UpLevelX = connectingStairs.UpLevelX;
            stairs.UpLevelY = connectingStairs.UpLevelY;
            level.DownStairs.Add(stairs);
            level.DownStairs.Remove(connectingStairs);
            return level;
        }

        public Level CreateDownLevel(Stairs stairs)
        {
            var level = CreateLevel(Game, stairs.BranchName, (byte)(stairs.Up.Depth + 1));
            var connectingStairs = level.UpStairs.Single(s => s.BranchName == stairs.Up.Name);
            stairs.Down = level;
            stairs.DownLevelX = connectingStairs.DownLevelX;
            stairs.DownLevelY = connectingStairs.DownLevelY;
            level.UpStairs.Add(stairs);
            level.UpStairs.Remove(connectingStairs);
            return level;
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

            if ((newLocationX >= Width) || (newLocationY >= Height))
            {
                return null;
            }

            var newLocationIndex = _pointToIndex[newLocationX, newLocationY];
            return CanMoveTo(newLocationIndex) ? (int?)newLocationIndex : null;
        }

        public bool CanMoveTo(Point newLocation)
        {
            // Since byte is unsigned there is no need to compare with 0
            if ((newLocation.X >= Width) || (newLocation.Y >= Height))
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
                    (a.LevelX == currentLocation.X + direction.X) &&
                    (a.LevelY == currentLocation.Y + direction.Y)))
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
                itemOrStack.LevelId = Id;
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
            item.LevelId = null;
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