using CSharpScriptSerialization;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils;

namespace UnicornHack.Generation.Map;

/// <summary>
///     A <see cref="RoomLayout" /> that fills the room with chunks placed in a uniform-grid lot
///     selection scheme. Chunks are picked from the available <see cref="MapFragment" />s and
///     connected with corridors.
/// </summary>
/// <remarks>
///     Honoring <see cref="RoomConnection" />s (edge placement of doors): currently the room
///     <see cref="RoomComponent" /> spans the entire level and connections are <see cref="ConnectionComponent" />s
///     created by the <see cref="ConnectingMapFragment" />s embedded in chunks. Once levels split
///     into multiple per-room <see cref="RoomComponent" /> entities the <c>connections</c> argument
///     will drive door placement on the room's perimeter; for now it's accepted but not enforced
///     because every chunk-placed connection is already on the perimeter of the single big room.
/// </remarks>
public class UniformRoomLayout : RoomLayout, ICSScriptSerializable
{
    protected static Dimensions DefaultMinLotSize = new(5, 5);

    public Dimensions MinLotSize
    {
        get;
        set;
    } = DefaultMinLotSize;

    protected int LotPlacementAttempts
    {
        get;
        set;
    } = 16;

    public override List<Chunk>? Fill(
        RoomComponent room,
        Branch branch,
        DefiningMapFragment fragment,
        Dictionary<Direction, int> doorwayDemand)
    {
        // Terrain has been allocated by the LevelLayout. Initialize it (default-fill)
        // and then place fragments into lots.
        var defaultTerrain = fragment.GetEffectiveDefaultTerrain(branch);
        if (defaultTerrain != MapFeature.Default)
        {
            for (var index = 0; index < room.TileCount; index++)
            {
                room.Terrain[index] = defaultTerrain;
            }
        }

        var chunks = PlaceDefiningMapFragment(room, fragment);

        var remainingDemand = new Dictionary<Direction, int>(doorwayDemand);
        for (var i = 0; i < chunks.Count; i++)
        {
            DeductChunkDoorways(room, chunks[i], remainingDemand);
        }

        PlaceSurroundingFragments(room, chunks, remainingDemand);

        return SatisfiesDemand(room, chunks, doorwayDemand) ? chunks : null;
    }

    private static List<Chunk> PlaceDefiningMapFragment(RoomComponent room, DefiningMapFragment fragment)
    {
        var chunks = new List<Chunk>();
        var chunk = fragment.TryPlace(room, room.BoundingRectangle);
        if (chunk != null)
        {
            chunks.Add(chunk);
        }

        return chunks;
    }

    private void PlaceSurroundingFragments(
        RoomComponent room,
        List<Chunk> chunks,
        Dictionary<Direction, int> remainingDemand)
    {
        var levelBoundingRectangle = room.BoundingRectangle;
        var partition = new RectanglePartition(levelBoundingRectangle);
        var filledArea = 0;

        var placedChunk = chunks.FirstOrDefault();
        if (placedChunk != null)
        {
            partition.Insert(placedChunk.BoundingRectangle);
            filledArea += placedChunk.BoundingRectangle.Area;
        }

        var placingLevelConnection = NeedsLevelConnection(room);
        while (filledArea / (float)levelBoundingRectangle.Area < Coverage && chunks.Count < MaxChunkCount)
        {
            var nextLot = SelectNextLot(partition, room.Level.GenerationRandom);
            if (nextLot == null)
            {
                // Room is too small for more lots — done.
                break;
            }

            var lotMinDoorways = ComputeLotMinDoorways(room, nextLot.Value, remainingDemand);

            var danglingConnection = room.IncomingConnections
                .Select(c => c.Connection!).FirstOrDefault(c => c.TargetLevelX == null);
            var sortedFragments = placingLevelConnection
                ? room.Level.GenerationRandom.WeightedOrder(
                    room.Level.Game.Services.Data.ConnectingMapFragments.GetAsList(),
                    f => f.GetWeight(room, nextLot.Value, danglingConnection))
                : (IEnumerable<MapFragment>)room.Level.GenerationRandom.WeightedOrder(
                    room.Level.Game.Services.Data.NormalMapFragments.GetAsList(),
                    f => f.GetWeight(room, nextLot.Value));

            placedChunk = TryPlace(room, nextLot.Value, sortedFragments, lotMinDoorways);
            if (placedChunk == null)
            {
                if (placingLevelConnection)
                {
                    placingLevelConnection = false;
                    continue;
                }

                // Room is too small for any fragment — done.
                break;
            }

            chunks.Add(placedChunk);
            partition.Insert(placedChunk.BoundingRectangle);
            filledArea += placedChunk.BoundingRectangle.Area;
            DeductChunkDoorways(room, placedChunk, remainingDemand);

            // Cap inter-level connections at one per room: as soon as the connection
            // fragment is placed, switch the rest of the loop to NormalMapFragments.
            if (placingLevelConnection && placedChunk.Fragment is ConnectingMapFragment)
            {
                placingLevelConnection = false;
            }
        }

        if (chunks.Count > 1)
        {
            var connectedChunks = new List<Chunk> { chunks.First() };
            var unconnectedChunks = new List<Chunk>(chunks.Skip(1));
            while (unconnectedChunks.Count > 0)
            {
                var randomConnectedChunk = room.Level.GenerationRandom.Pick(connectedChunks);
                var unconnectedChunk = randomConnectedChunk.GetOrthogonallyClosest(unconnectedChunks)!;
                var closestConnectedChunk = unconnectedChunk.GetOrthogonallyClosest(connectedChunks)!;
                if (!Connect(unconnectedChunk, closestConnectedChunk))
                {
                    throw new InvalidOperationException("Couldn't connect all chunks");
                }

                connectedChunks.Add(unconnectedChunk);
                unconnectedChunks.Remove(unconnectedChunk);
            }
        }
    }

