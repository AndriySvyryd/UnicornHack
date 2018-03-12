using System;
using System.Collections.Generic;
using System.Linq;
using UnicornHack.Utils;

namespace UnicornHack.Generation.Map
{
    public abstract class Layout
    {
        public virtual float Coverage { get; set; } = 0.33f;
        public virtual byte MaxRoomCount { get; set; } = 16;

        public virtual void OnLoad()
        {
        }

        public virtual void Fill(Level level, DefiningMapFragment fragment)
        {
            InitializeTerrain(level, fragment);
            PlaceDefiningMapFragment(level, fragment);
            PlaceSurroundingFragments(level);
        }

        protected virtual void InitializeTerrain(Level level, DefiningMapFragment fragment)
        {
            level.TerrainChanges = null;
            level.WallNeighboursChanges = null;
            if (fragment.DefaultTerrain != MapFeature.Default)
            {
                for (var index = 0; index < level.Terrain.Length; index++)
                {
                    level.Terrain[index] = (byte)fragment.DefaultTerrain;
                }
            }
        }

        protected virtual void PlaceDefiningMapFragment(Level level, DefiningMapFragment fragment)
        {
            fragment.TryPlace(level, level.BoundingRectangle);
            // TODO: place nested fragments if needed
        }

        protected virtual void PlaceSurroundingFragments(Level level)
        {
            var levelBoundingRectangle = level.BoundingRectangle;
            var placedFragments = new RectangleIntervalTree(levelBoundingRectangle);
            var filledArea = 0;

            var placedRoom = level.Rooms.FirstOrDefault();
            if (placedRoom != null)
            {
                placedFragments.Insert(placedRoom.BoundingRectangle);
                filledArea += placedRoom.BoundingRectangle.Area;
            }

            var placingConnections = true;
            while (filledArea / (float)levelBoundingRectangle.Area < Coverage && level.Rooms.Count < MaxRoomCount)
            {
                var nextLot = SelectNextLot(placedFragments, level.GenerationRandom);
                if (nextLot == null)
                {
                    throw new InvalidOperationException("No more available lots found");
                }

                // For every existing connection to this level that hasn't been connected yet generate a destination fragment.
                // Then generate up to 3 source fragments, depending on the number of incoming connections
                //     At least 2 to the next branch level if not final
                var danglingConnection = level.IncomingConnections.FirstOrDefault(c => c.TargetLevelX == null);
                placingConnections = placingConnections && (danglingConnection != null ||
                                                            level.Connections.Count(c => c.TargetLevelX == null) < 3 ||
                                                            (level.Branch.Length > level.Depth &&
                                                             level.Connections.Count(c =>
                                                                 c.TargetBranchName == level.BranchName &&
                                                                 c.TargetLevelDepth == level.Depth + 1) < 2));
                var sortedFragments = placingConnections
                    ? level.GenerationRandom.WeightedOrder(
                        ConnectingMapFragment.Loader.GetAsList(),
                        f => f.GetWeight(level, nextLot.Value, danglingConnection))
                    : (IEnumerable<MapFragment>)level.GenerationRandom.WeightedOrder(
                        NormalMapFragment.Loader.GetAsList(),
                        f => f.GetWeight(level, nextLot.Value));

                placedRoom = TryPlace(level, nextLot.Value, sortedFragments);
                if (placedRoom == null)
                {
                    if (placingConnections)
                    {
                        placingConnections = false;
                        continue;
                    }

                    throw new InvalidOperationException("No suitable fragments");
                }

                placedFragments.Insert(placedRoom.BoundingRectangle);
                filledArea += placedRoom.BoundingRectangle.Area;
            }

            // TODO: Fill the empty space with snapping fragments and connect them to ovewritable fragments

            if (level.Rooms.Count > 1)
            {
                // TODO: Perf: use a different data structure
                var connectedRooms = new List<Room> {level.Rooms.First()};
                var unconnectedRooms = new List<Room>(level.Rooms.Skip(1).Where(r => r.DoorwayPoints.Count > 0));
                while (unconnectedRooms.Count > 0)
                {
                    var randomConnectedRoom = level.GenerationRandom.Pick(connectedRooms);
                    var unconnectedRoom = randomConnectedRoom.GetOrthogonallyClosest(unconnectedRooms);
                    var closestConnectedRoom = unconnectedRoom.GetOrthogonallyClosest(connectedRooms);
                    if (!Connect(unconnectedRoom, closestConnectedRoom))
                    {
                        throw new InvalidOperationException("Couldn't connect all rooms");
                    }

                    connectedRooms.Add(unconnectedRoom);
                    unconnectedRooms.Remove(unconnectedRoom);
                }
            }
        }

        private Room TryPlace(Level level, Rectangle nextLot, IEnumerable<MapFragment> sortedFragments)
        {
            foreach (var nextFragment in sortedFragments)
            {
                var placedRoom = nextFragment.TryPlace(level, nextLot);
                if (placedRoom != null)
                {
                    level.IncrementInstanceCounts(nextFragment);
                    return placedRoom;
                }
            }

            return null;
        }

