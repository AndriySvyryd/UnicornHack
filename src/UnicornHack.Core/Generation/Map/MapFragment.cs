using System;
using System.Collections.Generic;
using CSharpScriptSerialization;
using UnicornHack.Utils;

namespace UnicornHack.Generation.Map
{
    public class MapFragment : ICSScriptSerializable, ILoadable
    {
        #region State

        public virtual string Name { get; set; }
        public virtual string Map { get; set; } = "";
        public virtual DynamicMap DynamicMap { get; set; }
        public virtual byte[] ByteMap { get; set; }
        public virtual byte Width { get; set; }
        public virtual byte Height { get; set; }
        public virtual Rectangle PayloadArea { get; set; }
        public virtual Weight GenerationWeight { get; set; }
        public virtual ICollection<string> Tags { get; set; }

        public virtual bool NoRandomDoorways { get; set; }
        // TODO: properties: NoMirror (for reflection symmetry, only 3 turns),
        // NoTurnOver(for rotational symmetry, only mirror and half-turn),
        // NoTransform(implies previous two), NoOverwrite

        public virtual bool ConditionalFirstRow { get; set; }
        public virtual bool ConditionalLastRow { get; set; }
        public virtual bool ConditionalFirstColumn { get; set; }
        public virtual bool ConditionalLastColumn { get; set; }

        public virtual int[,] PointToIndex { get; private set; }
        public virtual Point[] IndexToPoint { get; private set; }

        #endregion

        #region Creation

        // Characters that can be used as conditions for neighbors:
        // ~ - marks the edge row and/or column as conditional
        // X - should be outside the level
        // # - wall or other unpassable terrain
        // . - floor or other passable terrain
        void ILoadable.OnLoad()
        {
        }

        public virtual void EnsureInitialized(Game game)
        {
            // TODO: make this thread-safe
            if (ByteMap == null)
            {
                byte x = 0, y = 0;
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < Map.Length; i++)
                {
                    switch (Map[i])
                    {
                        case '\r':
                            continue;
                        case '\n':
                            if (x == 0 && y == 0)
                            {
                                continue;
                            }
                            if (x > Width)
                            {
                                Width = x;
                            }
                            x = 0;
                            y++;
                            continue;
                        default:
                            x++;
                            break;
                    }
                }

                if (x > 0)
                {
                    if (x > Width)
                    {
                        Width = x;
                    }
                    y++;
                }

                Height = y;

                (PointToIndex, IndexToPoint) = game.GetPointIndex(Width, Height);

                x = 0;
                y = 0;
                ByteMap = new byte[Width * Height];
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < Map.Length; i++)
                {
                    var character = Map[i];
                    switch (character)
                    {
                        case '\r':
                            continue;
                        case '\n':
                            if (x == 0 && y == 0)
                            {
                                continue;
                            }
                            while (x != Width)
                            {
                                ByteMap[PointToIndex[x, y]] = (byte)' ';
                                x++;
                            }
                            x = 0;
                            y++;
                            continue;
                        case '~':
                            if (x == 0)
                            {
                                ConditionalFirstColumn = true;
                            }
                            else if (x == Width - 1)
                            {
                                ConditionalLastColumn = true;
                            }
                            else if (y != 0 && y != Height - 1)
                            {
                                throw new InvalidOperationException(
                                    $"{Name}: '~' can only be either the first or the last character in a row if not in the first or last row.");
                            }

                            if (y == 0)
                            {
                                ConditionalFirstRow = true;
                            }
                            else if (y == Height - 1)
                            {
                                ConditionalLastRow = true;
                            }

                            ByteMap[PointToIndex[x, y]] = (byte)character;
                            x++;
                            break;
                        default:
                            ByteMap[PointToIndex[x, y]] = (byte)character;
                            x++;
                            break;
                    }
                }

                if (Width == 0)
                {
                    return;
                }