    /// <summary>
    ///     Returns whether <paramref name="room" /> needs a <see cref="ConnectingMapFragment" />
    ///     placed during fill:
    ///     <list type="bullet">
    ///         <item>Entrance room with a pending incoming connection from the level above.</item>
    ///         <item>Exit room on a non-terminal level (Depth &lt; Length).</item>
    ///         <item>Exit room on a terminal level when the branch is not flagged
    ///         <see cref="GameBranch.IsTerminal" />. The actual destination branch is
    ///         picked randomly by <see cref="LevelConnection.CreateSourceConnection" /> using
    ///         <see cref="Branch.GenerationWeight" />s.</item>
    ///         <item>An exit room that already has an outgoing inter-level connection
    ///         (placed by the defining fragment) is skipped.</item>
    ///     </list>
    /// </summary>
    private static bool NeedsLevelConnection(RoomComponent room)
    {
        if ((room.Features & RoomFeatures.Entrance) != 0
            && room.IncomingConnections.Count > 0)
        {
            return true;
        }

        if ((room.Features & RoomFeatures.Exit) != 0)
        {
            // Already has an outgoing inter-level connection from the defining fragment.
            foreach (var connection in room.Connections.Values)
            {
                if (connection.Connection!.TargetLevelX == null)
                {
                    return false;
                }
            }

            var branch = room.Level.Branch;
            if (room.Level.Depth < branch.Length)
            {
                return true;
            }

            // Terminal level: only place a connection when the branch isn't terminal.
            return !branch.IsTerminal;
        }

        return false;
    }

    private static Chunk? TryPlace(
        RoomComponent room,
        Rectangle nextLot,
        IEnumerable<MapFragment> sortedFragments,
        int minDoorways)
    {
        foreach (var nextFragment in sortedFragments)
        {
            var placedChunk = nextFragment.TryPlace(room, nextLot, minDoorways);
            if (placedChunk == null)
            {
                continue;
            }

            IncrementInstanceCounts(room, nextFragment);
            return placedChunk;
        }

        return null;
    }

    /// <summary>
    ///     Returns the per-fragment minimum doorway count for a lot at <paramref name="lot" />:
    ///     the number of distinct room-perimeter directions the lot touches that have
    ///     remaining unmet demand. A corner lot touching two unsatisfied edges needs at
    ///     least two doorways from its fragment; a strictly interior lot or one on an
    ///     already-satisfied edge demands none.
    /// </summary>
    /// <remarks>
    ///     Cardinal directions are counted when the lot's bounding rectangle is flush
    ///     with the matching room edge. Ordinal directions are counted only at the
    ///     matching corner (lot flush with both adjacent cardinals) — a single-edge lot
    ///     can serve a diagonal-edge graph connection through whichever cardinal it
    ///     touches, so we don't bill it twice.
    /// </remarks>
    private static int ComputeLotMinDoorways(
        RoomComponent room,
        Rectangle lot,
        Dictionary<Direction, int> remainingDemand)
    {
        if (remainingDemand.Count == 0)
        {
            return 0;
        }

        var touchesWest = lot.TopLeft.X == 0;
        var touchesEast = lot.BottomRight.X == room.Width - 1;
        var touchesNorth = lot.TopLeft.Y == 0;
        var touchesSouth = lot.BottomRight.Y == room.Height - 1;

        var min = 0;
        foreach (var (direction, required) in remainingDemand)
        {
            if (required <= 0)
            {
                continue;
            }

            var touches = direction switch
            {
                Direction.West => touchesWest,
                Direction.East => touchesEast,
                Direction.North => touchesNorth,
                Direction.South => touchesSouth,
                Direction.Northwest => touchesNorth && touchesWest,
                Direction.Northeast => touchesNorth && touchesEast,
                Direction.Southwest => touchesSouth && touchesWest,
                Direction.Southeast => touchesSouth && touchesEast,
                _ => false
            };

            if (touches)
            {
                min++;
            }
        }

        // Add one for the intra-room connection
        return min + 1;
    }

