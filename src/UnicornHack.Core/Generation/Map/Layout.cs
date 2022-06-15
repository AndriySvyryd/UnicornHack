using System;
using System.Collections.Generic;
using System.Linq;
using UnicornHack.Primitives;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Generation.Map;

public abstract class Layout
{
    public float Coverage
    {
        get;
        set;
    } = 0.33f;

    public byte MaxRoomCount
    {
        get;
        set;
    } = 16;

    public virtual List<Room> Fill(LevelComponent level, DefiningMapFragment fragment)
    {
        InitializeTerrain(level, fragment);
        var rooms = PlaceDefiningMapFragment(level, fragment);
        PlaceSurroundingFragments(level, rooms);

        return rooms;
    }

    protected void InitializeTerrain(LevelComponent level, DefiningMapFragment fragment)
    {
        level.TerrainChanges = null;
        level.WallNeighborsChanges = null;
        if (fragment.DefaultTerrain != MapFeature.Default)
        {
            for (var index = 0; index < level.Terrain.Length; index++)
            {
                level.Terrain[index] = (byte)fragment.DefaultTerrain;
            }
        }
    }

    protected List<Room> PlaceDefiningMapFragment(LevelComponent level, DefiningMapFragment fragment)
    {
        var rooms = new List<Room>();
        var room = fragment.TryPlace(level, level.BoundingRectangle);
        if (room != null)
        {
            rooms.Add(room);
        }
        // TODO: place nested fragments if needed

        return rooms;
    }

    protected void PlaceSurroundingFragments(LevelComponent level, List<Room> rooms)
    {
        var manager = level.Entity.Manager;
        var levelBoundingRectangle = level.BoundingRectangle;
        var placedFragments = new RectangleIntervalTree(levelBoundingRectangle);
        var filledArea = 0;

        var placedRoom = rooms.FirstOrDefault();
        if (placedRoom != null)
        {
            placedFragments.Insert(placedRoom.BoundingRectangle);
            filledArea += placedRoom.BoundingRectangle.Area;
        }

        var placingConnections = true;
        while (filledArea / (float)levelBoundingRectangle.Area < Coverage && rooms.Count < MaxRoomCount)
        {
            var nextLot = SelectNextLot(placedFragments, level.GenerationRandom);
            if (nextLot == null)
            {
                throw new InvalidOperationException("No more available lots found");
            }

            // For every existing connection to this level that hasn't been connected yet generate a destination fragment.
            // Then generate up to 3 source fragments, depending on the number of incoming connections
            //     At least 2 to the next branch level if not final
            var danglingConnection = level.IncomingConnections
                .Select(c => c.Connection).FirstOrDefault(c => c.TargetLevelX == null);
            placingConnections = placingConnections
                                 && (danglingConnection != null
                                     || level.Connections.Values
                                         .Count(c => c.Connection.TargetLevelX == null) < 3
                                     || (level.Branch.Length > level.Depth
                                         && level.Connections.Values
                                             .Select(c => manager.FindEntity(c.Connection.TargetLevelId).Level)
                                             .Count(l =>
                                                 l.BranchName == level.BranchName
                                                 && l.Depth == level.Depth + 1) < 2));
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

            rooms.Add(placedRoom);
            placedFragments.Insert(placedRoom.BoundingRectangle);
            filledArea += placedRoom.BoundingRectangle.Area;
        }

        // TODO: Fill the empty space with snapping fragments and connect them to overwritable fragments

        if (rooms.Count > 1)
        {
            // TODO: Perf: use a different data structure
            var connectedRooms = new List<Room> { rooms.First() };
            var unconnectedRooms = new List<Room>(rooms.Skip(1).Where(r => r.DoorwayPoints.Count > 0));
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

    private Room TryPlace(LevelComponent level, Rectangle nextLot, IEnumerable<MapFragment> sortedFragments)
    {
        foreach (var nextFragment in sortedFragments)
        {
            var placedRoom = nextFragment.TryPlace(level, nextLot);
            if (placedRoom != null)
            {
                IncrementInstanceCounts(level, nextFragment);
                return placedRoom;
            }
        }

        return null;
    }

    public static void IncrementInstanceCounts(LevelComponent levelComponent, MapFragment fragment)
    {
        // TODO: Increment fragment instance count on level, branch, game
        // TODO: Increment each tag instance count on level, branch, game
    }

    private bool Connect(Room source, Room target,
        bool allowDiagonals = false, bool avoidTurns = true, int width = 1, bool addColumns = false)
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

    private Corridor FindCorridorPath(
        Point source, Point target, LevelComponent level, bool allowDiagonals, bool avoidTurns)
    {
        // TODO: Use A* to tunnel around fragments
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
                isHorizontalFree = CanPlaceCorridor(horizontalMove.Value, level);
            }

            if (heading.Y != 0)
            {
                verticalMove = new Point(current.X, (byte)(current.Y + heading.Y));
                isVerticalFree = CanPlaceCorridor(verticalMove.Value, level);

                if (allowDiagonals && heading.X != 0)
                {
                    diagonalMove = current.Translate(heading);
                    isDiagonalFree = CanPlaceCorridor(diagonalMove.Value, level);
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

    public static bool CanPlaceCorridor(Point location, LevelComponent levelComponent)
    {
        switch ((MapFeature)levelComponent.Terrain[levelComponent.PointToIndex[location.X, location.Y]])
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

    private void WriteCorridor(IReadOnlyList<Point> path, LevelComponent level, int width, bool addColumns)
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

        public IReadOnlyList<Point> Path
        {
            get;
        }

        public int FragmentFeaturesHit
        {
            get;
        }

        public int FragmentFeaturesAlmostHit
        {
            get;
        }
    }
}