                if (x > 0)
                {
                    while (x != Width)
                    {
                        ByteMap[x + Width * y] = (byte)' ';
                        x++;
                    }
                }
                var payloadOrigin = new Point((byte)(ConditionalFirstColumn ? 1 : 0),
                    (byte)(ConditionalFirstRow ? 1 : 0));
                PayloadArea = new Rectangle(
                    payloadOrigin,
                    (byte)(Width - payloadOrigin.X - (ConditionalLastColumn ? 1 : 0)),
                    (byte)(Height - payloadOrigin.Y - (ConditionalLastRow ? 1 : 0)));
            }

            if (PointToIndex == null)
            {
                (PointToIndex, IndexToPoint) = game.GetPointIndex(Width, Height);
            }
        }

        #endregion

        #region Actions

        private Func<string, byte, int, int, float> _weightFunction;

        public virtual float GetWeight(Level level, Rectangle boundingRectangle)
        {
            // TODO: take transformations into account
            if (PayloadArea.Width > boundingRectangle.Width
                || PayloadArea.Height > boundingRectangle.Height)
            {
                return 0;
            }

            if (_weightFunction == null)
            {
                _weightFunction = GenerationWeight.CreateFragmentWeightFunction();
            }

            return _weightFunction(level.Branch.Name, level.Depth, 0, 0);
        }

        public Room TryPlace(Level level, Rectangle boundingRectangle)
        {
            try
            {
                EnsureInitialized(level.Game);

                if (ByteMap.Length != 0)
                {
                    return TryPlaceImplementation(level, boundingRectangle);
                }

                return TryPlace(level, boundingRectangle, DynamicMap);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error while placing fragment " + Name, ex);
            }
        }

        protected virtual Room TryPlaceImplementation(Level level, Rectangle boundingRectangle)
        {
            // TODO: take transformations into account
            var target = boundingRectangle.PlaceInside(PayloadArea, level.GenerationRandom);
            if (!target.HasValue)
            {
                return null;
            }

            var doorwayPoints = new List<Point>();
            var points = new List<Point>();
            WriteMap(target.Value, (c, x, y) =>
            {
                var feature = MapFeature.Default;
                switch (c)
                {
                    case '.':
                        feature = MapFeature.StoneFloor;
                        goto case 'X';
                    case ',':
                        feature = MapFeature.RockFloor;
                        goto case 'X';
                    case '?':
                        feature = MapFeature.StoneFloor;
                        doorwayPoints.Add(new Point(x, y));
                        goto case 'X';
                    case '#':
                        feature = MapFeature.StoneWall;
                        level.AddNeighbours(feature, x, y);
                        goto case 'X';
                    case '=':
                        feature = MapFeature.Pool;
                        goto case 'X';
                    case '<':
                    case '{':
                    case '[':
                    case '>':
                    case '}':
                    case ']':
                        feature = MapFeature.StoneFloor;
                        CreateConnection(level, new Point(x, y), c);
                        goto case 'X';
                    case '$':
                        feature = MapFeature.StoneFloor;
                        Item.Get("gold coin").Instantiate(new LevelCell(level, x, y), quantity: 9);
                        goto case 'X';
                    case '%':
                        feature = MapFeature.StoneFloor;
                        Item.Get("carrot").Instantiate(new LevelCell(level, x, y));
                        goto case 'X';
                    case ')':
                        feature = MapFeature.StoneFloor;
                        goto case 'X';
                    case '(':
                        feature = MapFeature.StoneFloor;
                        goto case 'X';
                    case 'b':
                        feature = MapFeature.StoneFloor;
                        Creature.Get("lightning bug").Instantiate(level, x, y);
                        goto case 'X';
                    case 'B':
                        feature = MapFeature.StoneFloor;
                        Creature.Get("firefly").Instantiate(level, x, y);
                        goto case 'X';
                    case 'X':
                        points.Add(new Point(x, y));
                        break;
                    case ' ':
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported map character '{c}' at {x},{y}");
                }

                level.Terrain[level.PointToIndex[x, y]] = (byte)feature;
            });

            return BuildRoom(level, points,
                NoRandomDoorways ? null : doorwayPoints,
                new Rectangle(target.Value, PayloadArea.Width, PayloadArea.Height), null, null, null);
        }

        protected virtual void CreateConnection(Level level, Point point, char? glyph)
            => throw new InvalidOperationException("Only " + nameof(ConnectingMapFragment) + " or "
                                                   + nameof(EncompassingMapFragment) + " can have a connection.");

        protected virtual Room TryPlace(Level level, Rectangle boundingRectangle, DynamicMap map)
        {
            if (map == null
                || !boundingRectangle.Contains(map.MinSize))
            {
                return null;
            }

            // TODO: Read the defaults from the encompassing fragment
            var room = BuildRoom(level, map.GetRoomPoints(level, boundingRectangle),
                p => { level.Terrain[level.PointToIndex[p.X, p.Y]] = (byte)MapFeature.StoneFloor; },
                p =>
                {
                    level.Terrain[level.PointToIndex[p.X, p.Y]] = (byte)MapFeature.StoneWall;
                    level.AddNeighbours(MapFeature.StoneWall, p.X, p.Y);
                },
                p => { });

            return room;
        }

        public void WriteMap(Point target, Action<char, byte, byte> write)
        {
            var map = ByteMap;
            var x = target.X;
            var y = target.Y;
            var lastFragmentY = 0;
            foreach (var fragmentPoint in PayloadArea)
            {
                var mapPoint = (char)map[PointToIndex[fragmentPoint.X, fragmentPoint.Y]];

                if (lastFragmentY != fragmentPoint.Y)
                {
                    lastFragmentY = fragmentPoint.Y;
                    x = target.X;
                    y++;
                }

                write(mapPoint, x, y);
                x++;
            }
        }

        public virtual Room BuildRoom(Level level, IEnumerable<Point> points,
            Action<Point> insideAction, Action<Point> perimeterAction, Action<Point> outsideAction)
            => BuildRoom(level, points, new List<Point>(), null, insideAction, perimeterAction, outsideAction);

        public virtual Room BuildRoom(Level level, IEnumerable<Point> points, List<Point> doorwayPoints,
            Rectangle? boundingRectangle,
            Action<Point> insideAction, Action<Point> perimeterAction, Action<Point> outsideAction)
        {
            void Noop(Point p)
            {
            }

            insideAction = insideAction ?? Noop;
            perimeterAction = perimeterAction ?? Noop;
            outsideAction = outsideAction ?? Noop;
            var neighbours = new byte[level.Width * level.Height];
            var firstPoint = new Point();
            var lastPoint = new Point();
            var left = byte.MaxValue;
            var right = byte.MinValue;
            var occupiedPointCount = 0;
            foreach (var roomPoint in points)
            {
                if (occupiedPointCount == 0)
                {
                    firstPoint = roomPoint;
                    right = firstPoint.X;
                    left = firstPoint.X;
                }
                else if (lastPoint.Y != roomPoint.Y)
                {
                    right = Math.Max(right, lastPoint.X);
                    left = Math.Min(left, roomPoint.X);
                }
                lastPoint = roomPoint;

                for (var directionIndex = 0; directionIndex < 8; directionIndex++)
                {
                    var direction = Level.MovementDirections[directionIndex];
                    var newLocationX = (byte)(roomPoint.X + direction.X);
                    var newLocationY = (byte)(roomPoint.Y + direction.Y);

                    if (newLocationX >= level.Width || newLocationY >= level.Height)
                    {
                        continue;
                    }

                    var newLocationIndex = level.PointToIndex[newLocationX, newLocationY];
                    var newNeighbours = (byte)(neighbours[newLocationIndex]
                                               | 1 << Level.OppositeDirectionIndexes[directionIndex]);
                    if (newNeighbours == Byte.MaxValue
                        && (neighbours[level.PointToIndex[roomPoint.X, roomPoint.Y]] & 1 << directionIndex) != 0)
                    {
                        insideAction.Invoke(new Point(newLocationX, newLocationY));
                    }

                    neighbours[newLocationIndex] = newNeighbours;
                }
                occupiedPointCount++;
            }

            if (occupiedPointCount == 0)
            {
                return null;
            }

            if (doorwayPoints != null && doorwayPoints.Count == 0)
            {
                var perimeter = WalkPerimeter(
                    firstPoint, neighbours, level.PointToIndex, perimeterAction, outsideAction);

                if (perimeter.Count > 7)
                {
                    doorwayPoints = FindDoorwayPoints(perimeter, level);
                }
            }

            return new Room(level,
                boundingRectangle ?? new Rectangle(new Point(left, firstPoint.Y), new Point(right, lastPoint.Y)),
                doorwayPoints);
        }

        public static List<Point> FindDoorwayPoints(IReadOnlyList<Point> perimeter, Level level,
            bool allowCorners = false)
        {
            var doorwayPoints = new List<Point>();
            for (var i = 0; i < perimeter.Count; i++)
            {
                var previousPoint = perimeter[i == 0 ? perimeter.Count - 1 : i - 1];
                var nextPoint = perimeter[i == perimeter.Count - 1 ? 0 : i + 1];
                var direction = previousPoint.DirectionTo(nextPoint).GetUnit();
                if (!allowCorners
                    && direction.X != 0
                    && direction.Y != 0)
                {
                    continue;
                }

                var firstOrthogonal = direction.GetOrthogonal();
                var secondOrthogonal = firstOrthogonal.GetInverse();
                var point = perimeter[i];

                var corridorCandidatePoint = point.Translate(firstOrthogonal);
                if (!level.IsValid(corridorCandidatePoint)
                    || !level.CanPlaceCorridor(corridorCandidatePoint))
                {
                    continue;
                }

                var doorwayCandidatePoint = point.Translate(secondOrthogonal);
                if (!level.IsValid(doorwayCandidatePoint)
                    || !level.CanPlaceCorridor(doorwayCandidatePoint))
                {
                    continue;
                }

                doorwayPoints.Add(point);
            }
            return doorwayPoints;
        }

        private static IReadOnlyList<Point> WalkPerimeter(
            Point firstPoint, byte[] neighbours, int[,] pointToIndex,
            Action<Point> perimeterAction, Action<Point> outsideAction)
            => WalkPerimeter(
                   new Point(firstPoint.X, (byte)(firstPoint.Y - 1)),
                   DirectionFlags.South,
                   Direction.North,
                   new Dictionary<Point, int>(),
                   neighbours,
                   pointToIndex,
                   perimeterAction,
                   outsideAction)
               ?? new List<Point>();

        private static List<Point> WalkPerimeter(
            Point firstPoint,
            DirectionFlags currentNeighbourMap,
            Direction previousPointDirection,
            Dictionary<Point, int> visitedIntersections,
            byte[] neighbours,
            int[,] pointToIndex,
            Action<Point> perimeterAction,
            Action<Point> outsideAction)
        {
            // Assuming both the perimeter and the inner area are contiguos
            List<Point> perimeter = null;
            var currentPoint = firstPoint;
            var previousPointDirectionIndex = (int)previousPointDirection;
            var perimeterLooped = false;

            for (var j = 0; j < neighbours.Length; j++)
            {
                var perimeterPointFound = false;
                for (var i = 1; i < 4; i++)
                {
                    var directionIndexToCheck = (previousPointDirectionIndex + i) % 4;
                    if (currentNeighbourMap.HasFlag((DirectionFlags)(1 << directionIndexToCheck)))
                    {
                        var directionToCheck = Level.MovementDirections[directionIndexToCheck];
                        var newLocationX = (byte)(currentPoint.X + directionToCheck.X);
                        var newLocationY = (byte)(currentPoint.Y + directionToCheck.Y);
                        var nextPoint = new Point(newLocationX, newLocationY);
                        if (nextPoint.Equals(firstPoint))
                        {
                            break;
                        }

                        var nextNeighbourMap = (DirectionFlags)neighbours[pointToIndex[newLocationX, newLocationY]];
                        var nextIsPerimeter = IsPerimeter(nextNeighbourMap);
                        if (nextIsPerimeter)
                        {
                            if (perimeterLooped)
                            {
                                break;
                            }
                            if (perimeter == null)
                            {
                                perimeter = new List<Point>();
                            }
                            if (perimeter.Count == 0
                                || !nextPoint.Equals(perimeter[0]))
                            {
                                perimeter.Add(nextPoint);
                                perimeterAction.Invoke(nextPoint);
                            }
                            else
                            {
                                perimeterLooped = true;
                            }

                            currentPoint = nextPoint;
                            currentNeighbourMap = nextNeighbourMap;
                            previousPointDirectionIndex = Level.OppositeDirectionIndexes[directionIndexToCheck];
                            perimeterPointFound = true;
                            break;
                        }

                        if (visitedIntersections.TryGetValue(nextPoint, out var visitedDirection))
                        {
                            continue;
                        }
                        visitedIntersections[nextPoint] = directionIndexToCheck;

                        outsideAction.Invoke(nextPoint);

                        var subPerimeter = WalkPerimeter(
                            nextPoint,
                            nextNeighbourMap,
                            (Direction)Level.OppositeDirectionIndexes[directionIndexToCheck],
                            visitedIntersections,
                            neighbours,
                            pointToIndex,
                            perimeterAction,
                            outsideAction);

                        if (subPerimeter != null)
                        {
                            if (perimeter != null)
                            {
                                throw new InvalidOperationException("Unconnected perimeters");
                            }

                            perimeter = subPerimeter;
                        }
                    }
                }

                if (!perimeterPointFound)
                {
                    return perimeter;
                }
            }

            throw new InvalidOperationException("Infinite loop");
        }

        private static bool IsPerimeter(DirectionFlags neighbourMap)
            => neighbourMap.HasFlag(DirectionFlags.NorthwestCorner)
               || neighbourMap.HasFlag(DirectionFlags.NortheastCorner)
               || neighbourMap.HasFlag(DirectionFlags.SoutheastCorner)
               || neighbourMap.HasFlag(DirectionFlags.SouthwestCorner);

        #endregion

        #region Serialization

        public static readonly CSScriptLoader<MapFragment> NormalLoader =
            new CSScriptLoader<MapFragment>(@"data\fragments\normal\");

        public static IReadOnlyList<MapFragment> GetAllNormalMapFragments() => NormalLoader.GetAll();

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<MapFragment>(GetPropertyConditions<MapFragment>());

        protected static Dictionary<string, Func<TMapFragment, object, bool>> GetPropertyConditions<TMapFragment>()
            where TMapFragment : MapFragment
        {
            return new Dictionary<string, Func<TMapFragment, object, bool>>
            {
                {nameof(Name), (o, v) => v != null},
                {nameof(GenerationWeight), (o, v) => (Weight)v != null},
                {nameof(Map), (o, v) => !string.IsNullOrEmpty((string)v)},
                {nameof(ByteMap), (o, v) => false},
                {nameof(PayloadArea), (o, v) => false},
                {nameof(Width), (o, v) => false},
                {nameof(Height), (o, v) => false},
                {nameof(ConditionalFirstColumn), (o, v) => false},
                {nameof(ConditionalFirstRow), (o, v) => false},
                {nameof(ConditionalLastColumn), (o, v) => false},
                {nameof(ConditionalLastRow), (o, v) => false}
            };
        }

        public virtual ICSScriptSerializer GetSerializer() => Serializer;

        #endregion
    }
}