    /// <summary>
    ///     Subtracts the doorway contribution of <paramref name="chunk" /> from
    ///     <paramref name="remainingDemand" />. Each doorway tile that is not already
    ///     used by an existing connection decrements every direction it satisfies.
    /// </summary>
    private static void DeductChunkDoorways(
        RoomComponent room,
        Chunk chunk,
        Dictionary<Direction, int> remainingDemand)
    {
        if (remainingDemand.Count == 0 || chunk.Room != room || chunk.DoorwayPoints.Count == 0)
        {
            return;
        }

        var centerX = room.Width / 2;
        var centerY = room.Height / 2;

        foreach (var p in chunk.DoorwayPoints)
        {
            if (room.Connections.ContainsKey(p))
            {
                continue;
            }

            var dx = p.X - centerX;
            var dy = p.Y - centerY;

            foreach (var direction in remainingDemand.Keys)
            {
                if (remainingDemand[direction] > 0 && direction.IsInDirection(dx, dy))
                {
                    remainingDemand[direction]--;
                }
            }
        }
    }

    public static void IncrementInstanceCounts(RoomComponent room, MapFragment fragment)
    {
        // TODO: Increment fragment instance count on level, branch, game
        // TODO: Increment each tag instance count on level, branch, game
    }

    private bool Connect(Chunk source, Chunk target,
        bool allowDiagonals = false, int width = 1, bool addColumns = false)
    {
        var from = source.GetGoodConnectionPoint(target);
        var to = target.GetGoodConnectionPoint(source);

        // Both calls return the path in source→target order. The "alternative" runs the
        // pathfinder with the endpoints swapped (which may discover a different route
        // through the room's open space because Dijkstra tie-breaks on enqueue order)
        // and asks FindCorridorPath to flip its output so Connect can treat both
        // candidates uniformly.
        var path = FindCorridorPath(from, to, source.Room, allowDiagonals, reverse: false);
        var alternativePath = FindCorridorPath(to, from, source.Room, allowDiagonals, reverse: true);

        var corridor =
            path.FragmentFeaturesHit > alternativePath.FragmentFeaturesHit
                ? alternativePath.Path
                : path.Path;

        corridor = TrimCorridorEndpoints(corridor, source, target);

        WriteCorridor(corridor, source.Room, width, addColumns);

        return true;
    }

    /// <summary>
    ///     Drops corridor tiles that already sit inside the source or target chunk's
    ///     interior. Such tiles were written as floor by the fragment, so the corridor
    ///     pass would otherwise either no-op on them (waste) or, more importantly,
    ///     carve perimeter walls reached through the interior into redundant archways.
    ///     The trimmed corridor begins immediately after the source chunk's interior
    ///     ends and stops immediately before the target chunk's interior begins, so
    ///     <see cref="WriteCorridor" /> only touches tiles that genuinely bridge the
    ///     gap between the two chunks.
    /// </summary>
    /// <remarks>
    ///     <paramref name="path" /> must be in source→target order;
    /// </remarks>
    public static IReadOnlyList<Point> TrimCorridorEndpoints(
        IReadOnlyList<Point> path, Chunk sourceChunk, Chunk targetChunk)
    {
        if (path.Count < 2)
        {
            return path;
        }

        var sourceInside = sourceChunk.InsidePoints;
        var targetInside = targetChunk.InsidePoints;

        // Walk forward and remember the LAST index whose tile is in the source chunk's
        // interior. The corridor only needs to start outside the source chunk because the
        // chunk's fragment already made every interior tile walkable.
        var dropUntil = -1;
        for (var i = 0; i < path.Count; i++)
        {
            if (sourceInside.Contains(path[i]))
            {
                dropUntil = i;
            }
        }

        // Symmetrically walk backward from the end and remember the EARLIEST index in the
        // target chunk's interior. dropUntil bounds the scan so a tile that's in both
        // interiors (the chunks overlap edge-to-edge) isn't double-counted.
        var dropFrom = path.Count;
        for (var i = path.Count - 1; i > dropUntil; i--)
        {
            if (targetInside.Contains(path[i]))
            {
                dropFrom = i;
            }
        }

        var start = dropUntil + 1;
        var end = dropFrom - 1;
        if (start > end)
        {
            return [];
        }
        if (start == 0 && end == path.Count - 1)
        {
            return path;
        }

        var trimmed = new Point[end - start + 1];
        for (var i = 0; i < trimmed.Length; i++)
        {
            trimmed[i] = path[start + i];
        }

        return trimmed;
    }

