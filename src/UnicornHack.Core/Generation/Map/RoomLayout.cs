using UnicornHack.Data.Fragments;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

/// <summary>
///     Fills a single <see cref="RoomComponent" /> from one or more <see cref="Chunk" />s
///     (typically produced by <see cref="MapFragment" />s).
/// </summary>
/// <remarks>
///     Owns the room-scope concerns:
///     <list type="bullet">
///         <item>Fragment placement within the room (sub-chunks)</item>
///         <item>Carving corridors between chunks inside the room</item>
///     </list>
///     Knows nothing about the level graph -- that's the <see cref="LevelLayout" />'s job.
///     Terrain allocation is owned by <see cref="RoomTerrain" /> and performed by the caller
///     before <see cref="Fill" /> runs. Door placement at the perimeter is owned by
///     <see cref="LevelLayout.MaterializeGraph" />, which queries the room's chunks for
///     suitable doorway tiles.
/// </remarks>
public abstract class RoomLayout
{
    protected static float DefaultCoverage = 0.5f;

    public float Coverage
    {
        get;
        set;
    } = DefaultCoverage;

    protected static byte DefaultMaxChunkCount = 6;

    public byte MaxChunkCount
    {
        get;
        set;
    } = DefaultMaxChunkCount;

    /// <summary>
    ///     Fills <paramref name="room" /> with terrain. The layout decides chunk placement
    ///     within the room. <paramref name="branch" /> supplies the level-scope defaults that
    ///     <paramref name="fragment" /> overrides may inherit from.
    /// </summary>
    /// <param name="doorwayDemand">
    ///     Number of doorway tiles the room must end up with in each <see cref="Direction" />,
    ///     keyed by the direction the corresponding inter-room edge leaves (or enters) the
    ///     room. Implementations must place chunks so that every entry's demand can be met
    ///     by walkable / wall doorway tiles in the matching half-plane (or quadrant for
    ///     diagonals) of the room; if the layout cannot satisfy the demand it must return
    ///     <c>null</c> so the caller can retry generation with a different seed.
    /// </param>
    /// <returns>
    ///     The chunks that make up this room, or <c>null</c> if the demand could not be met.
    /// </returns>
    public abstract List<Chunk>? Fill(
        RoomComponent room,
        Branch branch,
        DefiningMapFragment fragment,
        Dictionary<Direction, int> doorwayDemand,
        RoomParameters roomParameters);

