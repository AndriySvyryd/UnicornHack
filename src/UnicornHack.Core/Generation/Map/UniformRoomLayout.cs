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

    public override List<Chunk>? Fill(
        RoomComponent room,
        Branch branch,
        DefiningMapFragment fragment,
        Dictionary<Direction, int> doorwayDemand,
        RoomParameters roomParameters)
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

        var chunks = PlaceDefiningMapFragment(room, fragment, roomParameters);

        var remainingDemand = new Dictionary<Direction, int>(doorwayDemand);
        for (var i = 0; i < chunks.Count; i++)
        {
            DeductChunkDoorways(room, chunks[i], remainingDemand);
        }

        if (!PlaceSurroundingFragments(room, chunks, remainingDemand, roomParameters))
        {
            // Connectivity could not be ensured — bail so the level generator restarts
            // this room layout attempt.
            RecordRoomFailure(room, fragment, chunks);
            return null;
        }

        if (!SatisfiesDemand(room, chunks, doorwayDemand))
        {
            RecordRoomFailure(room, fragment, chunks);
            return null;
        }

        return chunks;
    }

    private static void RecordRoomFailure(RoomComponent room, DefiningMapFragment fragment, List<Chunk> chunks)
    {
        var stats = room.Level.GenerationStats;
        stats.RecordFragmentRoomFailure(fragment.SourceName);
        for (var i = 0; i < chunks.Count; i++)
        {
            var chunkFragment = chunks[i].Fragment;
            if (chunkFragment.SourceName != fragment.SourceName)
            {
                stats.RecordFragmentRoomFailure(chunkFragment.SourceName);
            }
        }
    }

    /// <summary>
    ///     Fills empty space around the defining chunk by iteratively allocating lots from
    ///     <see cref="RectanglePartition" /> and asking <see cref="MapFragment.TryPlace" /> to
    ///     place a weighted-pick fragment into each lot. Subclasses override this to inject
    ///     placement-strategy variations (e.g. <c>DenseRoomLayout</c>'s wall-merge shift).
    ///     Returns <c>false</c> when post-placement connectivity could not be ensured so the
    ///     caller can declare the layout attempt failed and restart.
    /// </summary>
    protected virtual bool PlaceSurroundingFragments(
        RoomComponent room,
        List<Chunk> chunks,
        Dictionary<Direction, int> remainingDemand,
        RoomParameters roomParameters)
    {
        var levelBoundingRectangle = room.BoundingRectangle;
        using var emptySpace = new RectanglePartition(levelBoundingRectangle);
        var placedChunkBounds = new RectangleIntervalTree(levelBoundingRectangle);
        var preservedConditions = new Dictionary<Point, List<EdgeConditionKind>>();
        var filledArea = 0;

        var definingChunk = chunks.FirstOrDefault();
        if (definingChunk != null)
        {
            emptySpace.Insert(definingChunk.BoundingRectangle);
            placedChunkBounds.Insert(definingChunk.BoundingRectangle);
            MapFragment.RegisterPreservedConditions(definingChunk, room, emptySpace, preservedConditions, roomParameters);
            filledArea += definingChunk.BoundingRectangle.Area;
        }

        var placingLevelConnection = NeedsLevelConnection(room);
        while (filledArea / (float)levelBoundingRectangle.Area < Coverage && chunks.Count < MaxChunkCount)
        {
            var nextLot = SelectNextLot(emptySpace, room.Level.GenerationRandom);
            if (nextLot == null)
            {
                // Room is too small for more lots — done.
                break;
            }

            room.Level.GenerationStats.LotPlacementAttempts++;

            var lotDemandedDoorways = ComputeLotDemandedDoorways(room, nextLot.Value, remainingDemand);

            // Build the per-lot placement context. The empty-space partition tracks unfilled
            // cells and is updated each iteration by Insert(...) below; targetEdge is
            // rederived per lot from the chunk-bounds interval tree. Subclasses can hook
            // CreatePlacementContext to adjust the lot (e.g. DenseRoomLayout's wall-merge
            // shift) before fragment selection.
            var ctx = CreatePlacementContext(
                nextLot.Value, emptySpace, placedChunkBounds, preservedConditions, chunks,
                lotDemandedDoorways, roomParameters);

            var danglingConnection = room.IncomingConnections
                .Select(c => c.Connection!).FirstOrDefault(c => c.TargetLevelX == null);
            IEnumerable<MapFragment> sortedFragments = placingLevelConnection
                ? room.Level.GenerationRandom.WeightedOrder(
                    room.Level.Game.Services.Data.ConnectingMapFragments.GetAsList(),
                    f => f.GetWeight(room, ctx, danglingConnection))
                : room.Level.GenerationRandom.WeightedOrder(
                    room.Level.Game.Services.Data.NormalMapFragments.GetAsList(),
                    f => f.GetWeight(room, ctx));

            var placedChunk = TryPlace(room, ctx, sortedFragments);

            // Fallback: when a level connection is needed but no ConnectingMapFragment
            // fit this lot, *and* the lot is flush with the level perimeter on a side
            // where the room itself is on the level graph edge, retry the placement
            // with NormalMapFragments that carry the specific `door-{side}` tag for
            // that perimeter side. The connection is then wired up to one of the
            // placed chunk's perimeter doorway points (instead of a random inside
            // point as a ConnectingMapFragment does).
            Direction? fallbackLevelEdge = null;
            if (placedChunk == null && placingLevelConnection)
            {
                var candidateEdges = ComputeLevelEdgeFlags(nextLot.Value, levelBoundingRectangle);
                foreach (var edge in EnumerateCardinalEdges(candidateEdges, room.Level.GenerationRandom))
                {
                    if (!IsRoomOnLevelGraphEdge(room, edge))
                    {
                        continue;
                    }

                    var capturedCtx = ctx;
                    var capturedEdge = edge;
                    var normalCandidates = room.Level.Game.Services.Data.NormalMapFragments.GetAsList()
                        .Where(f => HasDoorwayOnEdge(f, capturedEdge))
                        .ToList();
                    if (normalCandidates.Count == 0)
                    {
                        continue;
                    }

                    var fallbackOrder = room.Level.GenerationRandom.WeightedOrder(
                        normalCandidates, f => f.GetWeight(room, capturedCtx));
                    var attempt = TryPlace(room, ctx, fallbackOrder);
                    if (attempt != null)
                    {
                        placedChunk = attempt;
                        fallbackLevelEdge = edge;
                        break;
                    }
                }
            }

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
            emptySpace.Insert(placedChunk.BoundingRectangle);
            placedChunkBounds.Insert(placedChunk.BoundingRectangle);
            MapFragment.RegisterPreservedConditions(placedChunk, room, emptySpace, preservedConditions, roomParameters);
            filledArea += placedChunk.BoundingRectangle.Area;
            DeductChunkDoorways(room, placedChunk, remainingDemand);

            // Cap inter-level connections at one per room. ConnectingMapFragments wire up
            // the connection themselves inside BuildChunk; for NormalMapFragments placed
            // by the fallback path above we wire it up here at one of the chunk's
            // perimeter doorway points facing the level boundary.
            if (placingLevelConnection)
            {
                if (placedChunk.Fragment is ConnectingMapFragment)
                {
                    placingLevelConnection = false;
                }
                else if (placedChunk.Fragment is NormalMapFragment
                    && fallbackLevelEdge is { } chosenEdge
                    && TryPickLevelEdgeDoorway(
                        placedChunk, levelBoundingRectangle, chosenEdge,
                        room.Level.GenerationRandom, out var connectionCell))
                {
                    LevelConnection.CreateLevelConnection(room, connectionCell, connectionDefinition: null);
                    placingLevelConnection = false;
                }
            }
        }

        // Once no more fragments fit, materialize any still-deferred edge conditions:
        // empty cells next to placed chunks that carry preserved conditions are filled
        // with a satisfying MapFeature. Cells whose conditions can only be honoured by
        // leaving them empty (Unfilled / Edge) are skipped. The placement-time
        // compatibility check in MapFragment.EvaluateEdgeAtAnchor rejects placements
        // that would register contradictory deferred conditions, so the conflict
        // branch in TryCollectSatisfyingFeatures can no longer be reached here.
        chunks.AddRange(MapFragment.FixupDeferredConditions(
            room, preservedConditions, room.Level.GenerationRandom, roomParameters));

        if (chunks.Count > 1)
        {
            if (!TryRepairConnectivity(room, chunks))
            {
                room.Level.GenerationStats.RoomLayoutConnectivityRepairFailed++;
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Returns the cardinal sides on which <paramref name="lot" /> is flush with
    ///     <paramref name="levelBounds" />. Used to decide whether a normal fragment can
    ///     host an inter-level connection on this lot — the connection cell must lie on
    ///     the level perimeter so the receiving side can locate it geometrically.
    /// </summary>
    private static DirectionFlags ComputeLevelEdgeFlags(Rectangle lot, Rectangle levelBounds)
    {
        var flags = DirectionFlags.None;
        if (lot.TopLeft.X == levelBounds.TopLeft.X)
        {
            flags |= DirectionFlags.West;
        }

        if (lot.BottomRight.X == levelBounds.BottomRight.X)
        {
            flags |= DirectionFlags.East;
        }

        if (lot.TopLeft.Y == levelBounds.TopLeft.Y)
        {
            flags |= DirectionFlags.North;
        }

        if (lot.BottomRight.Y == levelBounds.BottomRight.Y)
        {
            flags |= DirectionFlags.South;
        }

        return flags;
    }

    /// <summary>
    ///     Returns true when <paramref name="fragment" /> carries the
    ///     <c>door-{n|s|e|w}</c> tag matching <paramref name="edge" />. Doorway tags
    ///     are emitted per-variant by <see cref="MapFragment.ApplyTagDerivations" />,
    ///     so the tag set already reflects the variant's rotated geometry.
    /// </summary>
    private static bool HasDoorwayOnEdge(NormalMapFragment fragment, Direction edge)
    {
        var tags = fragment.Tags;
        if (tags == null)
        {
            return false;
        }

        return edge switch
        {
            Direction.North => tags.Contains("door-n"),
            Direction.South => tags.Contains("door-s"),
            Direction.East => tags.Contains("door-e"),
            Direction.West => tags.Contains("door-w"),
            _ => false
        };
    }

    /// <summary>
    ///     Whether <paramref name="room" />'s perimeter on <paramref name="edge" />
    ///     coincides with the level's outer perimeter, i.e. <paramref name="room" /> is
    ///     a level-graph-edge node on that side. Today every <see cref="RoomComponent" />
    ///     spans its entire level so this is trivially true; once levels split into
    ///     multiple per-room components this gates which rooms may host an inter-level
    ///     connection on which side.
    /// </summary>
    private static bool IsRoomOnLevelGraphEdge(RoomComponent room, Direction edge)
    {
        _ = room;
        _ = edge;
        return true;
    }

    /// <summary>
    ///     Returns the cardinal directions present in <paramref name="edges" /> in a
    ///     random order so that corner lots don't systematically prefer one side over
    ///     another when looking for a matching door-tagged fragment.
    /// </summary>
    private static Direction[] EnumerateCardinalEdges(DirectionFlags edges, SimpleRandom random)
    {
        if (edges == DirectionFlags.None)
        {
            return [];
        }

        var count = 0;
        var buffer = new Direction[4];
        if ((edges & DirectionFlags.North) != 0) { buffer[count++] = Direction.North; }
        if ((edges & DirectionFlags.South) != 0) { buffer[count++] = Direction.South; }
        if ((edges & DirectionFlags.East) != 0) { buffer[count++] = Direction.East; }
        if ((edges & DirectionFlags.West) != 0) { buffer[count++] = Direction.West; }

        // Fisher–Yates shuffle in place — count is at most 4.
        for (var i = count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
        }

        if (count == 4)
        {
            return buffer;
        }

        var result = new Direction[count];
        Array.Copy(buffer, result, count);
        return result;
    }

    /// <summary>
    ///     Picks a doorway point on the placed chunk that lies on the level-edge side
    ///     <paramref name="edge" />. The connection cell must coincide with the level
    ///     perimeter so the matching receiving connection on the adjacent level can be
    ///     wired up at a deterministic location.
    /// </summary>
    private static bool TryPickLevelEdgeDoorway(
        Chunk chunk, Rectangle levelBounds, Direction edge,
        SimpleRandom random, out Point connectionCell)
    {
        List<Point>? matches = null;
        foreach (var p in chunk.DoorwayPoints)
        {
            if (IsOnLevelEdge(p, levelBounds, edge))
            {
                matches ??= [];
                matches.Add(p);
            }
        }

        if (matches == null)
        {
            connectionCell = default;
            return false;
        }

        connectionCell = matches.Count == 1 ? matches[0] : random.Pick(matches);
        return true;
    }

    private static bool IsOnLevelEdge(Point p, Rectangle levelBounds, Direction edge) => edge switch
    {
        Direction.West => p.X == levelBounds.TopLeft.X,
        Direction.East => p.X == levelBounds.BottomRight.X,
        Direction.North => p.Y == levelBounds.TopLeft.Y,
        Direction.South => p.Y == levelBounds.BottomRight.Y,
        _ => false
    };

    /// <summary>
    ///     Builds the <see cref="LotPlacementContext" /> for the next chunk. Subclasses
    ///     override this to adjust the lot before fragment selection runs — e.g.
    ///     <c>DenseRoomLayout</c> shifts the lot by one tile toward an adjacent placed
    ///     chunk so the two chunks' perimeter walls collapse into a single shared column.
    ///     The default implementation just picks the target edge and forwards the lot
    ///     unchanged.
    /// </summary>
    protected virtual LotPlacementContext CreatePlacementContext(
        Rectangle lot,
        RectanglePartition emptySpace,
        RectangleIntervalTree placedChunkBounds,
        Dictionary<Point, List<EdgeConditionKind>> preservedConditions,
        IReadOnlyList<Chunk> placedChunks,
        DirectionFlags lotDemandedDoorways,
        RoomParameters roomParameters)
    {
        var targetEdge = PickTargetEdge(lot, placedChunkBounds);
        // Subclass hook: e.g. DenseRoomLayout shifts the lot one tile toward an adjacent
        // placed chunk to collapse the double-wall column into a single shared wall. The
        // default implementation returns the lot unchanged.
        var originalLot = lot;
        lot = MaybeShiftLot(lot, targetEdge, placedChunks);
        var lotShifted = !lot.Equals(originalLot);

        // Force one baseline doorway per chunk for intra-room connectivity by OR-ing the
        // target edge into the demanded directions. PickTargetEdge always returns a
        // cardinal, so this is a safe widening.
        var demanded = lotDemandedDoorways | (targetEdge?.AsCardinalFlag() ?? DirectionFlags.None);
        return new LotPlacementContext(
            lot, roomParameters, targetEdge, emptySpace, MinLotSize,
            placedChunkBounds, preservedConditions, demanded, lotShifted);
    }

    /// <summary>
    ///     Subclass hook called from <see cref="CreatePlacementContext" /> after the
    ///     target edge is picked and before the placement context is built. Returns the
    ///     lot to use for fragment selection — the default implementation forwards
    ///     <paramref name="lot" /> unchanged. <c>DenseRoomLayout</c> overrides this to
    ///     shift the lot by one tile toward an adjacent placed chunk so the two chunks'
    ///     perimeter walls collapse into a single shared column.
    /// </summary>
    protected virtual Rectangle MaybeShiftLot(
        Rectangle lot, Direction? targetEdge, IReadOnlyList<Chunk> placedChunks)
        => lot;

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

    protected Rectangle? FindLargestFitting(RectanglePartition partition)
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
