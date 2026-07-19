using System.Buffers;
using UnicornHack.Data.Fragments;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils;

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
    ///     The ordered list of <see cref="RoomDoorwayAnchor" />s describing which perimeter sides the
    ///     room's connections must (or should) be steered onto. Each doorway-demand anchor implies one
    ///     doorway tile the room must expose on its side. Implementations must place chunks so that
    ///     every demanded doorway can be met by walkable / wall doorway tiles in the matching
    ///     half-plane (or quadrant for diagonals) of the room; if the layout cannot satisfy the demand
    ///     it must return <c>null</c> so the caller can retry generation with a different seed.
    /// </param>
    /// <returns>
    ///     The chunks that make up this room, or <c>null</c> if the demand could not be met.
    /// </returns>
    public abstract List<Chunk>? Fill(
        RoomComponent room,
        Branch branch,
        DefiningMapFragment fragment,
        IReadOnlyList<RoomDoorwayAnchor> doorwayDemand,
        RoomParameters roomParameters);

    /// <summary>
    ///     Whether every placeable feature <paramref name="room" /> asks for has been bound by a
    ///     fragment payload glyph during placement. <see cref="RoomFeatures.Trapped" /> is exempt
    ///     because <c>BindWildcardFeatures</c> intentionally omits it — multiple trap-bearing
    ///     fragments may co-exist in a room and binding the bit would suppress later trap placements.
    ///     Records <c>RoomLayoutUnboundFeatures</c> on failure.
    /// </summary>
    protected static bool AllPlaceableFeaturesBound(RoomComponent room)
    {
        var unboundFeatures = room.Features & MapFragment.PlaceableFeaturesMask
            & ~RoomFeatures.Trapped
            & ~room.BoundPlaceableFeatures;
        if (unboundFeatures != 0)
        {
            room.Level.GenerationStats.RoomLayoutUnboundFeatures++;
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Whether the directional doorway demand has been met, judged purely from the room layout's
    ///     running tally of <paramref name="remainingAnchors" />: each doorway-demand anchor is
    ///     retired by <see cref="DeductChunkDoorways" /> the moment a chunk doorway faces its side, so
    ///     a doorway-demand anchor that survives is an unmet directional demand. Positioning-only
    ///     anchors carry no demand and are ignored. This is the authoritative verdict for layouts that
    ///     track anchors incrementally (no need to recount the doorway supply from scratch as
    ///     <see cref="SatisfiesDemand" /> does). Records <c>RoomLayoutInsufficientDoorways</c> on
    ///     failure.
    /// </summary>
    protected static bool DoorwayDemandSatisfied(
        RoomComponent room, IReadOnlyList<RoomDoorwayAnchor> remainingAnchors)
    {
        for (var i = 0; i < remainingAnchors.Count; i++)
        {
            if (remainingAnchors[i].IsDoorwayDemand)
            {
                room.Level.GenerationStats.RoomLayoutInsufficientDoorways++;
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Returns <c>true</c> when the doorway points on this room's chunks number at
    ///     least the total edge demand summed over every direction. <c>false</c> means the
    ///     layout must produce a different chunk set (or fail) for the room to be usable
    ///     by <see cref="LevelLayout.MaterializeGraph" />.
    /// </summary>
    protected static bool SatisfiesDemand(
        RoomComponent room,
        IReadOnlyList<Chunk> chunks,
        IReadOnlyList<RoomDoorwayAnchor> doorwayDemand)
    {
        if (!AllPlaceableFeaturesBound(room))
        {
            return false;
        }

        // Each doorway-demand anchor implies one demanded doorway tile, so the total required count
        // is simply the number of doorway-demand anchors.
        var required = 0;
        for (var i = 0; i < doorwayDemand.Count; i++)
        {
            if (doorwayDemand[i].IsDoorwayDemand)
            {
                required++;
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
    ///     Whether <paramref name="room" /> has a still-pending incoming inter-level connection
    ///     from a neighboring level (its <see cref="ConnectionComponent.TargetLevelX" /> is
    ///     <c>null</c>) that a connecting fragment must <em>receive</em>. This marks the room as
    ///     the level's entrance; once any fragment receives it the connection is no longer
    ///     pending. Wire it up with <see cref="LevelConnection.CreateReceivingConnection" />.
    /// </summary>
    protected static bool NeedsReceivingConnection(RoomComponent room)
    {
        foreach (var connectionEntity in room.IncomingConnections)
        {
            if (connectionEntity.Connection!.TargetLevelX == null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Whether <paramref name="room" /> must <em>source</em> a new outgoing inter-level
    ///     connection. <see cref="RoomFeatures.Exit" /> or <see cref="RoomFeatures.Shortcut" />. Both go through
    ///     <see cref="NeedsOutgoingSourceConnection" />: <c>false</c> when the room already
    ///     sourced one, <c>true</c> only when the target depth is reachable (within the branch,
    ///     or via a next branch when the branch isn't terminal). Wire it up with
    ///     <see cref="LevelConnection.CreateSourceConnection" />.
    /// </summary>
    protected static bool NeedsSourceConnection(RoomComponent room)
    {
        // Exit and Shortcut are symmetric outgoing inter-level sources; they differ only in
        // how far ahead they target (Exit: +1, Shortcut: +2).
        if ((room.Features & RoomFeatures.Exit) != 0)
        {
            return NeedsOutgoingSourceConnection(room, depthDelta: 1);
        }

        if ((room.Features & RoomFeatures.Shortcut) != 0)
        {
            return NeedsOutgoingSourceConnection(room, depthDelta: 2);
        }

        return false;
    }

    /// <summary>
    ///     Shared gate for an outgoing inter-level source room (Exit or Shortcut): returns
    ///     <c>false</c> when the room already sourced a connection, otherwise <c>true</c> iff a
    ///     destination <paramref name="depthDelta" /> levels ahead is reachable — within the
    ///     current branch, or via a next branch when this branch isn't terminal.
    /// </summary>
    private static bool NeedsOutgoingSourceConnection(RoomComponent room, int depthDelta)
    {
        foreach (var connection in room.Connections.Values)
        {
            if (connection.Connection!.Orientation == Direction.Down)
            {
                // Already sourced an outgoing inter-level connection
                return false;
            }
        }

        var branch = room.Level.Branch;
        if (room.Level.Depth + depthDelta <= branch.Length)
        {
            // The target depth still lies within this branch.
            return true;
        }

        // The target is past the end of the branch: only place a connection when the branch
        // isn't terminal (a next branch exists to skip into).
        return !branch.IsTerminal;
    }

    /// <summary>
    ///     The canonical role tag (from <see cref="MapFragment.FeatureTagNames" />) that a
    ///     connecting fragment can carry to be preferred for <paramref name="room" />'s current
    ///     connection need: <c>entrance</c> while it's still receiving a dangling incoming,
    ///     otherwise <c>exit</c> or <c>shortcut</c> for the matching source role. Returns
    ///     <c>null</c> when the room needs no connection. The tag only biases selection weight
    ///     (see <see cref="ConnectingMapFragment.GetWeight" /> callers) — an untagged connecting
    ///     fragment is always still selectable.
    /// </summary>
    public static string? GetConnectingRoleTag(RoomComponent room)
    {
        if (NeedsReceivingConnection(room))
        {
            return MapFragment.FeatureTagNames[RoomFeatures.Entrance];
        }

        if ((room.Features & RoomFeatures.Exit) != 0)
        {
            return MapFragment.FeatureTagNames[RoomFeatures.Exit];
        }

        if ((room.Features & RoomFeatures.Shortcut) != 0)
        {
            return MapFragment.FeatureTagNames[RoomFeatures.Shortcut];
        }

        return null;
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
    ///     <paramref name="room" /> perimeter and the matching doorway demand among
    ///     <paramref name="remainingAnchors" /> is still unmet. Used to populate
    ///     <see cref="LotPlacementContext.DemandedDoorways" /> so the fragment-selection
    ///     weight can favour fragments whose authored <c>door-{n|s|e|w}</c> tags line up
    ///     with the lot's unmet directional demand, and so dynamic placers can bias their
    ///     payload rectangle toward those edges.
    /// </summary>
    /// <remarks>
    ///     The per-direction demand is derived from the doorway-demand anchors still present in
    ///     <paramref name="remainingAnchors" /> (the room layout's single source of truth for both
    ///     steering and demand), so it always agrees with the anchor list.
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
        IReadOnlyList<RoomDoorwayAnchor> remainingAnchors)
    {
        var touchesWest = lot.TopLeft.X == 0;
        var touchesEast = lot.BottomRight.X == room.Width - 1;
        var touchesNorth = lot.TopLeft.Y == 0;
        var touchesSouth = lot.BottomRight.Y == room.Height - 1;

        var flags = DirectionFlags.None;

        // Pass 1: cardinal doorway-demand anchors deterministically claim their adjacent edge. The
        // anchor list is the layout's single source of truth, so this reads it directly (rather than
        // collapsing it into a per-direction count) — a repeated cardinal just re-sets its flag.
        for (var i = 0; i < remainingAnchors.Count; i++)
        {
            var anchor = remainingAnchors[i];
            if (!anchor.IsDoorwayDemand)
            {
                continue;
            }

            switch (anchor.Side)
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

        // Pass 2: ordinal doorway-demand anchors. Each is satisfied only when *both* adjacent
        // cardinals are already flagged; if just one is set, add the other (so two ordinal demands
        // set two flags). Otherwise pick a cardinal — randomly when both touch, deterministically
        // when only one does. Runs after Pass 1 so every cardinal demand is already reflected.
        var random = room.Level.GenerationRandom;
        for (var i = 0; i < remainingAnchors.Count; i++)
        {
            var anchor = remainingAnchors[i];
            if (!anchor.IsDoorwayDemand)
            {
                continue;
            }

            DirectionFlags firstFlag;
            bool firstTouches;
            DirectionFlags secondFlag;
            bool secondTouches;
            switch (anchor.Side)
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
    ///     Retires the anchors satisfied by <paramref name="chunk" /> from
    ///     <paramref name="remainingAnchors" /> — the room layout's single source of truth that drives
    ///     both lot steering (front anchor) and the per-direction doorway demand. The two anchor
    ///     kinds retire on their own trigger, so the demand and the steering always agree:
    ///     <list type="bullet">
    ///         <item>
    ///             A doorway-demand anchor retires once one of <paramref name="chunk" />'s free doorway
    ///             tiles (not already used by an existing connection) faces its side — at most one
    ///             demand anchor per side per tile, mirroring the single per-direction doorway a tile
    ///             supplies. Once a side's doorways are all supplied no further chunk is steered there.
    ///         </item>
    ///         <item>
    ///             A positioning-only anchor (which carries no doorway demand of its own, e.g. the
    ///             entrance's side opposite its outgoing connection) retires once <paramref name="chunk" />
    ///             hosts an inter-level connection: the chunk that places the connection resolves the
    ///             need the anchor was reserving space for. Because the defining fragment can itself
    ///             place connections, this also retires positioning-only anchors on the first chunk.
    ///         </item>
    ///     </list>
    /// </summary>
    protected static void DeductChunkDoorways(
        RoomComponent room,
        Chunk chunk,
        List<RoomDoorwayAnchor> remainingAnchors)
    {
        if (remainingAnchors.Count == 0 || chunk.Room != room)
        {
            return;
        }

        if (chunk.DoorwayPoints.Count > 0)
        {
            var centerX = room.Width / 2;
            var centerY = room.Height / 2;

            // A single doorway tile may face several sides (e.g. a north-east tile faces North, East
            // and Northeast) but must retire at most one demand anchor per side, so this tracks which
            // sides it has already satisfied during the scan of one point.
            Span<bool> satisfiedThisPoint = stackalloc bool[(int)Direction.Down + 1];

            foreach (var p in chunk.DoorwayPoints)
            {
                if (room.Connections.ContainsKey(p))
                {
                    continue;
                }

                satisfiedThisPoint.Clear();
                var dx = p.X - centerX;
                var dy = p.Y - centerY;

                for (var i = remainingAnchors.Count - 1; i >= 0; i--)
                {
                    var anchor = remainingAnchors[i];
                    if (!anchor.IsDoorwayDemand || satisfiedThisPoint[(int)anchor.Side])
                    {
                        continue;
                    }

                    if (anchor.Side.IsInDirection(dx, dy))
                    {
                        satisfiedThisPoint[(int)anchor.Side] = true;
                        remainingAnchors.RemoveAt(i);
                    }
                }
            }
        }

        // Positioning-only anchors carry no doorway demand; they reserve a side until the inter-level
        // connection they were making room for is placed. A chunk (including the defining one) that
        // owns a connection cell resolves that need, so retire every positioning-only anchor. A chunk
        // "owns" a connection when one of the room's connection cells falls within its bounding
        // rectangle: connecting fragments wire their connection at an interior cell during placement,
        // the UniformRoomLayout fallback at a perimeter doorway — either way the cell is inside the
        // chunk's bounds. (Because the positioning anchors are removed the first time a chunk reports
        // a connection, a later chunk overlapping an already-counted connection is a harmless no-op.)
        var hostsConnection = false;
        foreach (var cell in room.Connections.Keys)
        {
            if (chunk.BoundingRectangle.Contains(cell))
            {
                hostsConnection = true;
                break;
            }
        }

        if (hostsConnection)
        {
            for (var i = remainingAnchors.Count - 1; i >= 0; i--)
            {
                if (!remainingAnchors[i].IsDoorwayDemand)
                {
                    remainingAnchors.RemoveAt(i);
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
    ///     Iteratively detects unreached chunks via <see cref="FirstUnreachedChunk" /> and
    ///     carves corridors with <see cref="Connect" /> from the unreached chunk's
    ///     orthogonally-nearest reached chunk. Returns true when every chunk is mutually
    ///     reachable; returns false when a corridor cannot be carved so the caller can
    ///     declare the layout attempt failed and restart.
    /// </summary>
    protected bool TryRepairConnectivity(
        RoomComponent room, List<Chunk> chunks,
        Dictionary<Point, EdgeConditionKind[]> preservedConditions,
        RoomParameters roomParameters)
    {
        var pointToIndex = room.PointToIndex!;
        var tileCount = room.TileCount;
        var corridorPathWorkspace = new WeightedGraphSearch.PathWorkspace(tileCount);
        // Pooled visited mask indexed by cell index, reused across every flood-fill in this repair
        // (cleared per iteration) instead of allocating a HashSet<Point> each time.
        var reached = ArrayPool<bool>.Shared.Rent(tileCount);
        try
        {
            // First try the corridor-based repair: it carves doorway-to-doorway corridors that look
            // intentional.
            var lastReachedCount = -1;
            for (var iteration = 0; iteration < chunks.Count; iteration++)
            {
                Array.Clear(reached, 0, tileCount);
                var reachedCount = FloodFillReachable(room, chunks[0].InsidePoints, reached);

                var unreached = FirstUnreachedChunk(chunks, reached, pointToIndex);
                if (unreached == null)
                {
                    return true;
                }

                // Bail to the guaranteed bridge the moment a Connect fails to grow the reached set:
                // its cost-minimizing pathfinder prefers existing open space, so when a chunk's
                // cheapest route runs through a *separate* disconnected walkable region it links the
                // two unreached regions together instead of bridging back, and the loop would spin.
                if (reachedCount <= lastReachedCount)
                {
                    break;
                }

                lastReachedCount = reachedCount;

                // reached already holds the flood from chunks[0], so PickNearestReachedChunk reuses
                // it rather than recomputing the same fill.
                var nearestReached = PickNearestReachedChunk(room, chunks, unreached, reached);
                if (nearestReached == null || !Connect(
                nearestReached, unreached,
                preservedConditions,
                roomParameters,
                corridorPathWorkspace))
                {
                    break;
                }
            }
        }
        finally
        {
            ArrayPool<bool>.Shared.Return(reached, clearArray: false);
        }

        // Guarantee connectivity with a wall-carving bridge that always reduces the number of
        // disconnected walkable components.
        return ForceConnectWalkableComponents(room, roomParameters);
    }

    private static Chunk? FirstUnreachedChunk(List<Chunk> chunks, bool[] reached, short[,] pointToIndex)
    {
        for (var i = 0; i < chunks.Count; i++)
        {
            if (!IsChunkReached(chunks[i], reached, pointToIndex))
            {
                return chunks[i];
            }
        }

        return null;
    }

    /// <summary>
    ///     Guarantees every walkable cell in <paramref name="room" /> is mutually reachable by
    ///     repeatedly carving the cheapest wall barrier between the component grown from the first
    ///     walkable cell and the nearest cell of any other walkable component. Walls and unfilled
    ///     background are carvable (cost 1); impassable features and the room edge are not, so this
    ///     returns <c>false</c> only when a walkable cell is genuinely sealed off behind features
    ///     and the layout attempt must restart.
    ///     <para>
    ///         Connection tiles are treated as non-traversable sinks:
    ///         a region reachable only by stepping through a connection is stranded (the step
    ///         teleports the actor away), so this carves a real corridor to it. This also makes the
    ///         method safe to re-run after <see cref="LevelLayout.MaterializeGraph" /> wires the
    ///         intra-level connections, which is where such stranding is introduced.
    ///     </para>
    /// </summary>
    public static bool ForceConnectWalkableComponents(RoomComponent room, RoomParameters roomParameters)
    {
        var pointToIndex = room.PointToIndex!;
        var width = room.Width;
        var height = room.Height;

        Point? seed = null;
        for (var y = 0; y < height && seed == null; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var cell = new Point((byte)x, (byte)y);
                if (room.Terrain[pointToIndex[x, y]].CanMoveTo() && !room.Connections.ContainsKey(cell))
                {
                    seed = cell;
                    break;
                }
            }
        }

        if (seed == null)
        {
            return true;
        }

        var tileCount = room.TileCount;
        var reached = ArrayPool<bool>.Shared.Rent(tileCount);
        try
        {
            // Each iteration grows the seed component and bridges one further component into it, so
            // the loop runs at most once per walkable cell.
            for (var guard = 0; guard <= tileCount; guard++)
            {
                Array.Clear(reached, 0, tileCount);
                FloodFillReachable(room, new[] { seed.Value }, reached);

                Point? target = null;
                for (var y = 0; y < height && target == null; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var cell = new Point((byte)x, (byte)y);
                        if (room.Terrain[pointToIndex[x, y]].CanMoveTo()
                            && !reached[pointToIndex[x, y]]
                            && !room.Connections.ContainsKey(cell))
                        {
                            target = cell;
                            break;
                        }
                    }
                }

                if (target == null)
                {
                    return true;
                }

                if (!CarveShortestBridge(room, reached, roomParameters))
                {
                    return false;
                }
            }
        }
        finally
        {
            ArrayPool<bool>.Shared.Return(reached, clearArray: false);
        }

        // Unreachable: each iteration bridges one disconnected component into the seed
        // component, and there are fewer than TileCount components, so the loop always exits
        // above via target == null. Returning true keeps the guarantee if that ever changes.
        return true;
    }

    /// <summary>
    ///     Multi-source Dijkstra from every cell in <paramref name="reached" /> to the nearest
    ///     walkable cell outside it, minimizing the number of carved walls. Stepping onto an
    ///     already-walkable cell is free; a wall or unfilled background costs one carve; impassable
    ///     features and the room edge are blocked. The walls on the discovered path are carved to
    ///     archway / floor so the outside component joins the reached set. Returns <c>false</c>
    ///     when no walkable cell outside the reached set can be reached by carving walls.
    /// </summary>
    private static bool CarveShortestBridge(RoomComponent room, bool[] reached, RoomParameters roomParameters)
    {
        var pointToIndex = room.PointToIndex!;
        var indexToPoint = room.IndexToPoint!;
        var tileCount = room.TileCount;

        // Pooled scratch arrays (only the [0, tileCount) prefix is touched, so a larger rented
        // buffer is fine). Returned in the finally below.
        var dist = ArrayPool<int>.Shared.Rent(tileCount);
        var parent = ArrayPool<short>.Shared.Rent(tileCount);
        try
        {
            for (var i = 0; i < tileCount; i++)
            {
                dist[i] = int.MaxValue;
                parent[i] = -1;
            }

            var open = new PriorityQueue<short, int>();
            for (short idx = 0; idx < tileCount; idx++)
            {
                // Connection tiles are reached sinks — never grow the bridge out of one, or it would
                // "connect" the two sides of a portal that the actor can't actually walk across.
                if (reached[idx] && !room.Connections.ContainsKey(indexToPoint[idx]))
                {
                    dist[idx] = 0;
                    open.Enqueue(idx, 0);
                }
            }

            var targetIdx = (short)-1;
            while (open.TryDequeue(out var currentIdx, out var currentDist))
            {
                if (currentDist > dist[currentIdx])
                {
                    continue;
                }

                var current = indexToPoint[currentIdx];

                // A walkable cell that isn't part of the reached component is the bridge target —
                // the cheapest such cell pops first.
                if (currentDist > 0 && room.Terrain[currentIdx].CanMoveTo())
                {
                    targetIdx = currentIdx;
                    break;
                }

                foreach (var neighbor in Neighbors8(room, current.X, current.Y))
                {
                    // Can't route a bridge through a connection.
                    if (room.Connections.ContainsKey(neighbor))
                    {
                        continue;
                    }

                    var nIdx = pointToIndex[neighbor.X, neighbor.Y];
                    var feature = room.Terrain[nIdx];
                    int stepCost;
                    if (feature.CanMoveTo())
                    {
                        stepCost = 0;
                    }
                    else if (feature is MapFeature.StoneWall or MapFeature.RockWall or MapFeature.Default)
                    {
                        stepCost = 1;
                    }
                    else
                    {
                        // Impassable feature (altar, fountain, pedestal, …) — can't carve through.
                        continue;
                    }

                    var candidate = currentDist + stepCost;
                    if (candidate >= dist[nIdx])
                    {
                        continue;
                    }

                    dist[nIdx] = candidate;
                    parent[nIdx] = currentIdx;
                    open.Enqueue(nIdx, candidate);
                }
            }

            if (targetIdx < 0)
            {
                return false;
            }

            // Carve the walls / background on the path between the reached set and the target.
            var pathTerrain = roomParameters.DefaultPathTerrain;
            var cursor = targetIdx;
            while (cursor != -1 && dist[cursor] > 0)
            {
                var feature = room.Terrain[cursor];
                if (!feature.CanMoveTo())
                {
                    var point = indexToPoint[cursor];
                    TerrainSystem.SetTerrain(
                        feature == MapFeature.Default ? pathTerrain : MapFeature.StoneArchway,
                        point, room);
                }

                cursor = parent[cursor];
            }

            return true;
        }
        finally
        {
            ArrayPool<int>.Shared.Return(dist);
            ArrayPool<short>.Shared.Return(parent);
        }
    }

    /// <summary>
    ///     Converts every unfilled (<see cref="MapFeature.Default" />) cell that is 8-adjacent to a
    ///     walkable tile into the branch's default wall (<paramref name="wall" /> — see
    ///     <see cref="RoomParameters.DefaultWall" />), walling off chunk floor and
    ///     carved corridors that ended up flush with the void between chunks — the
    ///     "chunks walled off" invariant. Only background void is touched (walkable terrain is never
    ///     modified) and walls don't block diagonal movement, so connectivity is always preserved.
    ///     The walkable set is invariant during the scan, so a single pass suffices.
    ///     <para>
    ///         Walkable tiles flush with the room's outer boundary are intentionally left alone: the
    ///         level boundary is itself an impassable wall (<see cref="TravelSystem" /> forbids
    ///         moving out of bounds), so such tiles are already enclosed — and walling them would
    ///         erode legitimate floor placed against the boundary.
    ///     </para>
    ///     <para>
    ///         Called both at the end of room fill and by
    ///         <see cref="LevelLayout.MaterializeGraph" /> after it carves inter-room archways,
    ///         since turning a perimeter wall into a walkable archway can newly expose the void on
    ///         the archway's flanks.
    ///     </para>
    /// </summary>
    public static void SealUnfilledAroundWalkables(RoomComponent room, MapFeature wall)
    {
        var pointToIndex = room.PointToIndex!;
        for (var y = 0; y < room.Height; y++)
        {
            for (var x = 0; x < room.Width; x++)
            {
                if (room.Terrain[pointToIndex[x, y]] != MapFeature.Default)
                {
                    continue;
                }

                if (HasWalkableNeighbor(room, x, y))
                {
                    var cell = new Point((byte)x, (byte)y);
                    TerrainSystem.SetTerrain(PickAdjacentWall(room, cell, wall), cell, room);
                }
            }
        }
    }

    /// <summary>
    ///     Picks the wall material for a synthesized seal at <paramref name="point" />: an existing
    ///     <see cref="MapFragment.IsWallFeature">wall</see> among its 8 neighbors when one exists (so
    ///     the seal extends the surrounding wall line instead of introducing a foreign wall type),
    ///     otherwise <paramref name="defaultWall" />. This is the single source of truth every place
    ///     that synthesizes a wall tile (room sealing, corridor enclosure, fixup fills) shares, so a
    ///     sealed cell always matches its neighboring walls.
    /// </summary>
    public static MapFeature PickAdjacentWall(RoomComponent room, Point point, MapFeature defaultWall)
    {
        var pointToIndex = room.PointToIndex!;
        foreach (var neighbor in Neighbors8(room, point.X, point.Y))
        {
            var neighborFeature = room.Terrain[pointToIndex[neighbor.X, neighbor.Y]];
            if (MapFragment.IsWallFeature(neighborFeature))
            {
                return neighborFeature;
            }
        }

        return defaultWall;
    }

    /// <summary>
    ///     Allocation-free enumeration of the in-bounds 8-connected grid neighbors of
    ///     <c>(x, y)</c> within <paramref name="room" />. The returned value type is its own
    ///     enumerator, so a <c>foreach</c> over it never heap-allocates. Out-of-bounds neighbors
    ///     are skipped, so a corner cell yields three neighbors and an edge cell five.
    /// </summary>
    public static NeighborWalker Neighbors8(RoomComponent room, int x, int y)
        => new(x, y, 0, 0, room.Width - 1, room.Height - 1, includeDiagonals: true);

    /// <summary>
    ///     Like <see cref="Neighbors8(RoomComponent, int, int)" /> but yields only the four cardinal
    ///     neighbors when <paramref name="includeDiagonals" /> is <c>false</c>. The cardinals are the
    ///     first four of <see cref="NeighborWalker" />'s fixed visitation order, so the diagonal-free
    ///     enumeration is a strict prefix of the full one.
    /// </summary>
    public static NeighborWalker Neighbors8(RoomComponent room, int x, int y, bool includeDiagonals)
        => new(x, y, 0, 0, room.Width - 1, room.Height - 1, includeDiagonals);

    /// <summary>
    ///     Allocation-free enumeration of the 8-connected grid neighbors of <c>(x, y)</c> that fall
    ///     within the inclusive <paramref name="bounds" /> rectangle. Mirrors
    ///     <see cref="Neighbors8(RoomComponent, int, int)" /> but clamps to an arbitrary sub-region
    ///     (e.g. a room's <see cref="RoomComponent.BoundingRectangle" />) rather than the whole room.
    /// </summary>
    public static NeighborWalker Neighbors8(Rectangle bounds, int x, int y)
        => new(x, y, bounds.TopLeft.X, bounds.TopLeft.Y, bounds.BottomRight.X, bounds.BottomRight.Y,
            includeDiagonals: true);

    /// <summary>
    ///     Value-type sequence and enumerator backing the <see cref="Neighbors8(RoomComponent, int, int)" />
    ///     overloads — a single struct serves both roles so <c>foreach</c> stays allocation-free. Its
    ///     bounds are inclusive on both ends. Neighbors are visited in a fixed order — the four
    ///     cardinals (E, W, S, N) first, then the four diagonals — which the order-sensitive corridor
    ///     pathfinders rely on for deterministic tie-breaking; do not reorder.
    /// </summary>
    public struct NeighborWalker
    {
        private static readonly (sbyte X, sbyte Y)[] Offsets =
        [
            (1, 0), (-1, 0), (0, 1), (0, -1),
            (1, 1), (1, -1), (-1, 1), (-1, -1)
        ];

        private readonly int _x;
        private readonly int _y;
        private readonly int _minX;
        private readonly int _minY;
        private readonly int _maxX;
        private readonly int _maxY;
        private readonly int _directions;
        private int _dir;

        public NeighborWalker(int x, int y, int minX, int minY, int maxX, int maxY, bool includeDiagonals)
        {
            _x = x;
            _y = y;
            _minX = minX;
            _minY = minY;
            _maxX = maxX;
            _maxY = maxY;
            _directions = includeDiagonals ? 8 : 4;
            _dir = -1;
            Current = default;
        }

        public Point Current { get; private set; }

        public readonly NeighborWalker GetEnumerator() => this;

        public bool MoveNext()
        {
            while (++_dir < _directions)
            {
                var (dx, dy) = Offsets[_dir];
                var nx = _x + dx;
                var ny = _y + dy;
                if (nx < _minX || ny < _minY || nx > _maxX || ny > _maxY)
                {
                    continue;
                }

                Current = new Point((byte)nx, (byte)ny);
                return true;
            }

            return false;
        }
    }

    private static bool HasWalkableNeighbor(RoomComponent room, int x, int y)
    {
        var pointToIndex = room.PointToIndex!;
        foreach (var neighbor in Neighbors8(room, x, y))
        {
            if (room.Terrain[pointToIndex[neighbor.X, neighbor.Y]].CanMoveTo()
                && !room.Connections.ContainsKey(neighbor))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Walls off walkable tiles that sit on the room's outer ring (and are therefore exposed
    ///     to the level void beyond the room boundary), restoring the "rooms are fully enclosed"
    ///     invariant for the boundary that <see cref="SealUnfilledAroundWalkables" /> can't cover
    ///     (there's no in-bounds <see cref="MapFeature.Default" /> cell beyond the edge to wall).
    ///     <para>
    ///         Connection tiles are preserved: archways, this room's outbound connection cells
    ///         (<see cref="RoomComponent.Connections" />) and the arrival cells of inbound
    ///         connections (<see cref="RoomComponent.IncomingConnections" />) are the legitimate
    ///         boundary portals and are left walkable. Every other boundary floor cell is converted
    ///         to the branch's default wall (<paramref name="wall" />) only when doing so keeps the
    ///         remaining
    ///         walkable tiles mutually reachable, so connectivity is never broken — a load-bearing
    ///         boundary tile (the rare case where the only route hugs the edge) is left in place.
    ///         The scan repeats to a fix point because walling one cell can free up a neighbor.
    ///     </para>
    ///     <para>
    ///         Only meaningful for rooms with enclosing walls (the production dungeon/entrance
    ///         layouts via <see cref="LevelLayout.MaterializeGraph" />); it is intentionally not
    ///         run for the synthetic all-floor rooms built directly in tests.
    ///     </para>
    /// </summary>
    public static void SealWalkableBoundary(RoomComponent room, MapFeature wall)
    {
        var pointToIndex = room.PointToIndex!;
        var width = room.Width;
        var height = room.Height;

        var protectedCells = new HashSet<Point>(room.Connections.Keys);
        foreach (var incoming in room.IncomingConnections)
        {
            if (incoming.Connection!.TargetCell is { } targetCell)
            {
                protectedCells.Add(targetCell);
            }
        }

        var changed = true;
        while (changed)
        {
            changed = false;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    if (x != 0 && y != 0 && x != width - 1 && y != height - 1)
                    {
                        continue;
                    }

                    var feature = room.Terrain[pointToIndex[x, y]];
                    if (!feature.CanMoveTo() || feature == MapFeature.StoneArchway)
                    {
                        continue;
                    }

                    var cell = new Point((byte)x, (byte)y);
                    if (protectedCells.Contains(cell)
                        || !SealingPreservesConnectivity(room, cell))
                    {
                        continue;
                    }

                    TerrainSystem.SetTerrain(PickAdjacentWall(room, cell, wall), cell, room);
                    changed = true;
                }
            }
        }
    }

    /// <summary>
    ///     Returns whether converting <paramref name="cell" /> to a wall leaves every other
    ///     walkable tile of <paramref name="room" /> mutually 8-connected (i.e. the cell is not a
    ///     cut vertex of the walkable graph). Used by <see cref="SealWalkableBoundary" /> to avoid
    ///     severing the room.
    /// </summary>
    private static bool SealingPreservesConnectivity(RoomComponent room, Point cell)
    {
        var pointToIndex = room.PointToIndex!;
        var indexToPoint = room.IndexToPoint!;
        var tileCount = room.TileCount;
        var cellIdx = pointToIndex[cell.X, cell.Y];

        var totalWalkable = 0;
        short start = -1;
        for (short idx = 0; idx < tileCount; idx++)
        {
            if (!room.Terrain[idx].CanMoveTo())
            {
                continue;
            }

            totalWalkable++;
            if (idx != cellIdx)
            {
                start = idx;
            }
        }

        // Walling the only walkable tile (or none) can't disconnect anything.
        if (totalWalkable <= 1 || start < 0)
        {
            return true;
        }

        var visited = ArrayPool<bool>.Shared.Rent(tileCount);
        try
        {
            Array.Clear(visited, 0, tileCount);
            var queue = new Queue<short>();
            visited[cellIdx] = true; // exclude the candidate from the flood
            visited[start] = true;
            queue.Enqueue(start);
            var reached = 1;

            while (queue.Count > 0)
            {
                var current = indexToPoint[queue.Dequeue()];
                foreach (var neighbor in Neighbors8(room, current.X, current.Y))
                {
                    var nIdx = pointToIndex[neighbor.X, neighbor.Y];
                    if (visited[nIdx] || !room.Terrain[nIdx].CanMoveTo())
                    {
                        continue;
                    }

                    visited[nIdx] = true;
                    reached++;
                    queue.Enqueue(nIdx);
                }
            }

            // Every walkable tile except the candidate must have been reached.
            return reached == totalWalkable - 1;
        }
        finally
        {
            ArrayPool<bool>.Shared.Return(visited, clearArray: false);
        }
    }

    private static Chunk? PickNearestReachedChunk(
        RoomComponent room, List<Chunk> chunks, Chunk target, bool[] reached)
    {
        var pointToIndex = room.PointToIndex!;
        Chunk? nearest = null;
        var nearestDistance = int.MaxValue;
        for (var i = 0; i < chunks.Count; i++)
        {
            var candidate = chunks[i];
            if (candidate == target
                || candidate.Fragment.Name == NormalMapFragmentData.Deferred.Name
                || !IsChunkReached(candidate, reached, pointToIndex))
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
    ///     it). Synthetic <see cref="NormalMapFragmentData.Deferred" />
    ///     chunks are reported as reached regardless: their cells are pure floor
    ///     adjacent to the chunks whose preserved conditions produced them and become
    ///     part of the flood-fill set the next time it runs. For all other chunks
    ///     (including open-perimeter ones without `+` doorways) we walk the inside
    ///     points and return true on first hit.
    /// </summary>
    private static bool IsChunkReached(Chunk chunk, bool[] reached, short[,] pointToIndex)
    {
        if (chunk.Fragment.Name == NormalMapFragmentData.Deferred.Name)
        {
            return true;
        }

        foreach (var p in chunk.InsidePoints)
        {
            if (reached[pointToIndex[p.X, p.Y]])
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Flood-fills <paramref name="reached" /> from the union of <paramref name="seedPoints" />
    ///     across all 8-connected neighbors whose terrain satisfies
    ///     <see cref="MapFeatureExtensions.CanMoveTo" />, returning the number of cells marked.
    ///     <paramref name="reached" /> is a caller-provided mask indexed by
    ///     <see cref="RoomComponent.PointToIndex" /> (cleared by the caller before reuse).
    ///     Diagonal expansion matches in-game actor movement so connectivity here reflects whether
    ///     actors can actually traverse between chunks. A seed cell is recorded as reached
    ///     unconditionally so a chunk with a non-floor interior tile (e.g. a feature glyph cell)
    ///     still counts as reachable, but only <em>walkable</em> seeds are expanded from: expanding
    ///     out of a non-walkable seed (a wall or feature cell inside a chunk) would let the flood
    ///     tunnel through a one-cell-thick wall into a neighboring chunk and mask a genuine
    ///     disconnection.
    /// </summary>
    private static int FloodFillReachable(
        RoomComponent room, IEnumerable<Point> seedPoints, bool[] reached)
    {
        var pointToIndex = room.PointToIndex!;
        var indexToPoint = room.IndexToPoint!;
        var queue = new Queue<short>();
        var count = 0;
        foreach (var seed in seedPoints)
        {
            var idx = pointToIndex[seed.X, seed.Y];
            if (!reached[idx])
            {
                reached[idx] = true;
                count++;
                // A connection cell teleports the actor to another room, so for intra-room
                // connectivity it is a dead end: record it as reached (it's a valid tile to step
                // onto) but never expand through it.
                if (room.Terrain[idx].CanMoveTo() && !room.Connections.ContainsKey(seed))
                {
                    queue.Enqueue(idx);
                }
            }
        }

        var bounds = room.BoundingRectangle;
        while (queue.Count > 0)
        {
            var p = indexToPoint[queue.Dequeue()];
            foreach (var neighbor in Neighbors8(bounds, p.X, p.Y))
            {
                var index = pointToIndex[neighbor.X, neighbor.Y];
                if (reached[index] || !room.Terrain[index].CanMoveTo())
                {
                    continue;
                }

                reached[index] = true;
                count++;
                // A connection tile is reachable but not traversable.
                if (!room.Connections.ContainsKey(neighbor))
                {
                    queue.Enqueue(index);
                }
            }
        }

        return count;
    }

    /// <summary>
    ///     Carves a corridor between <paramref name="source" /> and <paramref name="target" />.
    ///     Probes both directions through <see cref="FindCorridorPath" /> and keeps the
    ///     path that traverses fewer non-corridor (wall / feature) tiles.
    /// </summary>
    private bool Connect(Chunk source, Chunk target,
        Dictionary<Point, EdgeConditionKind[]> preservedConditions,
        RoomParameters roomParameters,
        WeightedGraphSearch.PathWorkspace workspace,
        bool allowDiagonals = false, int width = 1, bool addColumns = false)
    {
        var from = source.GetGoodConnectionPoint(target);
        var to = target.GetGoodConnectionPoint(source);

        // Both calls return the path in source→target order. The "alternative" runs the
        // pathfinder with the endpoints swapped (which may discover a different route
        // through the room's open space because target-directed search and tie-breaking can select
        // a different equal-cost route)
        // and asks FindCorridorPath to flip its output so Connect can treat both
        // candidates uniformly.
        var path = FindCorridorPath(
            from, to, source.Room, allowDiagonals, reverse: false, workspace);
        var alternativePath = FindCorridorPath(
            to, from, source.Room, allowDiagonals, reverse: true, workspace);

        var corridor =
            path.FragmentFeaturesHit > alternativePath.FragmentFeaturesHit
                ? alternativePath.Path
                : path.Path;

        corridor = TrimCorridorEndpoints(corridor, source, target);

        WriteCorridor(corridor, source.Room, width, addColumns, preservedConditions, roomParameters);

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
    ///     Finds a minimum-cost corridor path. Walkable tiles cost one step, walls cost ten, and the
    ///     outer ring carries a large penalty. The admissible grid-distance heuristic excludes the
    ///     zero-cost final step onto the target.
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
        Point source, Point target, RoomComponent room, bool allowDiagonals, bool reverse,
        WeightedGraphSearch.PathWorkspace workspace)
    {
        if (source.Equals(target))
        {
            return new Corridor([source], 0);
        }

        var pointToIndex = room.PointToIndex!;
        var indexToPoint = room.IndexToPoint!;
        var sourceIdx = pointToIndex[source.X, source.Y];
        var targetIdx = pointToIndex[target.X, target.Y];
        var pathIndexes = workspace.Path;
        var graph = new CorridorGraph(room, target, allowDiagonals);
        var found = WeightedGraphSearch.TryGetShortestPathAStar(
            graph, sourceIdx, targetIdx, int.MaxValue, workspace, pathIndexes, out _);
        if (!found)
        {
            return new Corridor([source], 1);
        }

        var hits = 0;
        var path = new List<Point>(pathIndexes.Count);
        foreach (var index in pathIndexes)
        {
            var point = indexToPoint[index];
            if (GetCorridorStepCost(room.Terrain[index]) >= 10)
            {
                hits++;
            }

            path.Add(point);
        }

        if (!reverse)
        {
            path.Reverse();
        }

        return new Corridor(path, hits);
    }

    private readonly struct CorridorGraph(
        RoomComponent room, Point target, bool allowDiagonals) : IWeightedGraph
    {
        public int NodeCount => room.TileCount;
        public int NeighborCount => allowDiagonals ? 8 : 4;

        public bool TryGetNeighbor(
            int node, int neighborIndex, out int neighbor, out int distance)
        {
            var current = room.IndexToPoint![node];
            var (offsetX, offsetY) = neighborIndex switch
            {
                0 => (1, 0),
                1 => (-1, 0),
                2 => (0, 1),
                3 => (0, -1),
                4 => (1, 1),
                5 => (1, -1),
                6 => (-1, 1),
                7 => (-1, -1),
                _ => throw new ArgumentOutOfRangeException(nameof(neighborIndex))
            };
            var x = current.X + offsetX;
            var y = current.Y + offsetY;
            if (x < 0 || x >= room.Width || y < 0 || y >= room.Height)
            {
                neighbor = 0;
                distance = 0;
                return false;
            }

            neighbor = room.PointToIndex![x, y];
            var isTarget = x == target.X && y == target.Y;
            distance = isTarget
                ? 0
                : GetCorridorStepCost(room.Terrain[neighbor]);
            if (distance == int.MaxValue)
            {
                return false;
            }

            if (!isTarget
                && (x == 0 || y == 0 || x == room.Width - 1 || y == room.Height - 1))
            {
                distance += OuterRingCorridorPenalty;
            }

            return true;
        }

        public int GetEstimatedDistance(int node, int targetNode)
        {
            Debug.Assert(targetNode == room.PointToIndex![target.X, target.Y]);
            var point = room.IndexToPoint![node];
            var distance = allowDiagonals
                ? point.DistanceTo(target)
                : point.OrthogonalDistanceTo(target);
            return Math.Max(0, distance - 1);
        }
    }

    private const int OuterRingCorridorPenalty = 1001;

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
            MapFeature.StoneWall => 10,
            MapFeature.RockWall => 10,
            _ => int.MaxValue
        };

    private static void WriteCorridor(
        IReadOnlyList<Point> path, RoomComponent room, int width, bool addColumns,
        Dictionary<Point, EdgeConditionKind[]> preservedConditions,
        RoomParameters roomParameters)
    {
        var pathTerrain = roomParameters.DefaultPathTerrain;
        var wall = roomParameters.DefaultWall;

        // Carve every corridor tile first, in its own pass. Carving and sealing must not be
        // interleaved: consecutive path tiles are 8-neighbors, so sealing an earlier tile's
        // surroundings would wall the next path tile before it's carved — and the carve step would
        // then turn that wall into an archway, producing a corridor made of arches. Carving the
        // whole path up front leaves every path tile walkable, so the seal pass below only ever
        // walls genuine background void.
        foreach (var point in path)
        {
            var index = room.PointToIndex![point.X, point.Y];
            var currentFeature = room.Terrain[index];
            if (!currentFeature.CanMoveTo())
            {
                // Punch a doorway through a wall; carve open background to the branch's path terrain.
                room.Terrain[index] = MapFragment.IsWallFeature(currentFeature)
                    ? MapFeature.StoneArchway
                    : pathTerrain;
            }
        }

        // Seal each corridor tile's surroundings in a separate pass, now that the whole corridor is
        // walkable.
        foreach (var point in path)
        {
            SealCorridorTileSurroundings(point, room, preservedConditions, wall);
        }
    }

    /// <summary>
    ///     Encloses a freshly carved corridor tile by converting each of its still-unfilled
    ///     (<see cref="MapFeature.Default" />) 8-neighbors to the branch's default wall
    ///     (<paramref name="wall" /> — see <see cref="RoomParameters.DefaultWall" />). A neighbor
    ///     carrying deferred edge conditions is only walled when the wall satisfies them
    ///     (<see cref="FeatureSatisfiesConditions" />), so a cell that a chunk's edge
    ///     marked as having to stay empty / become a threshold is left for
    ///     <see cref="MapFragment.FixupDeferredConditions" /> to resolve.
    /// </summary>
    private static void SealCorridorTileSurroundings(
        Point tile, RoomComponent room, Dictionary<Point, EdgeConditionKind[]> preservedConditions,
        MapFeature wall)
    {
        var pointToIndex = room.PointToIndex!;
        foreach (var cell in Neighbors8(room, tile.X, tile.Y))
        {
            if (room.Terrain[pointToIndex[cell.X, cell.Y]] != MapFeature.Default)
            {
                continue;
            }

            // Match an existing adjacent wall so the seal extends the surrounding wall line
            // instead of introducing a foreign wall type.
            var sealWall = PickAdjacentWall(room, cell, wall);
            if (preservedConditions.TryGetValue(cell, out var conditions)
                && !FeatureSatisfiesConditions(conditions, sealWall))
            {
                // The matched adjacent wall doesn't satisfy the cell's deferred conditions. Fall
                // back to the branch's default wall; only when that fails too is the cell left for
                // the fixup pass (its conditions require it to stay unfilled, lie on the room edge,
                // or become a doorway).
                if (sealWall == wall || !FeatureSatisfiesConditions(conditions, wall))
                {
                    continue;
                }

                sealWall = wall;
            }

            TerrainSystem.SetTerrain(sealWall, cell, room);
        }
    }

    /// <summary>
    ///     Returns whether <paramref name="feature" /> satisfies every preserved edge condition in
    ///     <paramref name="conditions" /> (the same AND semantics the fragment placement and fixup
    ///     passes apply). A wall never satisfies <see cref="EdgeConditionKind.Unfilled" />,
    ///     <see cref="EdgeConditionKind.Edge" /> or <see cref="EdgeConditionKind.Doorway" />, so
    ///     <see cref="SealCorridorTileSurroundings" /> uses this to avoid sealing a deferred-condition
    ///     cell that must stay empty or become a threshold. Per-condition evaluation is delegated to
    ///     the shared <see cref="MapFragment.EvaluateCondition" /> primitive.
    /// </summary>
    protected static bool FeatureSatisfiesConditions(EdgeConditionKind[] conditions, MapFeature feature)
    {
        var matched = 0;
        foreach (var condition in conditions)
        {
            if (!MapFragment.EvaluateCondition(condition, feature, neighborInRoom: true, ref matched))
            {
                return false;
            }
        }

        return true;
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