    /// <summary>
    ///     Returns <c>true</c> when the doorway points on this room's chunks number at
    ///     least the total edge demand summed over every direction. <c>false</c> means the
    ///     layout must produce a different chunk set (or fail) for the room to be usable
    ///     by <see cref="LevelLayout.MaterializeGraph" />.
    /// </summary>
    protected static bool SatisfiesDemand(
        RoomComponent room,
        IReadOnlyList<Chunk> chunks,
        IReadOnlyDictionary<Direction, int> demand)
    {
        // Every placeable feature the room asks for must have been bound by a fragment
        // payload glyph during placement. Trapped is exempt because BindWildcardFeatures
        // intentionally omits it -- multiple trap-bearing fragments may co-exist in a
        // room and binding the bit would suppress later trap placements.
        var unboundFeatures = room.Features & MapFragment.PlaceableFeaturesMask
            & ~RoomFeatures.Trapped
            & ~room.BoundPlaceableFeatures;
        if (unboundFeatures != 0)
        {
            room.Level.GenerationStats.RoomLayoutUnboundFeatures++;
            return false;
        }

        var required = 0;
        foreach (var (_, count) in demand)
        {
            if (count > 0)
            {
                required += count;
            }
        }

        if (required == 0)
        {
            return true;
        }

        var supply = 0;
        for (var i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            if (chunk.Room == room)
            {
                supply += chunk.DoorwayPoints.Count;
            }
        }

        if (supply < required)
        {
            room.Level.GenerationStats.RoomLayoutInsufficientDoorways++;
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Places the level's defining fragment as the first chunk inside <paramref name="room" />.
    /// </summary>
    /// <remarks>
    ///     The placement context carries the room's <paramref name="roomParameters" /> so
    ///     edge-condition evaluation against the room's actual background fill works
    ///     correctly (e.g. cavern branches pre-filled with <see cref="MapFeature.RockFloor" />).
    ///     The defining fragment's cells are NOT registered in
    ///     <see cref="RoomParameters.ChunkCells" /> — the <see cref="Chunk" /> constructor
    ///     skips registration when the fragment is a <see cref="DefiningMapFragment" /> so the
    ///     walkable-wins overlap rule continues to treat the defining fragment's floor as
    ///     background fill (overwritable by surrounding chunks' walls).
    /// </remarks>
    protected static List<Chunk> PlaceDefiningMapFragment(
        RoomComponent room, DefiningMapFragment fragment, RoomParameters roomParameters)
    {
        var chunks = new List<Chunk>();
        var chunk = fragment.TryPlace(room,
            new LotPlacementContext(room.BoundingRectangle, roomParameters));
        if (chunk != null)
        {
            chunks.Add(chunk);
        }

        return chunks;
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
    protected static bool NeedsLevelConnection(RoomComponent room)
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

    /// <summary>
    ///     Iterates the weighted fragment sequence in order, returning the first chunk
    ///     that successfully places into the lot described by <paramref name="ctx" />. The
    ///     doorway demand for the lot is taken from <see cref="LotPlacementContext.DemandedDoorways" />.
    /// </summary>
    protected static Chunk? TryPlace(
        RoomComponent room,
        in LotPlacementContext ctx,
        IEnumerable<MapFragment> sortedFragments)
    {
        foreach (var nextFragment in sortedFragments)
        {
            var placedChunk = nextFragment.TryPlace(room, ctx);
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
    ///     Picks the lot edge that already touches an existing chunk (so the next fragment
    ///     will be biased to grow off the placed structure). When the lot touches multiple
    ///     existing chunks the side with the longest contact run is chosen. Returns <c>null</c>
    ///     when the lot is strictly interior or only touches the room perimeter, or when
    ///     <paramref name="placedChunks" /> is <c>null</c> / empty.
    /// </summary>
    public static Direction? PickTargetEdge(Rectangle lot, RectangleIntervalTree? placedChunks)
    {
        if (placedChunks == null)
        {
            return null;
        }

        var bestDirection = (Direction?)null;
        var bestContact = 0;
        TryContactOnSide(Direction.North, lot, placedChunks, ref bestDirection, ref bestContact);
        TryContactOnSide(Direction.South, lot, placedChunks, ref bestDirection, ref bestContact);
        TryContactOnSide(Direction.West, lot, placedChunks, ref bestDirection, ref bestContact);
        TryContactOnSide(Direction.East, lot, placedChunks, ref bestDirection, ref bestContact);
        return bestDirection;
    }

    private static void TryContactOnSide(
        Direction direction, Rectangle lot, RectangleIntervalTree placedChunks,
        ref Direction? bestDirection, ref int bestContact)
    {
        Rectangle strip;
        var roomBounds = placedChunks.BoundingRectangle;
        switch (direction)
        {
            case Direction.North:
                if (lot.TopLeft.Y == 0)
                {
                    return;
                }
                strip = new Rectangle(
                    new Point(lot.TopLeft.X, (byte)(lot.TopLeft.Y - 1)),
                    new Point(lot.BottomRight.X, (byte)(lot.TopLeft.Y - 1)));
                break;
            case Direction.South:
                if (lot.BottomRight.Y >= roomBounds.BottomRight.Y)
                {
                    return;
                }
                strip = new Rectangle(
                    new Point(lot.TopLeft.X, (byte)(lot.BottomRight.Y + 1)),
                    new Point(lot.BottomRight.X, (byte)(lot.BottomRight.Y + 1)));
                break;
            case Direction.West:
                if (lot.TopLeft.X == 0)
                {
                    return;
                }
                strip = new Rectangle(
                    new Point((byte)(lot.TopLeft.X - 1), lot.TopLeft.Y),
                    new Point((byte)(lot.TopLeft.X - 1), lot.BottomRight.Y));
                break;
            case Direction.East:
                if (lot.BottomRight.X >= roomBounds.BottomRight.X)
                {
                    return;
                }
                strip = new Rectangle(
                    new Point((byte)(lot.BottomRight.X + 1), lot.TopLeft.Y),
                    new Point((byte)(lot.BottomRight.X + 1), lot.BottomRight.Y));
                break;
            default:
                return;
        }

        var horizontal = direction is Direction.North or Direction.South;
        var contact = 0;
        foreach (var chunkBounds in placedChunks.GetOverlapping(strip))
        {
            if (horizontal)
            {
                var overlapStart = Math.Max(chunkBounds.TopLeft.X, strip.TopLeft.X);
                var overlapEnd = Math.Min(chunkBounds.BottomRight.X, strip.BottomRight.X);
                if (overlapEnd >= overlapStart)
                {
                    contact += overlapEnd - overlapStart + 1;
                }
            }
            else
            {
                var overlapStart = Math.Max(chunkBounds.TopLeft.Y, strip.TopLeft.Y);
                var overlapEnd = Math.Min(chunkBounds.BottomRight.Y, strip.BottomRight.Y);
                if (overlapEnd >= overlapStart)
                {
                    contact += overlapEnd - overlapStart + 1;
                }
            }
        }

        if (contact > bestContact)
        {
            bestContact = contact;
            bestDirection = direction;
        }
    }

    /// <summary>
    ///     Cardinal directions on which <paramref name="lot" /> is flush with the
    ///     <paramref name="room" /> perimeter and the matching entry in
    ///     <paramref name="remainingDemand" /> is still positive. Used to populate
    ///     <see cref="LotPlacementContext.DemandedDoorways" /> so the fragment-selection
    ///     weight can favour fragments whose authored <c>door-{n|s|e|w}</c> tags line up
    ///     with the lot's unmet directional demand, and so dynamic placers can bias their
    ///     payload rectangle toward those edges.
    /// </summary>
    /// <remarks>
    ///     Cardinal demands always set their flag when the lot touches the matching edge.
    ///     An ordinal demand (e.g. <see cref="Direction.Northeast" />) is satisfied only
    ///     when <em>both</em> adjacent cardinal flags are already set; if just one is set,
    ///     the other one is added (when the lot touches it) so two ordinal demands cause
    ///     two flags to be set rather than collapsing onto one. When neither adjacent
    ///     cardinal is set yet, one of the two is picked &#8212; randomly via
    ///     <c>room.Level.GenerationRandom</c> if the lot touches both edges, otherwise the
    ///     touching one. The popcount of the returned flags drives
    ///     <see cref="LotPlacementContext.MinDoorways" />.
    /// </remarks>
    protected static DirectionFlags ComputeLotDemandedDoorways(
        RoomComponent room,
        Rectangle lot,
        Dictionary<Direction, int> remainingDemand)
    {
        if (remainingDemand.Count == 0)
        {
            return DirectionFlags.None;
        }

        var touchesWest = lot.TopLeft.X == 0;
        var touchesEast = lot.BottomRight.X == room.Width - 1;
        var touchesNorth = lot.TopLeft.Y == 0;
        var touchesSouth = lot.BottomRight.Y == room.Height - 1;

        var flags = DirectionFlags.None;

        // Pass 1: cardinal demands deterministically claim their adjacent edge.
        foreach (var (direction, required) in remainingDemand)
        {
            if (required <= 0)
            {
                continue;
            }

            switch (direction)
            {
                case Direction.West when touchesWest:
                    flags |= DirectionFlags.West;
                    break;
                case Direction.East when touchesEast:
                    flags |= DirectionFlags.East;
                    break;
                case Direction.North when touchesNorth:
                    flags |= DirectionFlags.North;
                    break;
                case Direction.South when touchesSouth:
                    flags |= DirectionFlags.South;
                    break;
            }
        }

        // Pass 2: ordinal demands. Each demand is satisfied only when *both* adjacent
        // cardinals are already flagged; if just one is set, add the other (so two
        // separate ordinal demands set two flags). Otherwise pick a cardinal — randomly
        // when both touch, deterministically when only one does.
        var random = room.Level.GenerationRandom;
        foreach (var (direction, required) in remainingDemand)
        {
            if (required <= 0)
            {
                continue;
            }

            DirectionFlags firstFlag;
            bool firstTouches;
            DirectionFlags secondFlag;
            bool secondTouches;
            switch (direction)
            {
                case Direction.Northwest:
                    firstFlag = DirectionFlags.North;
                    firstTouches = touchesNorth;
                    secondFlag = DirectionFlags.West;
                    secondTouches = touchesWest;
                    break;
                case Direction.Northeast:
                    firstFlag = DirectionFlags.North;
                    firstTouches = touchesNorth;
                    secondFlag = DirectionFlags.East;
                    secondTouches = touchesEast;
                    break;
                case Direction.Southwest:
                    firstFlag = DirectionFlags.South;
                    firstTouches = touchesSouth;
                    secondFlag = DirectionFlags.West;
                    secondTouches = touchesWest;
                    break;
                case Direction.Southeast:
                    firstFlag = DirectionFlags.South;
                    firstTouches = touchesSouth;
                    secondFlag = DirectionFlags.East;
                    secondTouches = touchesEast;
                    break;
                default:
                    continue;
            }

            var firstSet = (flags & firstFlag) != 0;
            var secondSet = (flags & secondFlag) != 0;
            if (firstSet && secondSet)
            {
                continue;
            }

            if (firstSet)
            {
                if (secondTouches)
                {
                    flags |= secondFlag;
                }
                continue;
            }

            if (secondSet)
            {
                if (firstTouches)
                {
                    flags |= firstFlag;
                }
                continue;
            }

            if (firstTouches && secondTouches)
            {
                flags |= random.Next(2) == 0 ? firstFlag : secondFlag;
            }
            else if (firstTouches)
            {
                flags |= firstFlag;
            }
            else if (secondTouches)
            {
                flags |= secondFlag;
            }
        }

        return flags;
    }

    /// <summary>
    ///     Subtracts the doorway contribution of <paramref name="chunk" /> from
    ///     <paramref name="remainingDemand" />. Each doorway tile that is not already
    ///     used by an existing connection decrements every direction it satisfies.
    /// </summary>
    protected static void DeductChunkDoorways(
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
        room.Level.GenerationStats.RecordFragmentInstance(fragment.SourceName, fragment.Tags, room.Features);
        room.Level.Game.GenerationStats.RecordFragmentInstance(fragment.SourceName, fragment.Tags, room.Features);
        room.Level.GenerationStats.RecordFragmentVariant(fragment.Name);
    }

    /// <summary>
    ///     Returns the first chunk in <paramref name="chunks" /> whose interior is not
    ///     reachable from <paramref name="chunks" />[0] across <paramref name="room" />'s
    ///     realized walkable terrain, or <c>null</c> when every chunk is mutually
    ///     reachable. Reachability is evaluated with 8-connected (diagonal) moves to
    ///     match in-game actor movement. Callers can pass the returned chunk to
    ///     <see cref="Connect" /> to carve a corridor toward it and re-check, or declare
    ///     the layout attempt failed and restart.
    ///     <para>
    ///         Synthetic <see cref="NormalMapFragmentData.DeferredFillFragment" /> chunks (produced
    ///         by <see cref="MapFragment.FixupDeferredConditions" /> to record gap-fill
    ///         rectangles) are skipped: they carry no doorway points so they cannot be
    ///         used as a corridor endpoint, and their walkable cells are already part of
    ///         the flood-fill reachability set via <paramref name="room" />'s terrain.
    ///     </para>
    /// </summary>
    protected static Chunk? EnsureConnectivity(RoomComponent room, List<Chunk> chunks)
    {
        if (chunks.Count <= 1)
        {
            return null;
        }

        var reached = new HashSet<Point>();
        FloodFillReachable(room, chunks[0].InsidePoints, reached);
        for (var i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            if (!IsChunkReached(chunk, reached))
            {
                return chunk;
            }
        }

        return null;
    }

    /// <summary>
    ///     Iteratively detects unreached chunks via <see cref="EnsureConnectivity" /> and
    ///     carves corridors with <see cref="Connect" /> from the unreached chunk's
    ///     orthogonally-nearest reached chunk. Returns true when every chunk is mutually
    ///     reachable; returns false when a corridor cannot be carved so the caller can
    ///     declare the layout attempt failed and restart.
    /// </summary>
    protected bool TryRepairConnectivity(RoomComponent room, List<Chunk> chunks)
    {
        // Outer bound: every successful Connect unions at least one previously-unreached
        // chunk into the reached set, so termination is guaranteed.
        for (var iteration = 0; iteration < chunks.Count; iteration++)
        {
            var unreached = EnsureConnectivity(room, chunks);
            if (unreached == null)
            {
                return true;
            }

            var nearestReached = PickNearestReachedChunk(room, chunks, unreached);
            if (nearestReached == null || !Connect(nearestReached, unreached))
            {
                return false;
            }
        }

        return EnsureConnectivity(room, chunks) == null;
    }

    private static Chunk? PickNearestReachedChunk(
        RoomComponent room, List<Chunk> chunks, Chunk target)
    {
        var reached = new HashSet<Point>();
        FloodFillReachable(room, chunks[0].InsidePoints, reached);

        Chunk? nearest = null;
        var nearestDistance = int.MaxValue;
        for (var i = 0; i < chunks.Count; i++)
        {
            var candidate = chunks[i];
            if (candidate == target
                || candidate.Fragment.Name == NormalMapFragmentData.DeferredFillFragment.Name
                || !IsChunkReached(candidate, reached))
            {
                // Skip the target itself and synthetic deferred-fill chunks (their
                // payloads have no authored perimeter, so the corridor planner has
                // nothing concrete to carve from their bounds). Open-perimeter chunks
                // WITHOUT '+' doorways are still valid anchors —
                // Chunk.GetGoodConnectionPoint falls back to inside points for them.
                continue;
            }

            var distance = candidate.BoundingRectangle.OrthogonalDistanceTo(target.BoundingRectangle);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = candidate;
            }
        }

        return nearest;
    }

    /// <summary>
    ///     Returns whether <paramref name="chunk" /> is mutually reachable with the
    ///     reference chunk (<paramref name="reached" /> contains the flood-fill from
    ///     it). Synthetic <see cref="NormalMapFragmentData.DeferredFillFragment" />
    ///     chunks are reported as reached regardless: their cells are pure floor
    ///     adjacent to the chunks whose preserved conditions produced them and become
    ///     part of the flood-fill set the next time it runs. For all other chunks
    ///     (including open-perimeter ones without `+` doorways) we walk the inside
    ///     points and return true on first hit.
    /// </summary>
    private static bool IsChunkReached(Chunk chunk, HashSet<Point> reached)
    {
        if (chunk.Fragment.Name == NormalMapFragmentData.DeferredFillFragment.Name)
        {
            return true;
        }

        foreach (var p in chunk.InsidePoints)
        {
            if (reached.Contains(p))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Flood-fills <paramref name="reached" /> from the union of <paramref name="seedPoints" />
    ///     across all 8-connected neighbours whose terrain satisfies
    ///     <see cref="MapFeatureExtensions.CanMoveTo" />. Diagonal expansion matches in-game
    ///     actor movement so connectivity here reflects whether actors can actually traverse
    ///     between chunks. Seed points are added unconditionally so a chunk with a non-floor
    ///     interior tile (e.g. a feature glyph cell) still counts as reachable; expansion past
    ///     the seed still requires walkable terrain.
    /// </summary>
    private static void FloodFillReachable(
        RoomComponent room, IReadOnlySet<Point> seedPoints, HashSet<Point> reached)
    {
        var queue = new Queue<Point>();
        foreach (var seed in seedPoints)
        {
            if (reached.Add(seed))
            {
                queue.Enqueue(seed);
            }
        }

        var rect = room.BoundingRectangle;
        var minX = rect.TopLeft.X;
        var minY = rect.TopLeft.Y;
        var maxX = rect.BottomRight.X;
        var maxY = rect.BottomRight.Y;
        var pointToIndex = room.PointToIndex!;

        while (queue.Count > 0)
        {
            var p = queue.Dequeue();
            TryExpand(p.X - 1, p.Y, minX, minY, maxX, maxY, room, pointToIndex, reached, queue);
            TryExpand(p.X + 1, p.Y, minX, minY, maxX, maxY, room, pointToIndex, reached, queue);
            TryExpand(p.X, p.Y - 1, minX, minY, maxX, maxY, room, pointToIndex, reached, queue);
            TryExpand(p.X, p.Y + 1, minX, minY, maxX, maxY, room, pointToIndex, reached, queue);
            TryExpand(p.X - 1, p.Y - 1, minX, minY, maxX, maxY, room, pointToIndex, reached, queue);
            TryExpand(p.X + 1, p.Y - 1, minX, minY, maxX, maxY, room, pointToIndex, reached, queue);
            TryExpand(p.X - 1, p.Y + 1, minX, minY, maxX, maxY, room, pointToIndex, reached, queue);
            TryExpand(p.X + 1, p.Y + 1, minX, minY, maxX, maxY, room, pointToIndex, reached, queue);
        }
    }

    private static void TryExpand(
        int x, int y, int minX, int minY, int maxX, int maxY,
        RoomComponent room, short[,] pointToIndex, HashSet<Point> reached, Queue<Point> queue)
    {
        if (x < minX || x > maxX || y < minY || y > maxY)
        {
            return;
        }

        var index = pointToIndex[x, y];
        if (!room.Terrain[index].CanMoveTo())
        {
            return;
        }

        var pt = new Point((byte)x, (byte)y);
        if (reached.Add(pt))
        {
            queue.Enqueue(pt);
        }
    }

    /// <summary>
    ///     Carves a corridor between <paramref name="source" /> and <paramref name="target" />.
    ///     Probes both directions through <see cref="FindCorridorPath" /> and keeps the
    ///     path that traverses fewer non-corridor (wall / feature) tiles.
    /// </summary>
    protected bool Connect(Chunk source, Chunk target,
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
                var isTarget = nIdx == targetIdx;
                var stepCost = isTarget
                    ? 0
                    : GetCorridorStepCost(room.Terrain[nIdx]);

                // Impassable tiles (fragment features like DeityAltar/SmallFountain,
                // pools, statues, etc.) report int.MaxValue. Skip them outright.
                if (stepCost == int.MaxValue)
                {
                    continue;
                }

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
        var safetyBudget = room.TileCount + 1;
        while (cursor != -1)
        {
            if (--safetyBudget < 0)
            {
                throw new InvalidOperationException(
                    $"FindCorridorPath parent chain exceeded TileCount={room.TileCount} iterations "
                    + $"(source={source}, target={target}); likely cycle in parent[].");
            }

            var point = indexToPoint[cursor];
            if (GetCorridorStepCost(room.Terrain[cursor]) >= 1000)
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
     => GetCorridorStepCost(room.Terrain[room.PointToIndex![location.X, location.Y]]) < int.MaxValue;

    private static int GetCorridorStepCost(MapFeature feature)
    => feature switch
        {
            MapFeature.Default => 1,
            MapFeature.RockFloor => 1,
            MapFeature.StoneFloor => 1,
            MapFeature.StoneArchway => 1,
            MapFeature.StoneWall => 1000,
            MapFeature.RockWall => 1000,
            _ => int.MaxValue
        };

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
}