        private bool Connect(Room source, Room target, bool allowDiagonals = false, bool avoidTurns = true,
            int width = 1, bool addColumns = false)
        {
            var from = source.GetGoodConnectionPoint(target);
            var to = target.GetGoodConnectionPoint(source);

            var path = FindCorridorPath(from, to, source.Level, allowDiagonals, avoidTurns);
            var alternativePath = FindCorridorPath(to, from, source.Level, allowDiagonals, avoidTurns);

            var corridor =
                path.FragmentFeaturesHit > alternativePath.FragmentFeaturesHit ||
                (path.FragmentFeaturesHit == alternativePath.FragmentFeaturesHit &&
                 path.FragmentFeaturesAlmostHit > alternativePath.FragmentFeaturesAlmostHit)
                    ? alternativePath.Path
                    : path.Path;

            WriteCorridor(corridor, source.Level, width, addColumns);

            return true;
        }

        private Corridor FindCorridorPath(Point source, Point target, Level level, bool allowDiagonals, bool avoidTurns)
        {
            // TODO: Use A* to tunnel arround fragments
            var path = new List<Point>();
            var fragmentFeaturesHit = 0;
            var fragmentFeaturesAlmostHit = 0;

            var current = source;
            while (true)
            {
                path.Add(current);

                if (current.Equals(target))
                {
                    break;
                }

                var heading = current.DifferenceTo(target).GetUnit();

                Point? horizontalMove = null;
                Point? verticalMove = null;
                Point? diagonalMove = null;
                var isHorizontalFree = false;
                var isVerticalFree = false;
                var isDiagonalFree = false;
                if (heading.X != 0)
                {
                    horizontalMove = new Point((byte)(current.X + heading.X), current.Y);
                    isHorizontalFree = level.CanPlaceCorridor(horizontalMove.Value);
                }

                if (heading.Y != 0)
                {
                    verticalMove = new Point(current.X, (byte)(current.Y + heading.Y));
                    isVerticalFree = level.CanPlaceCorridor(verticalMove.Value);

                    if (allowDiagonals && heading.X != 0)
                    {
                        diagonalMove = current.Translate(heading);
                        isDiagonalFree = level.CanPlaceCorridor(diagonalMove.Value);
                    }
                }

                if (isHorizontalFree || isVerticalFree || isDiagonalFree)
                {
                    fragmentFeaturesAlmostHit += horizontalMove != null && !isHorizontalFree ? 1 : 0;
                    fragmentFeaturesAlmostHit += verticalMove != null && !isVerticalFree ? 1 : 0;
                    fragmentFeaturesAlmostHit += diagonalMove != null && !isDiagonalFree ? 1 : 0;

                    horizontalMove = isHorizontalFree ? horizontalMove : null;
                    verticalMove = isVerticalFree ? verticalMove : null;
                    diagonalMove = isDiagonalFree ? diagonalMove : null;
                }

                if (avoidTurns)
                {
                    if (path.Count > 0)
                    {
                        var previousHeading = path[path.Count - 1];
                        if (previousHeading.X != 0)
                        {
                            if (previousHeading.Y != 0 && diagonalMove.HasValue)
                            {
                                current = diagonalMove.Value;
                                continue;
                            }

                            if (horizontalMove.HasValue)
                            {
                                current = horizontalMove.Value;
                                continue;
                            }
                        }
                        else if (verticalMove.HasValue)
                        {
                            current = verticalMove.Value;
                            continue;
                        }
                    }

                    if (horizontalMove.HasValue)
                    {
                        current = horizontalMove.Value;
                        continue;
                    }

                    if (verticalMove.HasValue)
                    {
                        current = verticalMove.Value;
                        continue;
                    }

                    if (diagonalMove.HasValue)
                    {
                        current = diagonalMove.Value;
                    }
                }
            }

            return new Corridor(path, fragmentFeaturesHit, fragmentFeaturesAlmostHit);
        }

        private void WriteCorridor(IReadOnlyList<Point> path, Level level, int width, bool addColumns)
        {
            // TODO: Try to place connecting fragments instead of plain corridors
            foreach (var point in path)
            {
                var currentFeature = (MapFeature)level.Terrain[level.PointToIndex[point.X, point.Y]];
                if (currentFeature != MapFeature.RockFloor && currentFeature != MapFeature.StoneFloor)
                {
                    var index = level.PointToIndex[point.X, point.Y];
                    if (level.Terrain[index] == (byte)MapFeature.StoneWall)
                    {
                        level.Terrain[index] = (byte)MapFeature.StoneArchway;
                    }
                    else
                    {
                        level.Terrain[index] = (byte)MapFeature.RockFloor;
                    }
                }
            }
        }

        protected abstract Rectangle? SelectNextLot(RectangleIntervalTree placedFragments, SimpleRandom random);

        private class Corridor
        {
            public Corridor(IReadOnlyList<Point> path, int fragmentFeaturesHit, int fragmentFeaturesAlmostHit)
            {
                Path = path;
                FragmentFeaturesHit = fragmentFeaturesHit;
                FragmentFeaturesAlmostHit = fragmentFeaturesAlmostHit;
            }

            public IReadOnlyList<Point> Path { get; }
            public int FragmentFeaturesHit { get; }
            public int FragmentFeaturesAlmostHit { get; }
        }
    }
}