    private static bool IsWall(Point location, RoomComponent room)
    {
        var feature = room.Terrain[room.PointToIndex![location.X, location.Y]];
        return feature == MapFeature.StoneWall || feature == MapFeature.RockWall || feature == MapFeature.StoneArchway;
    }

    /// <summary>
    ///     Dijkstra-style pathfinder from <paramref name="source" /> to
    ///     <paramref name="target" />. Walkable tiles cost 1 step, walls cost a large
    ///     penalty so corridors prefer existing open space but can still carve through
    ///     adjacent chunk walls when no open route exists (e.g. tight-packed lots where
    ///     two chunks share a wall column). The wall tiles on the resulting path are
    ///     converted to archways by <see cref="WriteCorridor" />.
    /// </summary>
    /// <param name="reverse">
    ///     When <c>false</c> (default), the returned <see cref="Corridor.Path" /> runs
    ///     from <paramref name="source" /> to <paramref name="target" />. When <c>true</c>,
    ///     the path is returned in the opposite order — useful for the "alternative
    ///     path" probe in <see cref="Connect" />, which calls this with swapped endpoints
    ///     but wants both candidates in the same source→target orientation for downstream
    ///     processing.
    /// </param>
    private static Corridor FindCorridorPath(
        Point source, Point target, RoomComponent room, bool allowDiagonals, bool reverse)
    {
        if (source.Equals(target))
        {
            return new Corridor([source], 0);
        }

        var width = room.Width;
        var height = room.Height;
        var pointToIndex = room.PointToIndex!;
        var indexToPoint = room.IndexToPoint!;

        var dist = new int[room.TileCount];
        var parent = new short[room.TileCount];
        for (var i = 0; i < dist.Length; i++)
        {
            dist[i] = int.MaxValue;
            parent[i] = -1;
        }

        var sourceIdx = pointToIndex[source.X, source.Y];
        var targetIdx = pointToIndex[target.X, target.Y];
        dist[sourceIdx] = 0;

        // Two priority "buckets" keyed by cost class: visit walkable neighbours first
        // (cost 1), wall neighbours later (cost WallStepCost). The cap on dist guards
        // against revisiting at a worse score.
        const int WallStepCost = 1000;

        var open = new PriorityQueue<short, int>();
        open.Enqueue(sourceIdx, 0);

        Span<(sbyte X, sbyte Y)> neighbours =
        [
            (1, 0),
            (-1, 0),
            (0, 1),
            (0, -1),
            (1, 1),
            (1, -1),
            (-1, 1),
            (-1, -1),
        ];
        var neighbourCount = allowDiagonals ? 8 : 4;

        while (open.TryDequeue(out var currentIdx, out var currentDist))
        {
            if (currentIdx == targetIdx)
            {
                break;
            }

            // Stale entry — a better cost has already been processed.
            if (currentDist > dist[currentIdx])
            {
                continue;
            }

            var current = indexToPoint[currentIdx];
            for (var i = 0; i < neighbourCount; i++)
            {
                var (dx, dy) = neighbours[i];
                var nx = current.X + dx;
                var ny = current.Y + dy;
                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                {
                    continue;
                }

                var nIdx = pointToIndex[(byte)nx, (byte)ny];
                var nPoint = new Point((byte)nx, (byte)ny);
                var isTarget = nIdx == targetIdx;
                var canWalk = isTarget || CanPlaceCorridor(nPoint, room);
                var stepCost = canWalk ? 1 : WallStepCost;

                var candidateDist = currentDist + stepCost;
                if (candidateDist >= dist[nIdx])
                {
                    continue;
                }

                dist[nIdx] = candidateDist;
                parent[nIdx] = currentIdx;
                open.Enqueue(nIdx, candidateDist);
            }
        }

        if (dist[targetIdx] == int.MaxValue)
        {
            // Truly unreachable (shouldn't happen with wall traversal allowed).
            return new Corridor([source], 1);
        }

        // Walk the parent chain from target back to source. The natural reconstruction
        // order is target→source; flipping to source→target is the default. The reverse
        // flag suppresses the flip so the alternative-direction probe in Connect can
        // line both candidates up in source→target order without an extra reverse pass.
        // Number of wall tiles traversed by the chosen path becomes the "hit" score so
        // Connect can prefer the direction with fewer carves.
        var hits = 0;
        var path = new List<Point>();
        var cursor = targetIdx;
        while (cursor != -1)
        {
            var point = indexToPoint[cursor];
            if (!CanPlaceCorridor(point, room))
            {
                hits++;
            }
            path.Add(point);
            if (cursor == sourceIdx)
            {
                break;
            }

            cursor = parent[cursor];
        }
        if (!reverse)
        {
            path.Reverse();
        }

        return new Corridor(path, hits);
    }

