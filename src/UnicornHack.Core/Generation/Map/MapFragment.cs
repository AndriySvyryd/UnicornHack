using System;
using System.Collections.Generic;
using CSharpScriptSerialization;
using UnicornHack.Utils;

namespace UnicornHack.Generation.Map
{
    public abstract class MapFragment : ICSScriptSerializable, ILoadable
    {
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

        // Characters that can be used as conditions for neighbors:
        // ~ - marks the edge row and/or column as conditional
        // X - should be outside the level
        // # - wall or other unpassable terrain
        // . - floor or other passable terrain
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
                PayloadArea = new Rectangle(payloadOrigin,
                    (byte)(Width - payloadOrigin.X - (ConditionalLastColumn ? 1 : 0)),
                    (byte)(Height - payloadOrigin.Y - (ConditionalLastRow ? 1 : 0)));
            }

            if (PointToIndex == null)
            {
                (PointToIndex, IndexToPoint) = game.GetPointIndex(Width, Height);
            }
        }

        public Room TryPlace(Level level, Rectangle boundingRectangle)
        {
            try
            {
                EnsureInitialized(level.Game);

                var room = ByteMap.Length != 0
                    ? TryPlaceStaticMap(level, boundingRectangle)
                    : TryPlace(level, boundingRectangle, DynamicMap);

                if (room != null)
                {
                    level.Rooms.Add(room);
                }

                return room;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error while placing fragment " + Name, ex);
            }
        }

        protected virtual Room TryPlaceStaticMap(Level level, Rectangle boundingRectangle)
        {
            // TODO: take transformations into account
            var target = boundingRectangle.PlaceInside(PayloadArea, level.GenerationRandom);
            if (!target.HasValue)
            {
                throw new InvalidOperationException($"Couldn't fit fragment {Name} into {boundingRectangle}");
            }

            var doorwayPoints = new List<Point>();
            var perimeterPoints = new List<Point>();
            var insidePoints = new List<Point>();
            var points = new List<Point>();
            WriteMap(target.Value, level, Write, (doorwayPoints, perimeterPoints, insidePoints, points));

            if (!NoRandomDoorways && doorwayPoints.Count == 0)
            {
                // TODO: find doorway candidates in the perimeter walls
            }

            return new Room(level, new Rectangle(target.Value, PayloadArea.Width, PayloadArea.Height), doorwayPoints,
                insidePoints);
        }

        protected virtual void Write(char c, Point point, Level level,
            (List<Point> doorwayPoints, List<Point> perimeterPoints, List<Point> insidePoints, List<Point> points)
                state)
        {
            (List<Point> doorwayPoints, List<Point> perimeterPoints, List<Point> insidePoints, List<Point> points) =
                state;
            var feature = MapFeature.Default;
            switch (c)
            {
                case '.':
                    feature = MapFeature.StoneFloor;
                    insidePoints.Add(point);
                    goto case '\u0001';
                case ',':
                    feature = MapFeature.RockFloor;
                    insidePoints.Add(point);
                    goto case '\u0001';
                case '?':
                    feature = MapFeature.StoneFloor;
                    doorwayPoints.Add(point);
                    goto case '\u0001';
                case '#':
                    feature = MapFeature.StoneWall;
                    goto case '\u0001';
                case 'A':
                    feature = MapFeature.StoneArchway;
                    goto case '\u0001';
                case '=':
                    feature = MapFeature.Pool;
                    goto case '\u0001';
                case '\u0001':
                    points.Add(point);
                    break;
                case ' ':
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported map character '{c}' at {point.X},{point.Y}");
            }

            level.Terrain[level.PointToIndex[point.X, point.Y]] = (byte)feature;
            level.AddNeighbours(feature, point);
        }

        protected virtual Room TryPlace(Level level, Rectangle boundingRectangle, DynamicMap map)
        {
            if (map == null || !boundingRectangle.Contains(map.MinSize))
            {
                return null;
            }

            // TODO: Read the defaults from the defining fragment
            var room = BuildRoom(level, map.GetRoomPoints(level, boundingRectangle),
                p => { level.Terrain[level.PointToIndex[p.X, p.Y]] = (byte)MapFeature.StoneFloor; }, p =>
                {
                    level.Terrain[level.PointToIndex[p.X, p.Y]] = (byte)MapFeature.StoneWall;
                    level.AddNeighbours(MapFeature.StoneWall, p);
                }, p => { });

            return room;
        }

        public void WriteMap<TState>(Point target, Level level, Action<char, Point, Level, TState> write, TState state)
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

                write(mapPoint, new Point(x, y), level, state);
                x++;
            }
        }

        public virtual Room BuildRoom(Level level, IEnumerable<Point> points, Action<Point> insideAction,
            Action<Point> perimeterAction, Action<Point> outsideAction)
        {
            void Noop(Point p)
            {
            }

            var insidePoints = new List<Point>();
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
                    var newNeighbours = (byte)(neighbours[newLocationIndex] |
                                               1 << Level.OppositeDirectionIndexes[directionIndex]);
                    if (newNeighbours == (byte)DirectionFlags.Circle &&
                        (neighbours[level.PointToIndex[roomPoint.X, roomPoint.Y]] & 1 << directionIndex) != 0)
                    {
                        var insidePoint = new Point(newLocationX, newLocationY);
                        insideAction.Invoke(insidePoint);
                        insidePoints.Add(insidePoint);
                    }

                    neighbours[newLocationIndex] = newNeighbours;
                }
                occupiedPointCount++;
            }

            if (occupiedPointCount == 0)
            {
                return null;
            }

            var perimeter = WalkPerimeter(firstPoint, neighbours, level.PointToIndex, perimeterAction, outsideAction);

            var doorwayPoints = perimeter.Count > 7 ? FindDoorwayPoints(perimeter, level) : null;

            return new Room(level, new Rectangle(new Point(left, firstPoint.Y), new Point(right, lastPoint.Y)),
                doorwayPoints, insidePoints);
        }

        private static List<Point> FindDoorwayPoints(IReadOnlyList<Point> perimeter, Level level,
            bool allowCorners = false)
        {
            var doorwayPoints = new List<Point>();
            for (var i = 0; i < perimeter.Count; i++)
            {
                var point = perimeter[i];
                var nextPoint = perimeter[i == perimeter.Count - 1 ? 0 : i + 1];
                if (point.DistanceTo(nextPoint) > 1)
                {
                    i++;
                    continue;
                }

                var previousPoint = perimeter[i == 0 ? perimeter.Count - 1 : i - 1];
                if (point.DistanceTo(previousPoint) > 1)
                {
                    continue;
                }

                var direction = previousPoint.DirectionTo(nextPoint).GetUnit();
                if (!allowCorners && direction.X != 0 && direction.Y != 0)
                {
                    continue;
                }

                var firstOrthogonal = direction.GetOrthogonal();
                var secondOrthogonal = firstOrthogonal.GetInverse();

                var corridorCandidatePoint = point.Translate(firstOrthogonal);
                if (!level.IsValid(corridorCandidatePoint) || !level.CanPlaceCorridor(corridorCandidatePoint))
                {
                    continue;
                }

                var doorwayCandidatePoint = point.Translate(secondOrthogonal);
                if (!level.IsValid(doorwayCandidatePoint) || !level.CanPlaceCorridor(doorwayCandidatePoint))
                {
                    continue;
                }

                doorwayPoints.Add(point);
            }
            return doorwayPoints;
        }

        private static IReadOnlyList<Point> WalkPerimeter(Point firstPoint, byte[] neighbours, int[,] pointToIndex,
            Action<Point> perimeterAction, Action<Point> outsideAction) =>
            WalkPerimeter(new Point(firstPoint.X, (byte)(firstPoint.Y - 1)), DirectionFlags.South, Direction.North,
                new Dictionary<Point, int>(), neighbours, pointToIndex, perimeterAction, outsideAction) ??
            new List<Point>();

        private static List<Point> WalkPerimeter(Point firstPoint, DirectionFlags currentNeighbourMap,
            Direction previousPointDirection, Dictionary<Point, int> visitedIntersections, byte[] neighbours,
            int[,] pointToIndex, Action<Point> perimeterAction, Action<Point> outsideAction)
        {
            // Assuming both the perimeter and the inner area are contiguos
            List<Point> perimeter = null;
            var currentPoint = firstPoint;
            var previousPointDirectionIndex = (int)previousPointDirection;
            var perimeterLooped = false;

            for (var j = 0; j < neighbours.Length; j++)
            {
                var perimeterPointFound = false;
                for (var i = 3; i > 0; i--)
                {
                    var directionIndexToCheck = (previousPointDirectionIndex + i * 2) % 8;
                    var neighbourDirection = (DirectionFlags)(1 << directionIndexToCheck);
                    if ((currentNeighbourMap & neighbourDirection) == neighbourDirection)
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
                            if (perimeter.Count == 0 || !nextPoint.Equals(perimeter[0]))
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

                        if (visitedIntersections.TryGetValue(nextPoint, out var _))
                        {
                            continue;
                        }
                        visitedIntersections[nextPoint] = directionIndexToCheck;

                        outsideAction.Invoke(nextPoint);

                        var subPerimeter = WalkPerimeter(nextPoint, nextNeighbourMap,
                            (Direction)Level.OppositeDirectionIndexes[directionIndexToCheck], visitedIntersections,
                            neighbours, pointToIndex, perimeterAction, outsideAction);

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

        private static bool IsPerimeter(DirectionFlags neighbourMap) =>
            (neighbourMap & DirectionFlags.NorthwestCorner) == DirectionFlags.NorthwestCorner ||
            (neighbourMap & DirectionFlags.NortheastCorner) == DirectionFlags.NortheastCorner ||
            (neighbourMap & DirectionFlags.SoutheastCorner) == DirectionFlags.SoutheastCorner ||
            (neighbourMap & DirectionFlags.SouthwestCorner) == DirectionFlags.SouthwestCorner;

        protected static Dictionary<string, Func<TMapFragment, object, bool>> GetPropertyConditions<TMapFragment>()
            where TMapFragment : MapFragment
        {
            return new Dictionary<string, Func<TMapFragment, object, bool>>
            {
                {nameof(Name), (o, v) => v != null},
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                {
                    nameof(GenerationWeight),
                    (o, v) => (Weight)v != null && (!(v is DefaultWeight def) || def.Multiplier != 1)
                },
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

        public abstract ICSScriptSerializer GetSerializer();
    }
}