    /// <summary>
    ///     Returns whether <paramref name="location" /> may host an intermediate corridor
    ///     tile. Walls are off-limits — the corridor algorithm in
    ///     <see cref="FindCorridorPath" /> only allows stepping onto a wall when the wall
    ///     tile is the corridor target itself (the door). All other tiles must be floor or
    ///     unfilled <see cref="MapFeature.Default" />.
    /// </summary>
    public static bool CanPlaceCorridor(Point location, RoomComponent room)
    {
        switch (room.Terrain[room.PointToIndex![location.X, location.Y]])
        {
            case MapFeature.Default:
            case MapFeature.RockFloor:
            case MapFeature.StoneFloor:
                return true;
            default:
                return false;
        }
    }

    private static void WriteCorridor(IReadOnlyList<Point> path, RoomComponent room, int width, bool addColumns)
    {
        foreach (var point in path)
        {
            var index = room.PointToIndex![point.X, point.Y];
            var currentFeature = room.Terrain[index];
            if (currentFeature != MapFeature.RockFloor && currentFeature != MapFeature.StoneFloor)
            {
                // TODO: Surround the corridor with walls matching the existing wall type
                room.Terrain[index] = currentFeature == MapFeature.StoneWall
                    ? MapFeature.StoneArchway
                    : MapFeature.RockFloor;
            }
        }
    }

    /// <summary>
    ///     Picks the next lot rectangle for fragment placement by querying the
    ///     <see cref="RectanglePartition" /> for the largest empty rectangle that fits
    ///     <see cref="MinLotSize" />. When no current empty rectangle is large enough,
    ///     <see cref="RectanglePartition.Recalculate" /> is invoked once to merge touching
    ///     empty rectangles into larger candidates before giving up.
    /// </summary>
    protected virtual Rectangle? SelectNextLot(RectanglePartition partition, SimpleRandom random)
    {
        var best = FindLargestFitting(partition);
        if (best != null)
        {
            return best;
        }

        var recalculated = partition.Recalculate();
        if (recalculated == null)
        {
            return null;
        }

        var minSize = MinLotSize;
        if (recalculated.Value.Width < minSize.Width || recalculated.Value.Height < minSize.Height)
        {
            return null;
        }

        return recalculated.Value;
    }

    private Rectangle? FindLargestFitting(RectanglePartition partition)
    {
        // EmptyRectangles is sorted by area descending, so the first rectangle that fits
        // MinLotSize is the largest one that does. Once we reach a rectangle whose area
        // is below the minimum required, no later rectangle can fit either.
        var minSize = MinLotSize;
        var minArea = minSize.Width * minSize.Height;
        foreach (var empty in partition.EmptyRectangles)
        {
            if (empty.Area < minArea)
            {
                break;
            }

            if (empty.Width >= minSize.Width && empty.Height >= minSize.Height)
            {
                return empty;
            }
        }

        return null;
    }

    private sealed class Corridor
    {
        public Corridor(IReadOnlyList<Point> path, int fragmentFeaturesHit)
        {
            Path = path;
            FragmentFeaturesHit = fragmentFeaturesHit;
        }

        public IReadOnlyList<Point> Path { get; }
        public int FragmentFeaturesHit { get; }
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<UniformRoomLayout>(GetPropertyConditions<UniformRoomLayout>());

    public ICSScriptSerializer GetSerializer() => Serializer;

    protected static Dictionary<string, Func<TUniformRoomLayout, object?, bool>> GetPropertyConditions<TUniformRoomLayout>()
        where TUniformRoomLayout : UniformRoomLayout => new()
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        { nameof(Coverage), (_, v) => (float)v! != DefaultCoverage },
        { nameof(MaxChunkCount), (_, v) => (byte)v! != DefaultMaxChunkCount },
        { nameof(MinLotSize), (_, v) => !DefaultMinLotSize.Equals(v) }
    };
}
