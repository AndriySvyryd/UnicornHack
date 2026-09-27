using System.Buffers;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils;

namespace UnicornHack.Generation.Map;

/// <summary>
///     Defines how rooms in a <see cref="GameLevel" /> are laid out and connected.
///     Each <see cref="LevelLayout" /> owns level-scope concerns:
///     <list type="bullet">
///         <item>How many rooms the level has and where they sit in the room graph</item>
///         <item>Which rooms are connected to which (and through which side)</item>
///         <item>Which rooms are entrances and which are exits</item>
///     </list>
///     Per-room work (terrain, chunks, doors at the requested perimeter sides) is delegated to
///     the <see cref="Branch.RoomLayout" /> property on the owning branch.
/// </summary>
/// <remarks>
///     <para>
///         Subclasses define the *topology* by overriding <see cref="GetNeighborOffsets" />
///         (4-/6-/8-neighbor and other variants) and <see cref="PlaceRooms" /> (initial cell
///         placement, including entrance/exit). The shared <see cref="BuildGraph" /> algorithm
///         starts with a fully-bidirectional graph (every topology adjacency = two directed
///         edges) and prunes edges in random order, validating after each removal that
///         every node remains reachable from the entrance and can still reach the exit.
///         When only one half of a bidirectional pair is removed the surviving edge becomes a
///         one-way passage in gameplay.
///     </para>
/// </remarks>
public abstract class LevelLayout
{
    /// <summary>Width of the cell grid used by the placement algorithm.</summary>
    public byte GridWidth { get; init; } = 7;

    /// <summary>Height of the cell grid used by the placement algorithm.</summary>
    public byte GridHeight { get; init; } = 5;

    /// <summary>
    ///     Edge-density target after pruning, in <c>[0, 1]</c>. <c>0</c> keeps only the
    ///     spanning tree; <c>1</c> keeps every topology-adjacency edge.
    /// </summary>
    public float Interconnectedness { get; init; } = 0.75f;

    /// <summary>Per-room probability of receiving a reward feature.</summary>
    public float RewardRoomProbability { get; init; } = 0.3f;

    /// <summary>Per-room probability of receiving the cursed feature.</summary>
    public float CurseRoomProbability { get; init; } = 0.1f;

    /// <summary>
    ///     Expected number of concealed rooms per level. The value is spread across the
    ///     hideable off-spine rooms (see <see cref="IsHideable" />), so each eligible room is
    ///     hidden with probability <c>HiddenConnectionProbability / hideableRoomCount</c> and a
    ///     bigger level doesn't get proportionally more secrets. A hidden room is concealed on
    ///     the map until the player discovers it, and is always a reward room drawn from the
    ///     premium-biased distribution (which unlocks the <see cref="RoomFeatures.Shortcut" />
    ///     reward). <c>0</c> disables concealment entirely, keeping every connection visible.
    /// </summary>
    public float HiddenConnectionProbability { get; init; } = 0.3f;

    /// <summary>
    ///     How winding the entrance-to-exit path should be, in <c>[0, 1]</c>.
    ///     <c>0</c> keeps the shortest possible entrance-to-exit path;
    ///     <c>1</c> stretches it toward the longest simple path on the
    ///     undirected adjacency graph. Intermediate values linearly interpolate.
    ///     Values over 0.5 not recommended as finding the longest possible path can be expensive.
    /// </summary>
    public float Sinuosity { get; init; }

    /// <summary>
    ///     Lower bound on rooms in a successfully generated level. <see cref="Fill" /> returns
    ///     <c>null</c> if the graph has fewer rooms than this; callers retry with a new seed.
    /// </summary>
    public byte MinRooms { get; init; } = 28;

    /// <summary>
    /// <summary>
    ///     Upper bound on rooms in a generated level. <see cref="TrimRooms" /> samples a
    ///     target count from a normal distribution over <c>[MinRooms, MaxRooms]</c>
    ///     after <see cref="PlaceRooms" /> runs.
    /// </summary>
    public byte MaxRooms { get; init; } = 32;

    /// <summary>
    ///     Builds <paramref name="level" />: creates the necessary <see cref="RoomComponent" />
    ///     entities, decides their connections, and invokes <see cref="RoomLayout" /> for each one
    ///     to fill its terrain. Each room independently picks its own
    ///     <see cref="DefiningMapFragment" /> via <see cref="LevelGenerator.PickFragmentForRoom" />
    ///     based on its <see cref="RoomFeatures" />.
    /// </summary>
    /// <returns>
    ///     The chunks that were placed, or <c>null</c> if generation failed for a recoverable
    ///     reason (e.g. <see cref="BuildGraph" /> couldn't satisfy the connectivity invariant
    ///     for the seed). Truly unexpected errors throw.
    /// </returns>
    public virtual List<Chunk>? Fill(GameLevel level, Branch branch)
    {
        var parameters = GetParameters(level.Difficulty);
        var gameStats = level.Game.GenerationStats;
        var graph = BuildGraph(parameters, level.GenerationRandom, level.GenerationStats, gameStats);
        return graph == null || graph.Rooms.Count < parameters.MinRooms
            ? null
            : MaterializeGraph(level, graph, branch);
    }

    /// <summary>
    ///     Returns the per-level <see cref="LayoutParameters" /> snapshot used by
    ///     <see cref="BuildGraph" /> for a given <paramref name="difficulty" />. Default
    ///     implementation returns the layout's instance properties verbatim; subclasses
    ///     override this to return difficulty-dependent (or otherwise dynamic) values
    ///     without mutating instance state.
    /// </summary>
    public virtual LayoutParameters GetParameters(int difficulty)
        => new(GridWidth, GridHeight, Interconnectedness, ComputeDensity(),
            Sinuosity, RewardRoomProbability, CurseRoomProbability, HiddenConnectionProbability,
            MinRooms, MaxRooms, difficulty);

    /// <summary>
    ///     Computes the placement density from <see cref="GridWidth" />,
    ///     <see cref="GridHeight" />, and <see cref="MaxRooms" />. For cell-fill layouts
    ///     (Grid, Hex) this is the probability each optional cell is occupied; for
    ///     column-walk layouts (Tree) it represents the fork probability. Subclasses
    ///     override to match their <see cref="PlaceRooms" /> semantics.
    /// </summary>
    public virtual float ComputeDensity()
        => Math.Min(1f, (float)(MaxRooms - 2) / Math.Max(1, GridWidth * GridHeight - 2));

    /// <summary>
    ///     Returns the directed cell offsets from <paramref name="cell" /> that count as
    ///     candidate-graph neighbors for this layout's topology. Examples:
    ///     <list type="bullet">
    ///         <item>4-neighbor grid: <c>(±1, 0), (0, ±1)</c></item>
    ///         <item>8-neighbor grid: <c>(±1, 0), (0, ±1), (±1, ±1)</c></item>
    ///         <item>Offset hex: column-parity-dependent (no vertical-only neighbors)</item>
    ///     </list>
    ///     Default implementation throws — subclasses using <see cref="BuildGraph" /> must override.
    /// </summary>
    public virtual IEnumerable<(int dx, int dy)> GetNeighborOffsets(byte cellX, byte cellY)
        => throw new NotImplementedException(
            $"{GetType().Name} does not implement {nameof(GetNeighborOffsets)}; override it or do not call {nameof(BuildGraph)}.");

    /// <summary>
    ///     Counts how many neighbors emitted by <see cref="GetNeighborOffsets" /> fall
    ///     inside the <paramref name="gridWidth" /> × <paramref name="gridHeight" />
    ///     bounding box. Subclasses use this in <see cref="PlaceRooms" /> to skip cells
    ///     that can never have enough connections to satisfy the
    ///     "every non-endpoint room has ≥ 2 undirected neighbors" invariant
    ///     <see cref="BuildGraph" /> requires; placing them just wastes the
    ///     <see cref="TrimRooms" /> budget and lowers the per-attempt success rate.
    /// </summary>
    public int CountInGridNeighbors(byte cellX, byte cellY, byte gridWidth, byte gridHeight)
    {
        var count = 0;
        foreach (var (dx, dy) in GetNeighborOffsets(cellX, cellY))
        {
            var nx = cellX + dx;
            var ny = cellY + dy;
            if (nx >= 0 && ny >= 0 && nx < gridWidth && ny < gridHeight)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    ///     Builds the room graph via <b>ear decomposition</b>. The result is a single-source
    ///     single-sink DAG (entrance → exit) where every non-endpoint node has at least
    ///     two undirected neighbors and lies on a simple entrance-to-exit path — i.e.
    ///     the player can always reach the exit from any room without revisiting an
    ///     already-entered room.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Algorithm:
    ///         <list type="number">
    ///             <item><see cref="PlaceRooms" /> emits nodes (entrance + exit + the rest);
    ///                 <see cref="TrimRooms" /> samples a target count from a normal
    ///                 distribution over <c>[MinRooms, MaxRooms]</c> and removes
    ///                 non-articulation rooms down to that target (preserving
    ///                 entrance↔exit edge-connectivity), then returns the surviving
    ///                 undirected adjacency.</item>
    ///             <item>Pick a <b>spine</b>: a simple entrance→exit path on the adjacency.
    ///                 With <see cref="LayoutParameters.Sinuosity" /> = 0 the spine is a
    ///                 BFS shortest path; with sinuosity 1 it's the longest path found via
    ///                 Warnsdorff walks; intermediate values pick the candidate whose length
    ///                 is closest to the interpolated target. Spine nodes get integer
    ///                 topological positions in <c>order</c>.</item>
    ///             <item>Repeatedly attach an <b>ear</b>: a simple path of uncovered nodes
    ///                 connecting two distinct already-covered anchors. Internal nodes get
    ///                 fractional topological orders strictly between the two anchor orders.
    ///                 By construction every internal node admits a simple entrance-to-exit
    ///                 path: walk forward through the lower-order anchor, traverse the ear
    ///                 forward through the node to the higher-order anchor, walk forward to
    ///                 exit. Forward walks in a topological order never revisit nodes.</item>
    ///             <item>Defence: every non-endpoint node must have at least two undirected
    ///                 neighbors; otherwise the graph is rejected (the caller retries
    ///                 with a fresh seed).</item>
    ///             <item>Add forward-oriented <b>cross-edges</b> from the adjacency up to
    ///                 the <see cref="LayoutParameters.Interconnectedness" /> target so the
    ///                 graph isn't strictly tree-shaped.</item>
    ///             <item>Each eligible off-spine room is concealed with probability
    ///                 <see cref="LayoutParameters.HiddenConnectionProbability" /> spread across
    ///                 the hideable rooms (so it's the expected hidden-room count per level), and
    ///                 every connection incident to a hidden room is marked hidden. Hiding never
    ///                 strands a visible predecessor (each keeps a visible outgoing edge) and the
    ///                 entrance→exit spine stays fully visible.</item>
    ///             <item>Reward / Curse features applied to non-entrance/non-exit rooms.</item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         Returns <c>null</c> when the adjacency for this seed can't be ear-decomposed
    ///         (an uncovered node with no second covered anchor reachable through other
    ///         uncovered nodes — typically a degree-1 leaf the trim pass couldn't remove).
    ///         The caller retries with a fresh seed.
    ///     </para>
    ///     <para>
    ///         <paramref name="stats" />, when non-null, records the failure category for
    ///         each early-return path so diagnostic tests can attribute null returns to
    ///         the specific stage that rejected the seed. <paramref name="gameStats" />
    ///         supplies the game-wide per-room-feature counts used by
    ///         <see cref="GetRewardFeatureWeight" /> to throttle features already placed
    ///         on previous levels; <c>null</c> falls back to a zero baseline.
    ///     </para>
    /// </remarks>
    public RoomGraph? BuildGraph(
        LayoutParameters parameters,
        SimpleRandom random,
        LevelGenerationStats stats,
        GenerationStats gameStats)
    {
        var nodes = PlaceRooms(parameters, random);
        if (nodes is null || nodes.Count < 2)
        {
            if (stats is not null) { stats.RoomGraphPlacementTooSmall++; }
            return null;
        }

        var entrance = nodes.Find(n => (n.Features & RoomFeatures.Entrance) != 0);
        var exit = nodes.Find(n => (n.Features & RoomFeatures.Exit) != 0);
        if (entrance is null || exit is null)
        {
            if (stats is not null) { stats.RoomGraphEntranceOrExitMissing++; }
            return null;
        }

        // Snapshot the full pre-trim placement so we can restore trimmed rooms if a
        // later stage (ear decomposition) gets stuck on a topologically isolated region.
        var allNodes = new List<RoomGraphNode>(nodes);
        var fullAdjacency = BuildTopologyAdjacency(allNodes);

        var adjacency = TrimRooms(nodes, parameters.MinRooms, parameters.MaxRooms, random);

        var removedPool = new HashSet<RoomGraphNode>(allNodes.Count - nodes.Count);
        foreach (var n in allNodes)
        {
            if (!adjacency.ContainsKey(n))
            {
                removedPool.Add(n);
            }
        }

        // Iteratively drop non-endpoint leaves (degree < 2 in undirected adjacency). A
        // leaf can't sit on a simple entrance→exit path through itself: it has at most
        // one neighbor, so any path through it must back-track. Removing leaves can
        // cascade (their sole neighbor may become a leaf in turn). If entrance or exit
        // end up isolated the seed is unusable and we return null so the caller retries.
        TrimLeaves(nodes, entrance, exit, adjacency);
        if (adjacency[entrance].Count == 0 || adjacency[exit].Count == 0 || nodes.Count < 2)
        {
            if (stats is not null) { stats.RoomGraphTrimIsolatedEndpoint++; }
            return null;
        }

        // Build the room connections (spine, ears, cross-edges) in an inner retry loop. Connections
        // that visually overlap (two grid diagonals crossing like an X on the rendered map) are
        // avoided three ways: each ear is re-rolled to dodge a crossing, crossing cross-edges are
        // dropped, and any crossing that still slips through is untwisted afterwards (re-pair / bend).
        // What survives all of that is almost always RNG-avoidable rather than structural — a
        // different spine / ear arrangement simply doesn't produce the twist — so the loop samples
        // many fresh arrangements (the phase is cheap relative to materializing rooms) and keeps the
        // one with the fewest residual crossings, stopping early the moment a crossing-free layout
        // appears. The early exit makes the high cap nearly free: only the rare seed that can't reach
        // zero crossings actually runs all the attempts. Empirically a cap of 40 drives the
        // production TreeLevelLayout from ~0.24 to ~0.01 residual crossings/level (Spiral to 0).
        const int maxConnectionAttempts = 40;
        HashSet<(RoomGraphNode Src, RoomGraphNode Tgt)>? bestEdges = null;
        List<RoomGraphNode>? bestSpine = null;
        List<RoomGraphNode>? bestNodes = null;
        var bestCrossings = int.MaxValue;
        for (var connectionAttempt = 0; connectionAttempt < maxConnectionAttempts; connectionAttempt++)
        {
            // The connection phase mutates its node / adjacency / removed-pool inputs (ear
            // restoration can re-add a trimmed room), so each attempt works on fresh per-attempt
            // copies and leaves the pristine post-trim state intact for the next attempt.
            var attemptNodes = new List<RoomGraphNode>(nodes);
            var attemptAdjacency = CloneAdjacency(adjacency);
            var attemptRemovedPool = new HashSet<RoomGraphNode>(removedPool);

            if (!TryConnectRooms(
                    attemptNodes, entrance, exit, attemptAdjacency, attemptRemovedPool, fullAdjacency,
                    parameters, random, stats, out var directedEdges, out var spine))
            {
                stats.RoomGraphConnectionRetries++;
                continue;
            }

            var crossings = CountConnectionCrossings(directedEdges);
            if (crossings < bestCrossings)
            {
                bestCrossings = crossings;
                bestEdges = directedEdges;
                bestSpine = spine;
                bestNodes = attemptNodes;
            }

            if (crossings == 0)
            {
                break; // can't do better than a crossing-free layout
            }

            stats.RoomGraphConnectionRetries++;
        }

        return bestEdges is null
            ? null
            : BuildFinalGraph(bestNodes!, bestEdges, bestSpine!, parameters, random, stats, gameStats);
    }

    /// <summary>
    ///     Runs one attempt of the connection phase on the post-trim node set: builds the
    ///     entrance→exit spine, covers the remaining rooms with an ear decomposition, then adds
    ///     <see cref="LayoutParameters.Interconnectedness" /> cross-edges — all while avoiding links
    ///     that would visually overlap an already-placed one (the two diagonals of a grid
    ///     unit-square form an X on the rendered map). Each ear is re-rolled a few times to dodge a
    ///     crossing, and an optional cross-edge that would cross is dropped; a crossing that no
    ///     re-roll avoids (an unavoidable one for that uncovered pocket) is accepted so generation
    ///     never fails purely to dodge a crossing. Returns <c>true</c> with the directed edge set
    ///     and spine on success; mutates <paramref name="nodes" />, <paramref name="adjacency" />
    ///     and <paramref name="removedPool" /> (ear restoration), so callers pass per-attempt copies.
    /// </summary>
    private static bool TryConnectRooms(
        List<RoomGraphNode> nodes,
        RoomGraphNode entrance,
        RoomGraphNode exit,
        Dictionary<RoomGraphNode, HashSet<RoomGraphNode>> adjacency,
        HashSet<RoomGraphNode> removedPool,
        Dictionary<RoomGraphNode, HashSet<RoomGraphNode>> fullAdjacency,
        LayoutParameters parameters,
        SimpleRandom random,
        LevelGenerationStats? stats,
        out HashSet<(RoomGraphNode Src, RoomGraphNode Tgt)> directedEdges,
        out List<RoomGraphNode> spine)
    {
        directedEdges = null!;
        spine = null!;

        var foundSpine = FindSpine(entrance, exit, adjacency, parameters.Sinuosity, random);
        if (foundSpine is null || foundSpine.Count < 2)
        {
            if (stats is not null) { stats.RoomGraphSpineNotFound++; }
            return false;
        }

        // Topological order. Spine nodes get integer positions; ear internals get
        // fractional values strictly between their two anchor orders. Two ears that share
        // identical anchor orders fall back to GridX/GridY (via CompareNodes) so every
        // undirected edge has a deterministic orientation.
        var order = new Dictionary<RoomGraphNode, double>(nodes.Count);
        for (var i = 0; i < foundSpine.Count; i++)
        {
            order[foundSpine[i]] = i;
        }

        // Tracks, per grid unit-square, which of its two diagonals an edge already occupies. A new
        // diagonal whose square already holds the opposite diagonal would cross it (an X), so it's
        // dropped (cross-edges) or re-rolled (ears). See <see cref="GetDiagonalKey" />.
        var occupiedDiagonals = new HashSet<(int MinX, int MinY, bool MainDiagonal)>();

        // The spine is a single (mostly cardinal) shortest path; record its diagonals so later ears
        // and cross-edges steer around them. A spine that self-crosses is rare and unchangeable
        // here (the path is fixed), so it's accepted rather than retried.
        var directed = new HashSet<(RoomGraphNode Src, RoomGraphNode Tgt)>(nodes.Count * 2);
        for (var i = 0; i < foundSpine.Count - 1; i++)
        {
            TryRegisterEdge(occupiedDiagonals, foundSpine[i], foundSpine[i + 1], failOnCrossing: false);
            directed.Add((foundSpine[i], foundSpine[i + 1]));
        }

        var uncovered = new HashSet<RoomGraphNode>(nodes.Count);
        foreach (var node in nodes)
        {
            if (!order.ContainsKey(node))
            {
                uncovered.Add(node);
            }
        }

        while (uncovered.Count > 0)
        {
            var ear = FindEar(uncovered, order, adjacency, random);
            if (ear is null)
            {
                // No ear available — an uncovered region is cut off from the covered
                // graph (or can't reach a second covered anchor through other uncovered
                // nodes). Try restoring a previously-trimmed room that bridges the gap;
                // if that fails (no useful candidate left, or MaxRooms hit), give up on
                // this seed.
                if (!TryRestoreRoomForEar(
                    removedPool, fullAdjacency, adjacency, uncovered, order,
                    nodes, parameters.MaxRooms))
                {
                    if (stats is not null) { stats.RoomGraphEarNotFound++; }
                    return false;
                }
                continue;
            }

            // Greedily prefer a non-crossing ear: re-roll FindEar (it samples a fresh ear from the
            // RNG) a few times and keep the first one whose links cross nothing already placed. When
            // every re-roll still crosses, the crossing is unavoidable for this pocket, so accept
            // the last candidate rather than failing generation.
            var earCrosses = EarIntroducesCrossing(occupiedDiagonals, ear);
            for (var reroll = 0; earCrosses && reroll < MaxEarCrossingRerolls; reroll++)
            {
                var alternative = FindEar(uncovered, order, adjacency, random);
                if (alternative is null)
                {
                    break;
                }
                ear = alternative;
                earCrosses = EarIntroducesCrossing(occupiedDiagonals, ear);
            }

            if (earCrosses && stats is not null) { stats.RoomGraphConnectionCrossing++; }

            for (var i = 0; i < ear.Count - 1; i++)
            {
                TryRegisterEdge(occupiedDiagonals, ear[i], ear[i + 1], failOnCrossing: false);
            }

            AttachEar(ear, order, directed, uncovered);
        }

        if (!HasMinDegree(nodes, entrance, exit, adjacency))
        {
            if (stats is not null) { stats.RoomGraphMinDegreeViolated++; }
            return false;
        }

        AddCrossEdges(adjacency, directed, order, parameters.Interconnectedness, random, occupiedDiagonals);

        // Some square-diagonal topologies leave crossings that no spine / ear / cross-edge choice
        // could avoid. Untwist them: where a unit-square ended up holding both diagonals (a rendered
        // X), re-pair its four corner rooms along the square's cardinal sides instead — provided the
        // re-pairing keeps every invariant. This clears structurally-unavoidable crossings the
        // earlier avoidance heuristics had to accept.
        UntwistCrossings(directed, order, nodes, entrance, exit, occupiedDiagonals, stats);

        // Defence-in-depth: confirm the DAG is well-formed (entrance reaches every node
        // via forward edges and every node reaches exit via forward edges). The ear
        // decomposition guarantees this by construction; failing here means a corner-case
        // bug — return false so the caller retries rather than handing back a broken graph.
        if (!ValidateDagConnectivity(nodes, entrance, exit, directed))
        {
            if (stats is not null) { stats.RoomGraphDagConnectivityBroken++; }
            return false;
        }

        directedEdges = directed;
        spine = foundSpine;
        return true;
    }

    /// <summary>
    ///     Deep-copies <paramref name="adjacency" /> (new outer dictionary and fresh neighbor
    ///     sets) so a connection attempt can mutate its copy without disturbing the pristine
    ///     post-trim adjacency reused by the next attempt.
    /// </summary>
    private static Dictionary<RoomGraphNode, HashSet<RoomGraphNode>> CloneAdjacency(
        Dictionary<RoomGraphNode, HashSet<RoomGraphNode>> adjacency)
    {
        var clone = new Dictionary<RoomGraphNode, HashSet<RoomGraphNode>>(adjacency.Count);
        foreach (var (node, neighbors) in adjacency)
        {
            clone[node] = [..neighbors];
        }

        return clone;
    }

    /// <summary>
    ///     Maps a unit-length grid diagonal between <paramref name="a" /> and <paramref name="b" />
    ///     to its unit-square (identified by the lower-left corner) and which of the square's two
    ///     diagonals it occupies — main (lower-left ↔ upper-right) or anti (lower-right ↔
    ///     upper-left). Returns <c>null</c> for a cardinal (axis-aligned) link, which can never
    ///     cross another unit-length link. Hex layouts never make both diagonals of a square
    ///     adjacent, so this stays inert for them; only true square grids (cardinal + diagonal
    ///     topologies) can place crossing diagonals.
    /// </summary>
    private static (int MinX, int MinY, bool MainDiagonal)? GetDiagonalKey(RoomGraphNode a, RoomGraphNode b)
    {
        if (Math.Abs(a.GridX - b.GridX) != 1 || Math.Abs(a.GridY - b.GridY) != 1)
        {
            return null;
        }

        var minX = Math.Min(a.GridX, b.GridX);
        var minY = Math.Min(a.GridY, b.GridY);
        // An endpoint sits on the main diagonal iff "is its X the square's min" agrees with "is its
        // Y the square's min" (lower-left: both true; upper-right: both false). The anti-diagonal's
        // endpoints disagree (lower-right, upper-left).
        var mainDiagonal = (a.GridX == minX) == (a.GridY == minY);
        return (minX, minY, mainDiagonal);
    }

    /// <summary>
    ///     Records the diagonal occupied by link {<paramref name="a" />,<paramref name="b" />} in
    ///     <paramref name="occupiedDiagonals" />. If the opposite diagonal of the same unit-square
    ///     is already taken the two would visually cross: when <paramref name="failOnCrossing" /> is
    ///     <c>true</c> this returns <c>false</c> without recording anything (the caller drops or
    ///     rejects the link); when <c>false</c> the crossing is accepted but the diagonal is still
    ///     recorded so later optional cross-edges steer clear of the now fully-occupied square.
    ///     Cardinal links never cross, so they're accepted (and not recorded) immediately.
    /// </summary>
    private static bool TryRegisterEdge(
        HashSet<(int MinX, int MinY, bool MainDiagonal)> occupiedDiagonals,
        RoomGraphNode a,
        RoomGraphNode b,
        bool failOnCrossing = true)
    {
        if (GetDiagonalKey(a, b) is not { } diagonal)
        {
            return true;
        }

        if (failOnCrossing
            && occupiedDiagonals.Contains((diagonal.MinX, diagonal.MinY, !diagonal.MainDiagonal)))
        {
            return false;
        }

        occupiedDiagonals.Add(diagonal);
        return true;
    }

    /// <summary>
    ///     How many times an ear is re-sampled from <see cref="FindEar" /> trying to find one whose
    ///     links don't visually cross an already-placed link before the crossing is accepted as
    ///     unavoidable. The connection phase is cheap, so a handful of re-rolls clears the vast
    ///     majority of avoidable crossings without meaningfully slowing generation.
    /// </summary>
    private const int MaxEarCrossingRerolls = 8;

    /// <summary>
    ///     Returns whether attaching <paramref name="ear" /> would place a diagonal that crosses one
    ///     already in <paramref name="occupiedDiagonals" /> or an earlier diagonal of the same ear.
    ///     Pure test (records nothing), so a crossing ear can be discarded for a re-rolled one.
    /// </summary>
    private static bool EarIntroducesCrossing(
        HashSet<(int MinX, int MinY, bool MainDiagonal)> occupiedDiagonals,
        List<RoomGraphNode> ear)
    {
        HashSet<(int MinX, int MinY, bool MainDiagonal)>? earDiagonals = null;
        for (var i = 0; i < ear.Count - 1; i++)
        {
            if (GetDiagonalKey(ear[i], ear[i + 1]) is not { } diagonal)
            {
                continue;
            }

            var opposite = (diagonal.MinX, diagonal.MinY, !diagonal.MainDiagonal);
            if (occupiedDiagonals.Contains(opposite) || (earDiagonals?.Contains(opposite) ?? false))
            {
                return true;
            }

            (earDiagonals ??= []).Add(diagonal);
        }

        return false;
    }

    /// <summary>
    ///     Counts how many pairs of links in <paramref name="directedEdges" /> visually cross — i.e.
    ///     grid unit-squares that hold both of their diagonals. Bidirectional pairs (two directed
    ///     edges over the same geometry) collapse to one diagonal, so they're never miscounted as a
    ///     crossing. Used to pick the cleanest of several sampled connection arrangements.
    /// </summary>
    private static int CountConnectionCrossings(HashSet<(RoomGraphNode Src, RoomGraphNode Tgt)> directedEdges)
    {
        var occupied = new HashSet<(int MinX, int MinY, bool MainDiagonal)>();
        var crossings = 0;
        foreach (var (src, tgt) in directedEdges)
        {
            if (GetDiagonalKey(src, tgt) is not { } diagonal)
            {
                continue;
            }

            if (occupied.Contains((diagonal.MinX, diagonal.MinY, !diagonal.MainDiagonal)))
            {
                crossings++;
            }

            occupied.Add(diagonal);
        }

        return crossings;
    }

    /// <summary>
    ///     Post-processing "untwist" pass run after all links are placed: for every grid unit-square
    ///     that still holds both of its diagonals (the two links render as a crossing X), tries to
    ///     resolve the crossing via <see cref="TryUntwistSquare" /> — re-pairing the four corner rooms
    ///     along the square's cardinal sides, or, failing that, bending one diagonal into a cardinal L
    ///     while keeping the other. This resolves crossings that the placement-time avoidance had to
    ///     accept as unavoidable.
    ///     <para>
    ///         The request's "select either the source rooms or the target rooms and swap the crossed
    ///         edges between them" is realized by the re-pair strategy, which tries both cardinal
    ///         matchings of the four corners (a main-diagonal corner is always cardinally adjacent to
    ///         either anti-diagonal corner, so both matchings are crossing-free) and commits the first
    ///         that preserves every invariant. Any fix is accepted only when the resulting directed
    ///         graph still has the entrance forward-reaching every room and every room forward-reaching
    ///         the exit; the new links are oriented along the topological <paramref name="order" />, so
    ///         acyclicity (and, for any still-reachable interior room, its incoming + outgoing edge) is
    ///         preserved by construction. A replacement link that duplicates an existing edge is
    ///         allowed to collapse onto it. If no fix validates, the crossing is left untouched.
    ///     </para>
    /// </summary>
    private static void UntwistCrossings(
        HashSet<(RoomGraphNode Src, RoomGraphNode Tgt)> directed,
        Dictionary<RoomGraphNode, double> order,
        List<RoomGraphNode> nodes,
        RoomGraphNode entrance,
        RoomGraphNode exit,
        HashSet<(int MinX, int MinY, bool MainDiagonal)> occupiedDiagonals,
        LevelGenerationStats? stats)
    {
        // Index each diagonal link by its unit-square. A square that carries both its main and its
        // anti diagonal is a crossing to untwist.
        var bySquare =
            new Dictionary<(int MinX, int MinY),
                ((RoomGraphNode Src, RoomGraphNode Tgt)? Main, (RoomGraphNode Src, RoomGraphNode Tgt)? Anti)>();
        foreach (var edge in directed)
        {
            if (GetDiagonalKey(edge.Src, edge.Tgt) is not { } diagonal)
            {
                continue;
            }

            var square = (diagonal.MinX, diagonal.MinY);
            bySquare.TryGetValue(square, out var slot);
            if (diagonal.MainDiagonal)
            {
                slot.Main = edge;
            }
            else
            {
                slot.Anti = edge;
            }

            bySquare[square] = slot;
        }

        foreach (var (square, slot) in bySquare)
        {
            if (slot is not { Main: { } mainEdge, Anti: { } antiEdge })
            {
                continue;
            }

            if (!TryUntwistSquare(
                    directed, order, nodes, entrance, exit, square, occupiedDiagonals, mainEdge, antiEdge))
            {
                continue;
            }

            if (stats is not null) { stats.RoomGraphConnectionUntwisted++; }
        }
    }

    /// <summary>
    ///     Tries to untwist the crossing formed by <paramref name="mainEdge" /> and
    ///     <paramref name="antiEdge" /> (the two diagonals of one unit-square), preferring the least
    ///     disruptive fix:
    ///     <list type="number">
    ///         <item>
    ///             <description>
    ///                 <b>Re-pair</b> — remove both diagonals and reconnect the four corner rooms
    ///                 along the square's cardinal sides, trying each of the two perfect matchings of
    ///                 main-corners to anti-corners. This keeps the link count unchanged (or lower, if
    ///                 a side collapses onto an existing edge).
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <b>Bend</b> — when neither matching validates (re-pairing would strand a
    ///                 corner that depended on a diagonal for its only incoming/outgoing edge),
    ///                 reroute just one diagonal into a cardinal L through an off-diagonal corner and
    ///                 keep the other diagonal (now alone in the square, it no longer crosses). A bend
    ///                 preserves the rerouted link's endpoints' mutual connectivity exactly — they
    ///                 stay joined through the waypoint corner — so it rescues crossings that
    ///                 re-pairing cannot. It is tried second because it adds one link (two cardinal
    ///                 sides replacing one diagonal).
    ///             </description>
    ///         </item>
    ///     </list>
    ///     A fix is committed only when the resulting directed graph still has the entrance
    ///     forward-reaching every room and every room forward-reaching the exit; otherwise every
    ///     tentative change is rolled back and the crossing is left untouched. The
    ///     <paramref name="occupiedDiagonals" /> set is updated to release whichever diagonal(s) the
    ///     committed fix vacated.
    /// </summary>
    private static bool TryUntwistSquare(
        HashSet<(RoomGraphNode Src, RoomGraphNode Tgt)> directed,
        Dictionary<RoomGraphNode, double> order,
        List<RoomGraphNode> nodes,
        RoomGraphNode entrance,
        RoomGraphNode exit,
        (int MinX, int MinY) square,
        HashSet<(int MinX, int MinY, bool MainDiagonal)> occupiedDiagonals,
        (RoomGraphNode Src, RoomGraphNode Tgt) mainEdge,
        (RoomGraphNode Src, RoomGraphNode Tgt) antiEdge)
    {
        var (m1, m2) = mainEdge;
        var (a1, a2) = antiEdge;

        // Strategy 1 — re-pair: drop both diagonals and reconnect along the cardinal sides. Matching
        // A pairs the corners one way ({m1,a1} + {m2,a2}); matching B the other ({m1,a2} + {m2,a1}).
        // Both consist solely of cardinal links, so neither can re-introduce a crossing; they differ
        // only in which rooms become directly connected, mirroring the "source-side vs target-side"
        // choice.
        directed.Remove(mainEdge);
        directed.Remove(antiEdge);
        if (TryCommitLinkPair(directed, order, nodes, entrance, exit, m1, a1, m2, a2)
            || TryCommitLinkPair(directed, order, nodes, entrance, exit, m1, a2, m2, a1))
        {
            occupiedDiagonals.Remove((square.MinX, square.MinY, true));
            occupiedDiagonals.Remove((square.MinX, square.MinY, false));
            return true;
        }

        directed.Add(mainEdge);
        directed.Add(antiEdge);

        // Strategy 2 — bend one diagonal through an off-diagonal corner, keeping the other. Identify
        // the four corners by position: the main diagonal joins LL=(minX,minY) and UR=(minX+1,minY+1)
        // (LL has the smaller X); the anti diagonal joins LR=(minX+1,minY) and UL=(minX,minY+1) (LR
        // has the larger X). Either off-diagonal corner is cardinally adjacent to both endpoints of
        // the diagonal being bent, so the L-path's two sides are always cardinal.
        var ll = m1.GridX < m2.GridX ? m1 : m2;
        var ur = m1.GridX < m2.GridX ? m2 : m1;
        var lr = a1.GridX > a2.GridX ? a1 : a2;
        var ul = a1.GridX > a2.GridX ? a2 : a1;

        // Bend the main diagonal (keep anti) via LR or UL.
        if (TryBendDiagonal(directed, order, nodes, entrance, exit, mainEdge, ll, ur, lr)
            || TryBendDiagonal(directed, order, nodes, entrance, exit, mainEdge, ll, ur, ul))
        {
            occupiedDiagonals.Remove((square.MinX, square.MinY, true));
            return true;
        }

        // Bend the anti diagonal (keep main) via LL or UR.
        if (TryBendDiagonal(directed, order, nodes, entrance, exit, antiEdge, lr, ul, ll)
            || TryBendDiagonal(directed, order, nodes, entrance, exit, antiEdge, lr, ul, ur))
        {
            occupiedDiagonals.Remove((square.MinX, square.MinY, false));
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Reroutes the diagonal link {<paramref name="p" />,<paramref name="q" />} into a cardinal
    ///     L-path through <paramref name="waypoint" /> (an off-diagonal corner adjacent to both
    ///     endpoints): removes the diagonal and adds the two oriented sides {p,waypoint} and
    ///     {waypoint,q} via <see cref="TryCommitLinkPair" />. Keeps the change only if the DAG stays
    ///     fully connected; otherwise the sides are rolled back and the diagonal restored. Returns
    ///     whether the bend was committed.
    /// </summary>
    private static bool TryBendDiagonal(
        HashSet<(RoomGraphNode Src, RoomGraphNode Tgt)> directed,
        Dictionary<RoomGraphNode, double> order,
        List<RoomGraphNode> nodes,
        RoomGraphNode entrance,
        RoomGraphNode exit,
        (RoomGraphNode Src, RoomGraphNode Tgt) diagonal,
        RoomGraphNode p,
        RoomGraphNode q,
        RoomGraphNode waypoint)
    {
        directed.Remove(diagonal);
        if (TryCommitLinkPair(directed, order, nodes, entrance, exit, p, waypoint, waypoint, q))
        {
            return true;
        }

        directed.Add(diagonal);
        return false;
    }

    /// <summary>
    ///     Adds the two cardinal links {<paramref name="a1" />,<paramref name="b1" />} and
    ///     {<paramref name="a2" />,<paramref name="b2" />} — each oriented from its lower-order
    ///     endpoint to its higher-order one (see <see cref="OrientEdge" />) — to
    ///     <paramref name="directed" /> and keeps them only if the DAG stays fully connected;
    ///     otherwise rolls back exactly the links it introduced (a link that merely collapsed onto an
    ///     existing edge is left in place). Returns whether the pair was committed. Shared by the
    ///     re-pair and bend untwist strategies.
    /// </summary>
    private static bool TryCommitLinkPair(
        HashSet<(RoomGraphNode Src, RoomGraphNode Tgt)> directed,
        Dictionary<RoomGraphNode, double> order,
        List<RoomGraphNode> nodes,
        RoomGraphNode entrance,
        RoomGraphNode exit,
        RoomGraphNode a1,
        RoomGraphNode b1,
        RoomGraphNode a2,
        RoomGraphNode b2)
    {
        var edge1 = OrientEdge(a1, b1, order);
        var edge2 = OrientEdge(a2, b2, order);
        var added1 = directed.Add(edge1);
        var added2 = directed.Add(edge2);

        if (ValidateDagConnectivity(nodes, entrance, exit, directed))
        {
            return true;
        }

        if (added2)
        {
            directed.Remove(edge2);
        }

        if (added1)
        {
            directed.Remove(edge1);
        }

        return false;
    }

    /// <summary>
    ///     Orders the undirected link {<paramref name="a" />,<paramref name="b" />} from the
    ///     lower-order endpoint to the higher-order one (ties broken by GridX then GridY), matching
    ///     the orientation rule used throughout the DAG so an added link never forms a back edge.
    /// </summary>
    private static (RoomGraphNode Src, RoomGraphNode Tgt) OrientEdge(
        RoomGraphNode a, RoomGraphNode b, Dictionary<RoomGraphNode, double> order)
        => CompareNodes(a, b, order) < 0 ? (a, b) : (b, a);

    /// <summary>
    ///     Subclass hook: emits the initial set of nodes (with grid positions and the
    ///     Entrance/Exit features). Default implementation throws.
    /// </summary>
    protected virtual List<RoomGraphNode> PlaceRooms(LayoutParameters parameters, SimpleRandom random)
        => throw new NotImplementedException(
            $"{GetType().Name} does not implement {nameof(PlaceRooms)}; override it or do not call {nameof(BuildGraph)}.");

    /// <summary>
    ///     Picks the entrance→exit "spine" of the graph: a simple path through the
    ///     undirected adjacency. With <see cref="LayoutParameters.Sinuosity" /> = 0 the
    ///     returned path is exactly the BFS shortest path; with sinuosity 1 it's the
    ///     longest path found across <see cref="LongestPathAttempts" /> Warnsdorff walks.
    ///     Intermediate values run Warnsdorff walks too but pick the candidate whose
    ///     length is closest to the interpolated target.
    /// </summary>
    private static List<RoomGraphNode>? FindSpine(
        RoomGraphNode entrance,
        RoomGraphNode exit,
        Dictionary<RoomGraphNode, HashSet<RoomGraphNode>> adjacency,
        float sinuosity,
        SimpleRandom random)
    {
        var shortest = BfsShortestPath(entrance, exit, adjacency);
        if (shortest is null)
        {
            return null;
        }

        if (sinuosity <= 0f)
        {
            return shortest;
        }

        var bestLongest = shortest;
        var visited = new HashSet<RoomGraphNode>(adjacency.Count);
        for (var attempt = 0; attempt < LongestPathAttempts; attempt++)
        {
            var swap = attempt >= LongestPathAttempts / 2;
            var start = swap ? exit : entrance;
            var target = swap ? entrance : exit;
            visited.Clear();
            var walk = WarnsdorffWalkPath(start, target, adjacency, visited, random);
            if (walk is null)
            {
                continue;
            }
            if (swap)
            {
                walk.Reverse();
            }
            if (walk.Count > bestLongest.Count)
            {
                bestLongest = walk;
            }
            if (bestLongest.Count - 1 >= adjacency.Count - 1)
            {
                break; // can't get longer
            }
        }

        if (sinuosity >= 1f || bestLongest == shortest)
        {
            return bestLongest;
        }

        // Pick whichever candidate is closer to the interpolated target length.
        var targetLen = shortest.Count - 1
            + (bestLongest.Count - shortest.Count) * (double)sinuosity;
        return Math.Abs(shortest.Count - 1 - targetLen) <= Math.Abs(bestLongest.Count - 1 - targetLen)
            ? shortest
            : bestLongest;
    }

    /// <summary>
    ///     BFS shortest path on the undirected <paramref name="adjacency" />, returned as
    ///     the node sequence (inclusive of both endpoints). Returns <c>null</c> if
    ///     <paramref name="target" /> is unreachable from <paramref name="source" />.
    /// </summary>
    private static List<RoomGraphNode>? BfsShortestPath(
        RoomGraphNode source,
        RoomGraphNode target,
        Dictionary<RoomGraphNode, HashSet<RoomGraphNode>> adjacency)
    {
        var parent = new Dictionary<RoomGraphNode, RoomGraphNode?> { [source] = null };
        var queue = new Queue<RoomGraphNode>();
        queue.Enqueue(source);
        var found = source == target;
        while (queue.Count > 0 && !found)
        {
            var current = queue.Dequeue();
            foreach (var neighbor in adjacency[current])
            {
                if (parent.ContainsKey(neighbor))
                {
                    continue;
                }
                parent[neighbor] = current;
                if (neighbor == target)
                {
                    found = true;
                    break;
                }
                queue.Enqueue(neighbor);
            }
        }

        if (!parent.ContainsKey(target))
        {
            return null;
        }

        var path = new List<RoomGraphNode>();
        RoomGraphNode? cursor = target;
        // BFS builds an acyclic parent tree rooted at `source` (whose entry maps to null),
        // so the walk must terminate within parent.Count steps. Cap to fail fast on any
        // future regression that would otherwise spin until OOM.
        var safetyBudget = parent.Count + 1;
        while (cursor != null)
        {
            if (--safetyBudget < 0)
            {
                throw new InvalidOperationException(
                    $"BfsShortestPath parent chain exceeded {parent.Count} iterations; likely cycle in parent[].");
            }

            path.Add(cursor);
            cursor = parent[cursor];
        }
        path.Reverse();
        return path;
    }

    private static Dictionary<RoomGraphNode, int> GetDirectedDistances(
        RoomGraphNode source,
        IReadOnlyDictionary<RoomGraphNode, List<RoomGraphNode>> adjacency)
    {
        var distances = new Dictionary<RoomGraphNode, int>
        {
            [source] = 0
        };
        var queue = new Queue<RoomGraphNode>();
        queue.Enqueue(source);
        while (queue.TryDequeue(out var node))
        {
            var nextDistance = distances[node] + 1;
            foreach (var neighbor in adjacency[node])
            {
                if (distances.TryAdd(neighbor, nextDistance))
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        return distances;
    }

    /// <summary>
    ///     Finds an <b>ear</b>: a simple path <c>[anchor1, uncoveredInternal..., anchor2]</c>
    ///     where both anchors are already covered (have an entry in <paramref name="order" />)
    ///     and every internal node is uncovered. Internal nodes may be empty (a chord
    ///     between two covered nodes) — the algorithm prefers ears that absorb at least
    ///     one uncovered node so progress is monotone. Returns <c>null</c> when no
    ///     valid ear exists (typically a degree-1 leaf the trim pass missed).
    /// </summary>
    private static List<RoomGraphNode>? FindEar(
        HashSet<RoomGraphNode> uncovered,
        Dictionary<RoomGraphNode, double> order,
        Dictionary<RoomGraphNode, HashSet<RoomGraphNode>> adjacency,
        SimpleRandom random)
    {
        // Iterate uncovered nodes in randomised order so the ears don't always grow from
        // the same end of the grid.
        var seeds = new List<RoomGraphNode>(uncovered);
        ShuffleInPlace(seeds, random);

        foreach (var seed in seeds)
        {
            // Find anchor1: any covered neighbor of seed.
            RoomGraphNode? anchor1 = null;
            foreach (var n in adjacency[seed])
            {
                if (order.ContainsKey(n))
                {
                    anchor1 = n;
                    break;
                }
            }
            if (anchor1 is null)
            {
                continue;
            }

            // BFS through uncovered nodes (excluding anchor1) until we hit a different
            // covered node — that's anchor2.
            var path = BfsThroughUncovered(seed, anchor1, uncovered, order, adjacency);
            if (path is null)
            {
                continue;
            }

            // Prepend anchor1 so the returned ear is [anchor1, seed, ..., anchor2].
            path.Insert(0, anchor1);
            return path;
        }

        return null;
    }

    /// <summary>
    ///     BFS from <paramref name="seed" /> through nodes in <paramref name="uncovered" />
    ///     plus any covered node except <paramref name="excludedAnchor" />. Returns the
    ///     simple node path <c>[seed, uncovered..., anchor2]</c> when a second covered
    ///     anchor is reachable; <c>null</c> otherwise.
    /// </summary>
    private static List<RoomGraphNode>? BfsThroughUncovered(
        RoomGraphNode seed,
        RoomGraphNode excludedAnchor,
        HashSet<RoomGraphNode> uncovered,
        Dictionary<RoomGraphNode, double> order,
        Dictionary<RoomGraphNode, HashSet<RoomGraphNode>> adjacency)
    {
        var parent = new Dictionary<RoomGraphNode, RoomGraphNode?> { [seed] = null };
        var queue = new Queue<RoomGraphNode>();
        queue.Enqueue(seed);
        RoomGraphNode? anchor2 = null;
        while (queue.Count > 0 && anchor2 is null)
        {
            var current = queue.Dequeue();
            foreach (var neighbor in adjacency[current])
            {
                if (parent.ContainsKey(neighbor) || neighbor == excludedAnchor)
                {
                    continue;
                }
                parent[neighbor] = current;
                if (order.ContainsKey(neighbor))
                {
                    anchor2 = neighbor;
                    break;
                }
                if (!uncovered.Contains(neighbor))
                {
                    // Covered but excluded — shouldn't happen since the only covered node
                    // we exclude is anchor1, but keep the guard for defensive clarity.
                    continue;
                }
                queue.Enqueue(neighbor);
            }
        }

        if (anchor2 is null)
        {
            return null;
        }

        var path = new List<RoomGraphNode>();
        RoomGraphNode? cursor = anchor2;
        // Parent dictionary is an acyclic BFS tree rooted at `seed`; cap to fail fast
        // on a future regression rather than grow the list until OOM.
        var safetyBudget = parent.Count + 1;
        while (cursor != null)
        {
            if (--safetyBudget < 0)
            {
                throw new InvalidOperationException(
                    $"BfsThroughUncovered parent chain exceeded {parent.Count} iterations; likely cycle in parent[].");
            }

            path.Add(cursor);
            cursor = parent[cursor];
        }
        path.Reverse();
        return path;
    }

    /// <summary>
    ///     Attaches <paramref name="ear" /> to the graph: assigns fractional topological
    ///     orders to the internal nodes interpolated between the two anchors' orders,
    ///     adds directed edges along the ear oriented earlier→later, and removes the
    ///     internal nodes from <paramref name="uncovered" />.
    /// </summary>
    private static void AttachEar(
        List<RoomGraphNode> ear,
        Dictionary<RoomGraphNode, double> order,
        HashSet<(RoomGraphNode Src, RoomGraphNode Tgt)> directedEdges,
        HashSet<RoomGraphNode> uncovered)
    {
        // Reverse the ear if needed so traversal direction matches the topological order
        // (anchor with the lower order is first). Internals then receive strictly
        // monotonically increasing fractional orders along the path, so every edge in
        // the ear gets oriented in its traversal direction — internals end up with both
        // an incoming and an outgoing edge, preserving the "reachable from entrance,
        // reaches exit" invariant.
        if (order[ear[0]] > order[ear[^1]])
        {
            ear.Reverse();
        }

        var anchor1 = ear[0];
        var anchor2 = ear[^1];
        var lo = order[anchor1];
        var hi = order[anchor2];

        // When both anchors share the same order (two ear-internals previously attached
        // by ears with identical anchor pairs end up at the same interpolated value),
        // fractional interpolation has no room. Bump every node with order ≥ lo (except
        // anchor1) by a fixed gap so anchor2 ends up strictly above anchor1 with an
        // empty interval available for the internals. Relative ordering of all existing
        // pairs is preserved (anything below lo is unchanged; everything ≥ lo shifts by
        // the same constant), so previously committed directed edges remain valid.
        if (lo >= hi)
        {
            const double gap = 1.0;
            var keys = order.Keys.ToList();
            foreach (var key in keys)
            {
                if (key != anchor1 && order[key] >= lo)
                {
                    order[key] += gap;
                }
            }
            hi = order[anchor2];
        }

        var internalCount = ear.Count - 2;
        if (internalCount > 0)
        {
            var step = (hi - lo) / (internalCount + 1);
            for (var i = 1; i <= internalCount; i++)
            {
                var node = ear[i];
                order[node] = lo + step * i;
                uncovered.Remove(node);
            }
        }

        // Orient every edge along the ear earlier-order → later-order. Since the ear
        // is now sorted by increasing order, this equals its traversal direction.
        for (var i = 0; i < ear.Count - 1; i++)
        {
            AddOrientedEdge(ear[i], ear[i + 1], directedEdges, order);
        }
    }

    /// <summary>
    ///     Adds the directed edge between <paramref name="a" /> and <paramref name="b" />
    ///     to <paramref name="directedEdges" />, oriented from the lower-order endpoint
    ///     to the higher-order one. Ties (identical orders, possible when two ears share
    ///     anchors) are broken by GridX then GridY for determinism.
    /// </summary>
    private static void AddOrientedEdge(
        RoomGraphNode a,
        RoomGraphNode b,
        HashSet<(RoomGraphNode Src, RoomGraphNode Tgt)> directedEdges,
        Dictionary<RoomGraphNode, double> order)
    {
        if (CompareNodes(a, b, order) < 0)
        {
            directedEdges.Add((a, b));
        }
        else
        {
            directedEdges.Add((b, a));
        }
    }

    private static int CompareNodes(RoomGraphNode a, RoomGraphNode b, Dictionary<RoomGraphNode, double> order)
    {
        var c = order[a].CompareTo(order[b]);
        if (c != 0)
        {
            return c;
        }
        c = a.GridX.CompareTo(b.GridX);
        return c != 0 ? c : a.GridY.CompareTo(b.GridY);
    }

    /// <summary>
    ///     Defence-in-depth check that every non-endpoint room has at least two undirected
    ///     neighbors in the topology adjacency. Ear decomposition can't produce a violator
    ///     on a well-formed adjacency (every covered internal of an ear is bracketed by
    ///     two neighbors), but a bug in <see cref="PlaceRooms" /> or
    ///     <see cref="TrimRooms" /> could leave a dangling leaf — fail loudly in
    ///     that case so the caller retries with a different seed.
    /// </summary>
    private static bool HasMinDegree(
        List<RoomGraphNode> nodes,
        RoomGraphNode entrance,
        RoomGraphNode exit,
        Dictionary<RoomGraphNode, HashSet<RoomGraphNode>> adjacency)
    {
        foreach (var node in nodes)
        {
            if (node == entrance || node == exit)
            {
                continue;
            }
            if (adjacency[node].Count < 2)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    ///     Iteratively drops non-endpoint nodes with fewer than two undirected neighbors
    ///     from <paramref name="adjacency" /> and <paramref name="nodes" />. A degree-1
    ///     node can't lie on any simple entrance→exit path through itself, so it would
    ///     break the ear-decomposition invariant; removing one may turn its sole
    ///     neighbor into a leaf in turn, so the trim cascades until the graph stabilises.
    ///     Entrance and exit are kept regardless of their degree (BuildGraph short-circuits
    ///     to null afterwards if they ended up isolated).
    /// </summary>
    private static void TrimLeaves(
        List<RoomGraphNode> nodes,
        RoomGraphNode entrance,
        RoomGraphNode exit,
        Dictionary<RoomGraphNode, HashSet<RoomGraphNode>> adjacency)
    {
        var queue = new Queue<RoomGraphNode>();
        foreach (var node in nodes)
        {
            if (node != entrance && node != exit && adjacency[node].Count < 2)
            {
                queue.Enqueue(node);
            }
        }

        var toRemove = new HashSet<RoomGraphNode>();
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            if (!toRemove.Add(node))
            {
                continue;
            }

            foreach (var neighbor in adjacency[node])
            {
                adjacency[neighbor].Remove(node);
                if (neighbor != entrance && neighbor != exit
                    && adjacency[neighbor].Count < 2
                    && !toRemove.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                }
            }
            adjacency.Remove(node);
        }

        if (toRemove.Count > 0)
        {
            nodes.RemoveAll(toRemove.Contains);
        }
    }

    /// <summary>
    ///     Verifies the DAG is well-formed: every node is forward-reachable from
    ///     <paramref name="entrance" /> AND every node forward-reaches <paramref name="exit" />.
    ///     Mirrors the test-only room-graph connectivity check but operates on the
    ///     pre-materialization edge set so failures don't escape to the rendering layer.
    /// </summary>
    private static bool ValidateDagConnectivity(
        List<RoomGraphNode> nodes,
        RoomGraphNode entrance,
        RoomGraphNode exit,
        HashSet<(RoomGraphNode Src, RoomGraphNode Tgt)> directedEdges)
    {
        var outgoing = new Dictionary<RoomGraphNode, List<RoomGraphNode>>(nodes.Count);
        var incoming = new Dictionary<RoomGraphNode, List<RoomGraphNode>>(nodes.Count);
        foreach (var n in nodes)
        {
            outgoing[n] = [];
            incoming[n] = [];
        }
        foreach (var (src, tgt) in directedEdges)
        {
            outgoing[src].Add(tgt);
            incoming[tgt].Add(src);
        }

        return DagReach(entrance, outgoing) == nodes.Count
            && DagReach(exit, incoming) == nodes.Count;
    }

    private static int DagReach(
        RoomGraphNode source,
        Dictionary<RoomGraphNode, List<RoomGraphNode>> adjacency)
    {
        var visited = new HashSet<RoomGraphNode> { source };
        var queue = new Queue<RoomGraphNode>();
        queue.Enqueue(source);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var n in adjacency[current])
            {
                if (visited.Add(n))
                {
                    queue.Enqueue(n);
                }
            }
        }
        return visited.Count;
    }

    /// <summary>
    ///     Adds forward-oriented edges from the topology adjacency that aren't already in
    ///     the DAG, up to the <see cref="LayoutParameters.Interconnectedness" /> target.
    ///     "Forward" is determined by the topological <paramref name="order" /> so adding
    ///     a cross-edge never introduces a back edge and the DAG property is preserved.
    /// </summary>
    private static void AddCrossEdges(
        Dictionary<RoomGraphNode, HashSet<RoomGraphNode>> adjacency,
        HashSet<(RoomGraphNode Src, RoomGraphNode Tgt)> directedEdges,
        Dictionary<RoomGraphNode, double> order,
        float interconnectedness,
        SimpleRandom random,
        HashSet<(int MinX, int MinY, bool MainDiagonal)> occupiedDiagonals)
    {
        // Each undirected adjacency edge gives at most one directed cross-edge candidate:
        // the forward-orientation. Skip edges already on the DAG.
        var candidates = new List<(RoomGraphNode Src, RoomGraphNode Tgt)>();
        foreach (var (u, neighbors) in adjacency)
        {
            foreach (var v in neighbors)
            {
                if (CompareNodes(u, v, order) >= 0)
                {
                    continue; // dedupe to one direction
                }
                if (directedEdges.Contains((u, v)))
                {
                    continue;
                }
                candidates.Add((u, v));
            }
        }

        ShuffleInPlace(candidates, random);
        var toAdd = (int)Math.Round(candidates.Count * (double)interconnectedness);
        var added = 0;
        foreach (var candidate in candidates)
        {
            if (added >= toAdd)
            {
                break;
            }

            // An optional cross-edge that would visually overlap an existing link is simply
            // dropped (unlike a required spine/ear link, it isn't needed for connectivity), keeping
            // the rendered map readable without failing the whole attempt.
            if (!TryRegisterEdge(occupiedDiagonals, candidate.Src, candidate.Tgt))
            {
                continue;
            }

            directedEdges.Add(candidate);
            added++;
        }
    }

    /// <summary>
    ///     Materializes the directed edge set into a <see cref="RoomGraph" /> with hidden
    ///     flags and reward / curse features applied. Every source keeps at least one
    ///     visible outgoing edge so a room is never displayed as completely sealed off,
    ///     even when its other edges roll hidden.
    /// </summary>
    private RoomGraph BuildFinalGraph(
        List<RoomGraphNode> nodes,
        HashSet<(RoomGraphNode Src, RoomGraphNode Tgt)> directedEdges,
        List<RoomGraphNode> spine,
        LayoutParameters parameters,
        SimpleRandom random,
        LevelGenerationStats stats,
        GenerationStats gameStats)
    {
        // A room can be concealed on the map only when (a) it sits off the entrance→exit
        // spine (the shortest route to the exit must always stay visible) and (b) hiding it
        // can't strand a predecessor — see IsHideable. Concealment is enabled only when
        // HiddenConnectionProbability > 0 (a 0 draws no RNG, so generation is unchanged — e.g.
        // the entrance level keeps every connection visible).
        var onSpine = parameters.HiddenConnectionProbability > 0
            ? new HashSet<RoomGraphNode>(spine)
            : null;
        var graph = new RoomGraph();
        graph.Rooms.AddRange(nodes);

        // Build every directed edge visible up-front and tally degrees. `degree` (in + out)
        // drives the reward "discovery prize" boost for low-degree rooms. `visibleOutgoing`
        // starts at each node's outgoing degree and is decremented as rooms are hidden (it's
        // the live count of a node's still-visible outgoing edges); together with
        // `incomingNeighbors` it drives the dead-end hideability test below.
        var degree = new Dictionary<RoomGraphNode, int>(nodes.Count);
        var visibleOutgoing = new Dictionary<RoomGraphNode, int>(nodes.Count);
        var incomingNeighbors = new Dictionary<RoomGraphNode, List<RoomGraphNode>>(nodes.Count);
        var outgoingNeighbors = nodes.ToDictionary(
            node => node,
            _ => new List<RoomGraphNode>());
        var reverseNeighbors = nodes.ToDictionary(
            node => node,
            _ => new List<RoomGraphNode>());
        foreach (var (source, target) in directedEdges)
        {
            graph.Connections.Add(new RoomGraphEdge { Source = source, Target = target, Hidden = false });
            outgoingNeighbors[source].Add(target);
            reverseNeighbors[target].Add(source);
            degree[source] = degree.GetValueOrDefault(source) + 1;
            degree[target] = degree.GetValueOrDefault(target) + 1;
            visibleOutgoing[source] = visibleOutgoing.GetValueOrDefault(source) + 1;
            if (!incomingNeighbors.TryGetValue(target, out var predecessors))
            {
                predecessors = [];
                incomingNeighbors[target] = predecessors;
            }
            predecessors.Add(source);
        }

        var entrance = nodes.Single(
            node => (node.Features & RoomFeatures.Entrance) != 0);
        var exit = nodes.Single(
            node => (node.Features & RoomFeatures.Exit) != 0);
        var distanceFromEntrance = GetDirectedDistances(entrance, outgoingNeighbors);
        var distanceToExit = GetDirectedDistances(exit, reverseNeighbors);
        var shortestPathLength = distanceFromEntrance[exit];
        var onShortestPath = nodes
            .Where(node => distanceFromEntrance.TryGetValue(node, out var fromEntrance)
                           && distanceToExit.TryGetValue(node, out var toExit)
                           && fromEntrance + toExit == shortestPathLength)
            .ToHashSet();

        // Apply Reward / Curse feature tags. Rooms with low total degree (one in + one
        // out) are more likely to receive rewards as a "discovery prize". Reward picks
        // are made with knowledge of what has been assigned so far (both on this level
        // and across the game), so subclasses can weight features dynamically (e.g.
        // clamp a feature to a max count by zeroing its weight once the cap is reached).
        // When stats is supplied the running counts are persisted (and shared with
        // PickRewardFeature) via <see cref="GenerationStats.RoomCountsByFeature" />.
        IReadOnlyDictionary<RoomFeatures, int> levelCounts = stats.RoomCountsByFeature;
        IReadOnlyDictionary<RoomFeatures, int> globalCounts = gameStats.RoomCountsByFeature;
        var hiddenRooms = onSpine != null ? new HashSet<RoomGraphNode>() : null;

        // Spread the configured probability across the hideable rooms so
        // HiddenConnectionProbability is the expected NUMBER of hidden rooms per level rather
        // than a per-room chance — a bigger level doesn't get proportionally more secrets. The
        // hideable count is taken against the initial (nothing-hidden) state; hiding a room only
        // ever shrinks the hideable set, so this is an upper bound and the realised hidden count
        // stays at or below HiddenConnectionProbability. Computing the count draws no RNG
        // (IsHideable is deterministic), so the per-room loop's random stream is unperturbed.
        var hideChance = 0f;
        if (onSpine != null)
        {
            var hideableCount = 0;
            foreach (var node in nodes)
            {
                if ((node.Features & (RoomFeatures.Entrance | RoomFeatures.Exit)) == 0
                    && IsHideable(node, onSpine, hiddenRooms!, visibleOutgoing, incomingNeighbors))
                {
                    hideableCount++;
                }
            }

            hideChance = hideableCount > 0 ? parameters.HiddenConnectionProbability / hideableCount : 0f;
        }

        foreach (var node in nodes)
        {
            if ((node.Features & (RoomFeatures.Entrance | RoomFeatures.Exit)) != 0)
            {
                stats.RecordRoomFeature(node.Features);
                gameStats.RecordRoomFeature(node.Features);
                continue;
            }

            // Decide concealment first: only a hideable room can be hidden, and only on the
            // per-room hide roll (HiddenConnectionProbability spread across the hideable rooms —
            // see hideChance above). A hidden room is always a reward room (a "secret" worth
            // finding) and draws from the premium-biased reward set, which additionally unlocks
            // the Shortcut reward (a hidden inter-level passage). The `&&` short-circuits before
            // the RNG draw for non-hideable rooms, so disabling the feature (onSpine == null)
            // consumes no randomness.
            var hidden = hiddenRooms != null
                && IsHideable(node, onSpine!, hiddenRooms, visibleOutgoing, incomingNeighbors)
                && random.NextSingle(0f, 1f) < hideChance;

            var d = degree.GetValueOrDefault(node);
            var isDeadEnd = d <= 2;
            var rewardChance = isDeadEnd
                ? Math.Min(1f, parameters.RewardRoomProbability * 1.5f)
                : parameters.RewardRoomProbability;
            // A hidden room always rewards the detour; a visible room rolls the normal chance.
            if (hidden || random.NextSingle(0f, 1f) < rewardChance)
            {
                var reward = PickRewardFeature(
                    levelCounts,
                    globalCounts,
                    parameters,
                    random,
                    hidden,
                    allowPremium: !onShortestPath.Contains(node));
                if (reward != RoomFeatures.Normal)
                {
                    node.Features |= reward;
                    stats.RecordRoomFeature(reward);
                    gameStats.RecordRoomFeature(reward);
                }
            }

            if (random.NextSingle(0f, 1f) < parameters.CurseRoomProbability)
            {
                node.Features |= RoomFeatures.Cursed;
                stats.RecordRoomFeature(RoomFeatures.Cursed);
                gameStats.RecordRoomFeature(RoomFeatures.Cursed);
            }

            if (hidden)
            {
                hiddenRooms!.Add(node);
                // Hiding this room hides every still-visible edge into it, dropping each
                // non-hidden predecessor's visible-outgoing count. Maintaining the count here
                // is what stops a predecessor of two hidden rooms from losing all its visible
                // exits (a cumulative dead end the per-room check alone wouldn't catch).
                if (incomingNeighbors.TryGetValue(node, out var predecessors))
                {
                    for (var i = 0; i < predecessors.Count; i++)
                    {
                        if (!hiddenRooms.Contains(predecessors[i]))
                        {
                            visibleOutgoing[predecessors[i]]--;
                        }
                    }
                }
            }
        }

        // Conceal every hidden room: hide all connections incident to it (in both directions)
        // so neither the room nor the passages to it appear on the map until the player
        // discovers it by reaching an adjacent room (revealed via TravelSystem / SensorySystem).
        // Hidden edges remain traversable and still count for graph reachability. No-op when
        // concealment is disabled (hiddenRooms == null) or no room was hidden.
        if (hiddenRooms is { Count: > 0 })
        {
            foreach (var edge in graph.Connections)
            {
                if (hiddenRooms.Contains(edge.Source) || hiddenRooms.Contains(edge.Target))
                {
                    edge.Hidden = true;
                }
            }
        }

        return graph;
    }

    /// <summary>
    ///     Whether <paramref name="node" /> may be concealed on the map. A room is hideable
    ///     only when it sits off the entrance→exit <paramref name="onSpine" /> spine (the
    ///     shortest route to the exit must always be visible) and hiding it can't make a
    ///     <em>visible</em> predecessor look like a dead end: every non-hidden room with an edge
    ///     into it must keep at least one other visible outgoing edge
    ///     (<paramref name="visibleOutgoing" /> &#8805; 2, so it stays &#8805; 1 after this room's
    ///     incoming edge is hidden). Predecessors that are already hidden don't display, so they
    ///     can't become visible dead ends and are skipped.
    /// </summary>
    private static bool IsHideable(
        RoomGraphNode node,
        HashSet<RoomGraphNode> onSpine,
        HashSet<RoomGraphNode> hiddenRooms,
        Dictionary<RoomGraphNode, int> visibleOutgoing,
        Dictionary<RoomGraphNode, List<RoomGraphNode>> incomingNeighbors)
    {
        if (onSpine.Contains(node))
        {
            return false;
        }

        if (incomingNeighbors.TryGetValue(node, out var preds))
        {
            for (var i = 0; i < preds.Count; i++)
            {
                var pred = preds[i];
                if (!hiddenRooms.Contains(pred) && visibleOutgoing.GetValueOrDefault(pred) < 2)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    ///     The reward <see cref="RoomFeatures" /> flags that <see cref="PickRewardFeature" />
    ///     can assign, in the canonical order it iterates them when sampling a weighted pick.
    ///     <see cref="RoomFeatures.Shortcut" /> is reward-only and selectable exclusively for
    ///     concealed (hidden) rooms — <see cref="GetRewardFeatureWeight" /> returns 0 for it
    ///     on a visible room.
    /// </summary>
    private static readonly RoomFeatures[] RewardFeatureCandidates =
    [
        RoomFeatures.DeityAltar,
        RoomFeatures.MajorShrine,
        RoomFeatures.MinorShrine,
        RoomFeatures.RitualShrine,
        RoomFeatures.SmallFountain,
        RoomFeatures.BigFountain,
        RoomFeatures.SkillPedestal,
        RoomFeatures.RacePedestal,
        RoomFeatures.TraitPedestal,
        RoomFeatures.FeatPedestal,
        RoomFeatures.ArmorStand,
        RoomFeatures.GildedArmorStand,
        RoomFeatures.ItemCase,
        RoomFeatures.GildedItemCase,
        RoomFeatures.WeaponStand,
        RoomFeatures.GildedWeaponStand,
        RoomFeatures.Shortcut
    ];

    public const RoomFeatures PremiumRewardFeatures =
        RoomFeatures.MajorShrine
        | RoomFeatures.BigFountain
        | RoomFeatures.GildedArmorStand
        | RoomFeatures.GildedItemCase
        | RoomFeatures.GildedWeaponStand;

    private const int LongestPathAttempts = 4;

    /// <summary>
    ///     Walks the graph from <paramref name="start" /> toward <paramref name="target" />
    ///     using Warnsdorff's rule: at each step the next node is the unvisited neighbor
    ///     with the fewest unvisited neighbors (ties broken at random). The target is
    ///     held back until no other neighbor is available so the walk doesn't terminate
    ///     prematurely with many nodes still unvisited. Returns the resulting node path
    ///     (inclusive of both endpoints) or <c>null</c> if the walk dead-ends before
    ///     reaching the target.
    /// </summary>
    private static List<RoomGraphNode>? WarnsdorffWalkPath(
        RoomGraphNode start,
        RoomGraphNode target,
        Dictionary<RoomGraphNode, HashSet<RoomGraphNode>> adjacency,
        HashSet<RoomGraphNode> visited,
        SimpleRandom random)
    {
        var nodeCount = adjacency.Count;
        var path = new List<RoomGraphNode> { start };
        visited.Add(start);
        var current = start;
        while (current != target)
        {
            RoomGraphNode? bestNext = null;
            var bestScore = int.MaxValue;
            var tieCount = 0;
            var deferredTarget = false;

            foreach (var neighbor in adjacency[current])
            {
                if (visited.Contains(neighbor))
                {
                    continue;
                }

                if (neighbor == target && visited.Count < nodeCount - 1)
                {
                    deferredTarget = true;
                    continue;
                }

                var score = 0;
                foreach (var nn in adjacency[neighbor])
                {
                    if (!visited.Contains(nn))
                    {
                        score++;
                    }
                }

                if (score < bestScore)
                {
                    bestScore = score;
                    bestNext = neighbor;
                    tieCount = 1;
                }
                else if (score == bestScore)
                {
                    // Reservoir sampling for fair random tie-breaking.
                    tieCount++;
                    if (random.Next(tieCount) == 0)
                    {
                        bestNext = neighbor;
                    }
                }
            }

            if (bestNext is null)
            {
                if (deferredTarget)
                {
                    path.Add(target);
                    return path;
                }
                return null;
            }

            visited.Add(bestNext);
            path.Add(bestNext);
            current = bestNext;
        }

        return path;
    }

    private static void ShuffleInPlace<T>(IList<T> list, SimpleRandom random)
    {
        // Fisher–Yates.
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>
    ///     Shuffle helper exposed to subclasses that want to randomize candidate placement
    ///     order.
    /// </summary>
    protected static void Shuffle<T>(IList<T> list, SimpleRandom random) => ShuffleInPlace(list, random);

    /// <summary>
    ///     Builds a symmetrized undirected adjacency map over <paramref name="nodes" />
    ///     using <see cref="GetNeighborOffsets" />. Each node maps to the set of nodes
    ///     in <paramref name="nodes" /> that are its topological neighbors; asymmetric
    ///     offsets (e.g. hex parity) are symmetrized so each edge appears in both
    ///     endpoints' sets.
    /// </summary>
    private Dictionary<RoomGraphNode, HashSet<RoomGraphNode>> BuildTopologyAdjacency(List<RoomGraphNode> nodes)
    {
        var byCell = new Dictionary<(byte X, byte Y), RoomGraphNode>(nodes.Count);
        foreach (var node in nodes)
        {
            byCell[(node.GridX, node.GridY)] = node;
        }

        var adjacency = new Dictionary<RoomGraphNode, HashSet<RoomGraphNode>>(nodes.Count);
        foreach (var node in nodes)
        {
            adjacency[node] = [];
        }

        foreach (var node in nodes)
        {
            foreach (var (dx, dy) in GetNeighborOffsets(node.GridX, node.GridY))
            {
                var nx = node.GridX + dx;
                var ny = node.GridY + dy;
                if (nx < 0 || ny < 0 || nx > byte.MaxValue || ny > byte.MaxValue)
                {
                    continue;
                }

                if (byCell.TryGetValue(((byte)nx, (byte)ny), out var neighbor))
                {
                    adjacency[node].Add(neighbor);
                    adjacency[neighbor].Add(node);
                }
            }
        }

        return adjacency;
    }

    /// <summary>
    ///     Restores a previously-trimmed room into the live graph when the ear
    ///     decomposition can't make progress (an uncovered region is cut off from the
    ///     covered graph, or no second covered anchor is reachable through other
    ///     uncovered nodes). Scores candidates in <paramref name="removedPool" /> by how
    ///     useful their kept-graph neighbor set is for unblocking ear-finding:
    ///     <list type="number">
    ///         <item>Best: ≥ 2 currently covered neighbors — forms an instant ear
    ///             <c>[covered, restored, covered']</c> at the next <see cref="FindEar" /> call.</item>
    ///         <item>Good: ≥ 1 covered + ≥ 1 uncovered neighbor — creates a fresh
    ///             uncovered-region bridge to the covered graph.</item>
    ///         <item>Useful: ≥ 2 uncovered neighbors — joins two uncovered components
    ///             together (helps when there's currently no anchor1 at all).</item>
    ///     </list>
    ///     Candidates with fewer than 2 live neighbors overall are skipped (restoring
    ///     them would just create a leaf that the rest of the pipeline can't use).
    ///     Returns <c>false</c> when <see cref="LayoutParameters.MaxRooms" /> is hit or
    ///     no candidate qualifies; the caller then bumps
    ///     <see cref="GenerationStats.RoomGraphEarNotFound" /> and returns <c>null</c>.
    /// </summary>
    private static bool TryRestoreRoomForEar(
        HashSet<RoomGraphNode> removedPool,
        Dictionary<RoomGraphNode, HashSet<RoomGraphNode>> fullAdjacency,
        Dictionary<RoomGraphNode, HashSet<RoomGraphNode>> adjacency,
        HashSet<RoomGraphNode> uncovered,
        Dictionary<RoomGraphNode, double> order,
        List<RoomGraphNode> nodes,
        byte maxRooms)
    {
        if (removedPool.Count == 0 || nodes.Count >= maxRooms)
        {
            return false;
        }

        RoomGraphNode? best = null;
        var bestScore = 0;
        foreach (var candidate in removedPool)
        {
            var covered = 0;
            var uncoveredCount = 0;
            foreach (var n in fullAdjacency[candidate])
            {
                if (!adjacency.ContainsKey(n))
                {
                    continue; // n is also trimmed
                }
                if (order.ContainsKey(n))
                {
                    covered++;
                }
                else if (uncovered.Contains(n))
                {
                    uncoveredCount++;
                }
            }

            if (covered + uncoveredCount < 2)
            {
                continue;
            }

            int score;
            if (covered >= 2)
            {
                score = 1000 + covered * 10 + uncoveredCount;
            }
            else if (covered >= 1 && uncoveredCount >= 1)
            {
                score = 500 + uncoveredCount;
            }
            else
            {
                score = 100 + uncoveredCount;
            }

            if (score > bestScore)
            {
                bestScore = score;
                best = candidate;
            }
        }

        if (best is null)
        {
            return false;
        }

        removedPool.Remove(best);
        nodes.Add(best);
        uncovered.Add(best);
        var newNeighbors = new HashSet<RoomGraphNode>();
        foreach (var n in fullAdjacency[best])
        {
            if (adjacency.ContainsKey(n))
            {
                newNeighbors.Add(n);
                adjacency[n].Add(best);
            }
        }
        adjacency[best] = newNeighbors;
        return true;
    }

    /// <summary>
    ///     Builds the topology adjacency from the layout's
    ///     <see cref="GetNeighborOffsets" />, samples a target room count from a normal
    ///     distribution clamped to <c>[minRooms, maxRooms]</c>, and trims the placement
    ///     down to that target by repeatedly removing a randomly-chosen non-endpoint room
    ///     that satisfies two constraints:
    ///     <list type="bullet">
    ///         <item>
    ///             It is <b>not an articulation point</b> of the undirected adjacency —
    ///             removing it does not disconnect the graph, so the number of
    ///             edge-disjoint entrance→exit paths is preserved as much as possible.
    ///         </item>
    ///         <item>
    ///             Every surviving topological neighbor keeps at least 2 neighbors of
    ///             its own — preserving the
    ///             "every non-endpoint room has ≥ 2 undirected neighbors" invariant
    ///             <see cref="BuildGraph" /> relies on.
    ///         </item>
    ///     </list>
    ///     Trim stops early when no removal satisfies both constraints, even if the
    ///     target count hasn't been reached; the level still satisfies
    ///     <see cref="LayoutParameters.MinRooms" /> downstream as long as the initial
    ///     placement does.
    /// </summary>
    /// <returns>
    ///     A symmetrized undirected adjacency map: each surviving <see cref="RoomGraphNode" />
    ///     maps to the set of its topological neighbors that also survived the trim.
    /// </returns>
    protected Dictionary<RoomGraphNode, HashSet<RoomGraphNode>> TrimRooms(
        List<RoomGraphNode> nodes,
        byte minRooms,
        byte maxRooms,
        SimpleRandom random)
    {
        var adjacency = BuildTopologyAdjacency(nodes);

        // Sample target room count from a normal distribution centred on (min+max)/2
        // with stddev (max-min)/4 (≈95% of samples fall in the [min,max] window before
        // clamping), so most levels end up near the middle of the room budget but the
        // size varies seed-to-seed.
        var target = SampleTargetRoomCount(minRooms, maxRooms, random);
        if (nodes.Count <= target)
        {
            return adjacency;
        }

        var entrance = nodes.Find(n => (n.Features & RoomFeatures.Entrance) != 0);
        var exit = nodes.Find(n => (n.Features & RoomFeatures.Exit) != 0);

        // Trim greedily, preferring removals that keep the remaining graph
        // 2-vertex-connected (no cut vertices). A 2-vertex-connected graph admits an
        // open ear decomposition by Whitney's theorem, so preserving this property here
        // dramatically reduces downstream EarNotFound rejections in BuildGraph. If no
        // biconnectivity-preserving removal is available, fall back to any non-cut-vertex
        // removal (which still preserves connectivity).
        var currentCutVertices = new HashSet<RoomGraphNode>(nodes.Count);
        var postRemovalCutVertices = new HashSet<RoomGraphNode>(nodes.Count);
        var preservingCandidates = new List<RoomGraphNode>(nodes.Count);
        var fallbackCandidates = new List<RoomGraphNode>(nodes.Count);
        var nodesSnapshot = new List<RoomGraphNode>(nodes.Count);
        while (nodes.Count > target)
        {
            ComputeArticulationPoints(nodes, adjacency, currentCutVertices);

            preservingCandidates.Clear();
            fallbackCandidates.Clear();
            nodesSnapshot.Clear();
            nodesSnapshot.AddRange(nodes);
            foreach (var node in nodesSnapshot)
            {
                if (node == entrance || node == exit || currentCutVertices.Contains(node))
                {
                    continue;
                }
                if (!IsRemovalSafeForNeighborDegrees(node, adjacency))
                {
                    continue;
                }

                fallbackCandidates.Add(node);

                // Tentatively remove and check whether the remaining graph stays
                // biconnected (no cut vertices). If so, this is a "preserving"
                // candidate and gets priority. We mutate adjacency in place and restore
                // it afterwards to avoid copying the entire map per candidate.
                var savedNeighbors = adjacency[node];
                adjacency.Remove(node);
                foreach (var n in savedNeighbors)
                {
                    adjacency[n].Remove(node);
                }
                nodes.Remove(node);

                ComputeArticulationPoints(nodes, adjacency, postRemovalCutVertices);
                var preservesBiconnectivity = postRemovalCutVertices.Count == 0;

                nodes.Add(node);
                adjacency[node] = savedNeighbors;
                foreach (var n in savedNeighbors)
                {
                    adjacency[n].Add(node);
                }

                if (preservesBiconnectivity)
                {
                    preservingCandidates.Add(node);
                }
            }

            var pool = preservingCandidates.Count > 0 ? preservingCandidates : fallbackCandidates;
            if (pool.Count == 0)
            {
                break;
            }

            var victim = pool[random.Next(pool.Count)];
            foreach (var neighbor in adjacency[victim])
            {
                adjacency[neighbor].Remove(victim);
            }
            adjacency.Remove(victim);
            nodes.Remove(victim);
        }

        return adjacency;
    }

    /// <summary>
    ///     Samples an integer target room count biased toward the midpoint of
    ///     <paramref name="minRooms" /> .. <paramref name="maxRooms" />. Averaging two
    ///     uniform draws yields a triangular distribution peaking at the centre — a cheap,
    ///     center-weighted approximation that doesn't need to be statistically precise.
    ///     <c>min == max</c> short-circuits to that value.
    /// </summary>
    private static int SampleTargetRoomCount(byte minRooms, byte maxRooms, SimpleRandom random)
    {
        if (minRooms >= maxRooms)
        {
            return maxRooms;
        }
        var a = random.Next(minRooms, maxRooms + 1);
        var b = random.Next(minRooms, maxRooms + 1);
        return (a + b) / 2;
    }

    /// <summary>
    ///     Returns <c>true</c> when removing <paramref name="node" /> leaves every
    ///     surviving neighbor with at least two of its own neighbors. Pure local check
    ///     using current adjacency counts — pair this with an articulation-point check
    ///     to guarantee the global connectivity invariant.
    /// </summary>
    private static bool IsRemovalSafeForNeighborDegrees(
        RoomGraphNode node,
        Dictionary<RoomGraphNode, HashSet<RoomGraphNode>> adjacency)
    {
        foreach (var neighbor in adjacency[node])
        {
            if (adjacency[neighbor].Count - 1 < 2)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    ///     Tarjan's articulation-point (cut-vertex) detection on the undirected
    ///     <paramref name="adjacency" />. A vertex is an articulation point if removing
    ///     it disconnects the graph into more components; <see cref="TrimRooms" /> uses
    ///     this set to keep candidate removals safe for connectivity. <paramref name="result" />
    ///     is reused across iterations — cleared and repopulated each call.
    /// </summary>
    private static void ComputeArticulationPoints(
        List<RoomGraphNode> nodes,
        Dictionary<RoomGraphNode, HashSet<RoomGraphNode>> adjacency,
        HashSet<RoomGraphNode> result)
    {
        result.Clear();
        var disc = new Dictionary<RoomGraphNode, int>(nodes.Count);
        var low = new Dictionary<RoomGraphNode, int>(nodes.Count);
        var timer = 0;
        foreach (var start in nodes)
        {
            if (!disc.ContainsKey(start))
            {
                ArticulationDfs(start, parent: null, isRoot: true, adjacency, disc, low, ref timer, result);
            }
        }
    }

    private static void ArticulationDfs(
        RoomGraphNode u,
        RoomGraphNode? parent,
        bool isRoot,
        Dictionary<RoomGraphNode, HashSet<RoomGraphNode>> adjacency,
        Dictionary<RoomGraphNode, int> disc,
        Dictionary<RoomGraphNode, int> low,
        ref int timer,
        HashSet<RoomGraphNode> result)
    {
        disc[u] = low[u] = timer++;
        var children = 0;
        foreach (var v in adjacency[u])
        {
            if (!disc.ContainsKey(v))
            {
                children++;
                ArticulationDfs(v, u, isRoot: false, adjacency, disc, low, ref timer, result);
                low[u] = Math.Min(low[u], low[v]);
                if (!isRoot && low[v] >= disc[u])
                {
                    result.Add(u);
                }
            }
            else if (v != parent)
            {
                low[u] = Math.Min(low[u], disc[v]);
            }
        }
        if (isRoot && children > 1)
        {
            result.Add(u);
        }
    }

    /// <summary>
    ///     Picks the reward <see cref="RoomFeatures" /> flag assigned to a reward room by
    ///     summing per-feature weights from <see cref="GetRewardFeatureWeight" /> and
    ///     sampling proportionally. Subclasses tune the distribution by overriding the
    ///     weight hook (e.g. zeroing out deep-tier features on the entrance level, or
    ///     capping a feature by returning 0 once <paramref name="assignedCounts" /> shows
    ///     it has already been placed enough times). When <paramref name="hidden" /> is
    ///     <c>true</c> the room is concealed on the map, which biases the draw toward
    ///     premium features and unlocks the <see cref="RoomFeatures.Shortcut" /> reward.
    ///     Returns <see cref="RoomFeatures.Normal" /> when every weight is zero so the
    ///     caller leaves the room un-tagged.
    /// </summary>
    private RoomFeatures PickRewardFeature(
        IReadOnlyDictionary<RoomFeatures, int> assignedCounts,
        IReadOnlyDictionary<RoomFeatures, int> globalAssignedCounts,
        LayoutParameters parameters,
        SimpleRandom random,
        bool hidden,
        bool allowPremium)
        => random.Pick(
            RewardFeatureCandidates,
            feature => !allowPremium
                       && (feature & PremiumRewardFeatures) != 0
                ? 0f
                : Math.Max(0f, GetRewardFeatureWeight(
                    feature,
                    assignedCounts.GetValueOrDefault(feature),
                    globalAssignedCounts.GetValueOrDefault(feature),
                    parameters,
                    hidden)));

    /// <summary>
    ///     Relative "quality" of each reward feature, used to skew a concealed (hidden)
    ///     room's reward draw toward better finds — a secret room should reward the detour.
    ///     Higher is rarer / more desirable. Applied as a multiplier on the base weight only
    ///     when the room is hidden; visible rooms use the unmodified base distribution.
    ///     <see cref="RoomFeatures.Shortcut" /> is weighted directly in
    ///     <see cref="GetRewardFeatureWeight" /> and isn't listed here.
    /// </summary>
    private static float GetHiddenRewardMultiplier(RoomFeatures feature)
        => feature switch
        {
            RoomFeatures.RitualShrine => 4f,
            RoomFeatures.MajorShrine => 3f,
            RoomFeatures.BigFountain => 3f,
            RoomFeatures.GildedArmorStand => 3f,
            RoomFeatures.GildedItemCase => 3f,
            RoomFeatures.GildedWeaponStand => 3f,
            RoomFeatures.DeityAltar => 1.5f,
            RoomFeatures.RacePedestal => 1.5f,
            RoomFeatures.MinorShrine => 1.5f,
            RoomFeatures.ItemCase => 1.5f,
            RoomFeatures.WeaponStand => 1.5f,
            RoomFeatures.ArmorStand => 1.5f,
            _ => 1f
        };

    /// <summary>
    ///     Returns the relative selection weight for <paramref name="feature" /> on the
    ///     current reward draw. Weights scale with <see cref="LayoutParameters.Difficulty" />
    ///     so deeper levels see more deep-tier features (BigFountain, MajorShrine,
    ///     RitualShrine) while shallow levels stay focused on simpler rewards. The
    ///     <paramref name="countSoFar" /> argument is the per-level count (lets caps
    ///     express themselves as zero weights — e.g. DeityAltar / RacePedestal cap at
    ///     one per level on early floors). <paramref name="globalCountSoFar" /> is the
    ///     game-wide count across all levels generated so far, useful for soft-throttling
    ///     repeat appearances of premium features. When <paramref name="hidden" /> the
    ///     room is concealed on the map: the <see cref="RoomFeatures.Shortcut" /> reward
    ///     becomes available (and scales with difficulty) and every other feature's weight
    ///     is multiplied by <see cref="GetHiddenRewardMultiplier" /> so premium finds dominate.
    ///     Subclasses can override to skew the distribution further; negative return
    ///     values are treated as 0.
    /// </summary>
    protected virtual float GetRewardFeatureWeight(
        RoomFeatures feature,
        int countSoFar,
        int globalCountSoFar,
        LayoutParameters parameters,
        bool hidden)
    {
        var difficulty = parameters.Difficulty;

        // Shortcut is a hidden-only reward (a concealed inter-level passage that skips a
        // level). It scales with difficulty so deeper dungeons hold more shortcuts, and is
        // soft-throttled on repeats like every other premium reward.
        if (feature == RoomFeatures.Shortcut)
        {
            return hidden
                ? Math.Clamp(difficulty * 4f, 4f, 48f) / ((2 + globalCountSoFar) >> 1)
                : 0f;
        }

        // TODO: Allow this to be overridden in the branch definition
        var baseWeight = feature switch
        {
            // Common shallow rewards — available everywhere.
            RoomFeatures.SmallFountain => 72f,
            RoomFeatures.SkillPedestal => 48f,
            RoomFeatures.TraitPedestal => 48f,
            RoomFeatures.FeatPedestal => 48f,
            RoomFeatures.WeaponStand => 32f,
            RoomFeatures.ArmorStand => 32f,
            RoomFeatures.ItemCase => 20f,
            RoomFeatures.MinorShrine => 16f,
            // Unique-per-level on the introductory floors so the first altar / race choice
            // doesn't immediately duplicate; the cap relaxes once the dungeon proper begins.
            RoomFeatures.DeityAltar => difficulty > 1 || countSoFar < 1 ? 32f : 0f,
            RoomFeatures.RacePedestal => difficulty > 1 || countSoFar < 1 ? 24f : 0f,
            // Deep-tier rewards — progressively unlocked with difficulty.
            RoomFeatures.BigFountain => Math.Clamp((difficulty - 1) * 4f, 0f, 32f),
            RoomFeatures.MajorShrine => Math.Clamp((difficulty - 1) / 2 * 4f, 0f, 24f),
            RoomFeatures.GildedArmorStand => Math.Clamp((difficulty - 1) / 2 * 4f, 0f, 24f),
            RoomFeatures.GildedItemCase => Math.Clamp((difficulty - 1) / 2 * 3f, 0f, 18f),
            RoomFeatures.GildedWeaponStand => Math.Clamp((difficulty - 1) / 2 * 4f, 0f, 24f),
            RoomFeatures.RitualShrine => Math.Clamp((difficulty - 3) / 3 * 4f, 0f, 16f),
            _ => 0f
        };

        // Soft-throttle repeat appearances across the run: each prior placement halves the
        // effective base weight. The (2 + n) >> 1 shape keeps the first two placements at
        // full weight, then decays by integer halving — and never divides by zero.
        var weight = baseWeight / ((2 + globalCountSoFar) >> 1);

        // A concealed room rewards the detour: bias its draw toward rarer, premium features.
        return hidden ? weight * GetHiddenRewardMultiplier(feature) : weight;
    }

    /// <summary>
    ///     Materialises a <see cref="RoomGraph" /> into <see cref="RoomComponent" /> entities and
    ///     intra-level <see cref="ConnectionComponent" /> entities, running
    ///     <see cref="RoomLayout" /> for each room. Each room picks its own
    ///     <see cref="DefiningMapFragment" /> via
    ///     <see cref="LevelGenerator.PickFragmentForRoom" /> based on its
    ///     <see cref="RoomFeatures" />. Effective per-room values
    ///     (RoomWidth/RoomHeight/DefaultTerrain/CreatureGenerator/ItemGenerator) come from
    ///     the picked fragment with fallback to the branch defaults.
    /// </summary>
    /// <remarks>
    ///     Door tiles are picked by <see cref="FindDoorwayInDirection" /> from the chunk
    ///     whose centre is most aligned with the source→target direction. Bidirectional
    ///     pairs (paired forward + reverse edges) become two separate
    ///     <see cref="ConnectionComponent" /> entities with mirrored Hidden flags.
    /// </remarks>
    protected List<Chunk>? MaterializeGraph(GameLevel gameLevel, RoomGraph graph, Branch branch)
    {
        var manager = gameLevel.Game.Manager;
        var allChunks = new List<Chunk>();
        // Per-room chunk lists drive chunk-aware door placement below.
        var chunksByRoom = new Dictionary<int, List<Chunk>>();
        var roomGenerationData = new List<(RoomComponent Room, RoomParameters Parameters, List<Chunk> Chunks)>();
        var stats = gameLevel.GenerationStats;

        // Compute the directional doorway demand per node up-front. Each edge contributes
        // one doorway to its source room in the source→target direction and one doorway
        // to its target room in the reverse direction. Both endpoints must be supplied by
        // the room layout, so RoomLayout.Fill needs the full per-room picture before it
        // commits to a chunk arrangement.
        var doorwayDemand = ComputeDoorwayDemand(graph);

        try
        {
            foreach (var node in graph.Rooms)
            {
                var room = (node.Features & RoomFeatures.Entrance) != 0
                    ? GetEmptyEntranceRoom(gameLevel, manager)
                    : CreateEmptyRoom(gameLevel, manager);
                // Shortcut is mutually exclusive with Entrance/Exit (each designates a
                // distinct connection-sourcing role); if a graph node somehow carries both,
                // the entrance/exit role wins and the shortcut bit is dropped.
                if ((node.Features & (RoomFeatures.Entrance | RoomFeatures.Exit)) != 0)
                {
                    node.Features &= ~RoomFeatures.Shortcut;
                }

                // A room's layout can fail (e.g. it couldn't bind a demanded placeable feature
                // glyph) for a purely RNG-dependent reason. Retry just this room with a fresh
                // fragment selection before throwing away the whole level — a failed room costs
                // one room-fill, a level retry re-fills every room. Only a room that wired no
                // inter-level connection during the failed attempt can be reset safely; one that
                // already sourced/received a cross-level link (and may have created a placeholder
                // target level) falls back to the whole-level retry so the link isn't orphaned.
                const int maxRoomAttempts = 4;
                List<Chunk>? chunks = null;
                RoomParameters? roomParameters = null;
                for (var roomAttempt = 0; roomAttempt < maxRoomAttempts; roomAttempt++)
                {
                    room.GridX = node.GridX;
                    room.GridY = node.GridY;
                    room.Features = node.Features;
                    node.Room = room;

                    // Per-room fragment selection. Throws if no fragment matches the room's
                    // RoomFeatures — a data-set bug.
                    var roomFragment = LevelGenerator.PickFragmentForRoom(
                        gameLevel, node.Features, stats);
                    roomParameters = roomFragment.GetRoomParameters(branch);

                    AllocateTerrain(room, roomParameters.RoomWidth, roomParameters.RoomHeight,
                        roomParameters.DefaultTerrain);

                    var demand = doorwayDemand.TryGetValue(node, out var d)
                        ? d
                        : [];
                    chunks = branch.RoomLayout.Fill(room, branch, roomFragment, demand, roomParameters);
                    if (chunks is not null)
                    {
                        break;
                    }

                    // This attempt's roomParameters isn't in roomGenerationData yet; dispose it
                    // explicitly before retrying or bailing (the finally block returns the rest).
                    roomParameters.Dispose();
                    roomParameters = null;

                    if (room.Connections.Count > 0)
                    {
                        // The room already wired a cross-level connection — fall back to the
                        // whole-level retry rather than orphaning it.
                        return null;
                    }

                    // Reset to a clean placeholder (returns pooled arrays, clears the partial
                    // fill, preserves IncomingConnections) and retry with a fresh fragment.
                    room.Reset();
                }

                if (chunks is null)
                {
                    // Exhausted the per-room retries — let the outer retry try a different seed.
                    return null;
                }

                allChunks.AddRange(chunks);
                chunksByRoom[room.EntityId] = chunks;
                roomGenerationData.Add((room, roomParameters!, chunks));
            }

            // Materialize edges. Bidirectional pairs are present as two separate edges (A→B and
            // B→A) and produce two ConnectionComponent entities — one per direction — with
            // mirrored Hidden flags.
            var usedDoors = new Dictionary<int, HashSet<Point>>();
            foreach (var edge in graph.Connections)
            {
                var sourceRoom = edge.Source.Room!;
                var targetRoom = edge.Target.Room!;

                var sourceDir = GridDeltaToDirection(
                    edge.Target.GridX - edge.Source.GridX, edge.Target.GridY - edge.Source.GridY);
                var targetDir = GridDeltaToDirection(
                    edge.Source.GridX - edge.Target.GridX, edge.Source.GridY - edge.Target.GridY);

                if (!usedDoors.TryGetValue(sourceRoom.EntityId, out var sourceUsed))
                {
                    sourceUsed = [..sourceRoom.Connections.Keys];
                    usedDoors[sourceRoom.EntityId] = sourceUsed;
                }

                if (!usedDoors.TryGetValue(targetRoom.EntityId, out var targetUsed))
                {
                    targetUsed = [..targetRoom.Connections.Keys];
                    usedDoors[targetRoom.EntityId] = targetUsed;
                }

                chunksByRoom.TryGetValue(sourceRoom.EntityId, out var sourceChunksList);
                chunksByRoom.TryGetValue(targetRoom.EntityId, out var targetChunksList);

                var sourceDoor = FindDoorwayInDirection(sourceRoom, sourceDir, sourceUsed,
                    sourceChunksList ?? (IReadOnlyList<Chunk>)Array.Empty<Chunk>());
                var targetDoor = FindDoorwayInDirection(targetRoom, targetDir, targetUsed,
                    targetChunksList ?? (IReadOnlyList<Chunk>)Array.Empty<Chunk>());
                sourceUsed.Add(sourceDoor);
                targetUsed.Add(targetDoor);

                // Carve wall tiles at the chosen door positions to archways so that they are
                // walkable and visually distinct. DoorwayPoints candidates are wall tiles;
                // the fallback may also land on a wall if no walkable tile was closer.
                CarveArchway(sourceRoom, sourceDoor);
                CarveArchway(targetRoom, targetDoor);

                var sourceOrientation = CardinalDirectionFromDoor(sourceRoom, sourceDoor);
                CreateIntraLevelConnection(sourceRoom, sourceDoor, targetRoom, targetDoor, sourceOrientation, edge.Hidden, manager);
            }

            // Carving inter-room archways above turned perimeter walls into walkable cells, which
            // can newly expose the unfilled background on their flanks. Re-seal each room's void so
            // the room-enclosure invariant holds after the graph is fully wired.
            foreach (var (room, roomParameters, _) in roomGenerationData)
            {
                // Wiring the intra-level connections can strand a region whose only link to the rest
                // of the room ran through a tile that just became a connection.
                // ForceConnectWalkableComponents treats connection
                // tiles as non-traversable sinks, so it carves a real corridor to any such region.
                RoomLayout.ForceConnectWalkableComponents(room, roomParameters);
                RoomLayout.SealUnfilledAroundWalkables(room, roomParameters.DefaultWall);
                RoomLayout.SealWalkableBoundary(room, roomParameters.DefaultWall);
            }

            foreach (var (room, roomParameters, chunks) in roomGenerationData)
            {
                roomParameters.CreatureGenerator.Fill(room, chunks);
                roomParameters.ItemGenerator.Fill(room, chunks);

                // Creature/item abilities and their effects enqueue lifecycle notifications. Drain once the
                // fully wired room is populated so a large level cannot exceed the queue's hard limit before
                // LevelGenerator performs its final generation flush.
                manager.Queue.ProcessQueue(manager);
            }

            return allChunks;
        }
        finally
        {
            // Return every rented ChunkCells buffer to the pool. Safe to run on the
            // success, abort, and exception paths alike — Dispose is idempotent and
            // roomGenerationData only contains the parameters we successfully
            // constructed (the abort path disposes the in-flight one inline).
            foreach (var entry in roomGenerationData)
            {
                entry.Parameters.Dispose();
            }
        }
    }

    /// <summary>
    ///     Returns the per-room ordered list of <see cref="RoomDoorwayAnchor" />s: the perimeter
    ///     anchors that bias lot selection and imply the doorway count, based on the directed edges in
    ///     <paramref name="graph" />. Each directed edge <c>(source, target)</c> contributes one
    ///     doorway-demand anchor on the <c>source</c> side facing the target and one on the
    ///     <c>target</c> side facing the source; bidirectional pairs are present as two separate
    ///     edges so the count naturally doubles where both endpoints need two doorways on the same
    ///     side. Anchors are ordered incoming-first (the sides facing edges where the room is the
    ///     target) then outgoing; a room with no incoming edge (the entrance) leads with
    ///     positioning-only anchors opposite its outgoing edges so the outgoing connections spread
    ///     toward those edges rather than crowding the central defining chunk. The facing direction
    ///     is kept as-is (cardinal or ordinal) and only collapsed to cardinals when a lot's edge
    ///     contact is tested.
    /// </summary>
    private static Dictionary<RoomGraphNode, IReadOnlyList<RoomDoorwayAnchor>> ComputeDoorwayDemand(RoomGraph graph)
    {
        var incoming = new Dictionary<RoomGraphNode, List<Direction>>();
        var outgoing = new Dictionary<RoomGraphNode, List<Direction>>();
        foreach (var edge in graph.Connections)
        {
            var dx = edge.Target.GridX - edge.Source.GridX;
            var dy = edge.Target.GridY - edge.Source.GridY;
            AddFacing(outgoing, edge.Source, GridDeltaToDirection(dx, dy));
            AddFacing(incoming, edge.Target, GridDeltaToDirection(-dx, -dy));
        }

        var result = new Dictionary<RoomGraphNode, IReadOnlyList<RoomDoorwayAnchor>>(graph.Rooms.Count);
        foreach (var node in graph.Rooms)
        {
            var hasIncoming = incoming.TryGetValue(node, out var inDirections);
            var hasOutgoing = outgoing.TryGetValue(node, out var outDirections);
            if (!hasIncoming && !hasOutgoing)
            {
                continue;
            }

            var anchors = new List<RoomDoorwayAnchor>();

            // Incoming connections demand a doorway on their facing side and are anchored first.
            if (hasIncoming)
            {
                foreach (var direction in inDirections!)
                {
                    anchors.Add(new RoomDoorwayAnchor(direction, IsDoorwayDemand: true));
                }
            }
            else
            {
                // Entrance-style room (no incoming edge): lead with positioning-only anchors
                // opposite each outgoing connection so the first chunks stay clear of the outgoing
                // side. Rotate(4) is the 180° (4-octant) opposite of a direction.
                foreach (var direction in outDirections!)
                {
                    AddAnchorOnlyDistinct(anchors, direction.Rotate(4));
                }
            }

            // Outgoing connections demand a doorway on their own facing side, anchored last.
            if (hasOutgoing)
            {
                foreach (var direction in outDirections!)
                {
                    anchors.Add(new RoomDoorwayAnchor(direction, IsDoorwayDemand: true));
                }
            }

            result[node] = anchors;
        }

        return result;
    }

    private static void AddFacing(
        Dictionary<RoomGraphNode, List<Direction>> facings,
        RoomGraphNode node,
        Direction direction)
    {
        if (!facings.TryGetValue(node, out var list))
        {
            list = [];
            facings[node] = list;
        }
        list.Add(direction);
    }

    private static void AddAnchorOnlyDistinct(List<RoomDoorwayAnchor> anchors, Direction side)
    {
        for (var i = 0; i < anchors.Count; i++)
        {
            if (anchors[i].Side == side)
            {
                return;
            }
        }
        anchors.Add(new RoomDoorwayAnchor(side, IsDoorwayDemand: false));
    }

    public static Direction GridDeltaToDirection(int dx, int dy)
        => (dx, dy) switch
        {
            ( > 0, 0) => Direction.East,
            ( > 0, < 0) => Direction.Northeast,
            (0, < 0) => Direction.North,
            ( < 0, < 0) => Direction.Northwest,
            ( < 0, 0) => Direction.West,
            ( < 0, > 0) => Direction.Southwest,
            (0, > 0) => Direction.South,
            ( > 0, > 0) => Direction.Southeast,
            _ => Direction.East
        };

    /// <summary>
    ///     Picks a tile inside <paramref name="room" /> for an inter-room connection door.
    ///     Prefers tiles from the chunk closest to the requested
    ///     <paramref name="direction" /> — specifically a <see cref="Chunk.DoorwayPoints" />
    ///     entry that's most aligned with the direction. Falls back to a room-wide walkable
    ///     tile search when chunks are unavailable or have no usable doorway points.
    ///     <para>
    ///         Among the doorway candidates the best-aligned one is used <em>unless</em> turning it
    ///         into a connection would strand part of the room: because stepping onto a connection
    ///         teleports the actor to another room, a doorway that is the sole link between a region
    ///         and the rest of the room is an articulation point (see
    ///         <see cref="DoorwayKeepsRoomConnected" />). When the best-aligned doorway is such a
    ///         point, the next best-aligned doorway that keeps the room connected is chosen instead;
    ///         if every candidate strands something, the best-aligned one is used and the post-wiring
    ///         <see cref="RoomLayout.ForceConnectWalkableComponents" /> pass carves a corridor to the
    ///         stranded region.
    ///     </para>
    /// </summary>
    /// <param name="usedTiles">
    ///     Tiles already claimed by other connections, including pre-existing inter-level
    ///     connections.
    /// </param>
    protected static Point FindDoorwayInDirection(
        RoomComponent room,
        Direction direction,
        HashSet<Point> usedTiles,
        IReadOnlyList<Chunk> chunks)
    {
        var center = new Point((byte)(room.Width / 2), (byte)(room.Height / 2));
        var dirVec = direction.AsVector();

        // Collect every unused doorway candidate, tracking the best-aligned one (the original
        // selection). The picker is permissive about side — it just returns the best-aligned unused
        // doorway, no matter which chunk it belongs to. This mirrors what
        // <see cref="RoomLayout.SatisfiesDemand" /> assumes: total unused doorway count is what
        // matters, not per-chunk distribution.
        var candidates = new List<(Point Point, int Score)>();
        Point? bestDoorway = null;
        var bestScore = int.MinValue;
        for (var i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            if (chunk.Room != room)
            {
                continue;
            }

            foreach (var p in chunk.DoorwayPoints)
            {
                if (usedTiles.Contains(p))
                {
                    continue;
                }

                var score = AlignmentScore(p.X, p.Y, center, dirVec);
                candidates.Add((p, score));
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDoorway = p;
                }
            }
        }

        if (bestDoorway is not { } best)
        {
            throw new InvalidOperationException(
                $"No usable doorway tile found in room {room.EntityId} facing {direction}. "
                + "Ensure the room's fragments provide reachable DoorwayPoints.");
        }

        // Common case: the best-aligned doorway keeps the room connected once it becomes a
        // (non-traversable) connection — use it, unchanged.
        if (DoorwayKeepsRoomConnected(room, best, usedTiles))
        {
            return best;
        }

        // The best-aligned doorway is an articulation point. Prefer the next best-aligned doorway
        // that preserves connectivity. Sort by score desc with a deterministic (X, Y) tie-break.
        candidates.Sort(static (a, b) =>
        {
            var byScore = b.Score.CompareTo(a.Score);
            if (byScore != 0)
            {
                return byScore;
            }

            var byX = a.Point.X.CompareTo(b.Point.X);
            return byX != 0 ? byX : a.Point.Y.CompareTo(b.Point.Y);
        });

        foreach (var (p, _) in candidates)
        {
            if (!p.Equals(best) && DoorwayKeepsRoomConnected(room, p, usedTiles))
            {
                return p;
            }
        }

        // Every candidate strands part of the room — fall back to the best-aligned one; the
        // post-wiring ForceConnectWalkableComponents pass reconnects the stranded region.
        return best;
    }

    /// <summary>
    ///     Whether turning <paramref name="candidate" /> into a connection leaves every other
    ///     walkable tile of <paramref name="room" /> mutually reachable without stepping through a
    ///     connection. Connection tiles (the already-claimed <paramref name="usedTiles" /> plus the
    ///     candidate) teleport the actor to another room, so they are treated as non-traversable
    ///     sinks: a region reachable only through one of them is stranded.
    /// </summary>
    private static bool DoorwayKeepsRoomConnected(
        RoomComponent room, Point candidate, HashSet<Point> usedTiles)
    {
        var pointToIndex = room.PointToIndex!;
        var indexToPoint = room.IndexToPoint!;
        var width = room.Width;
        var height = room.Height;

        bool IsSink(Point p) => candidate.Equals(p) || usedTiles.Contains(p);

        Point? seed = null;
        var total = 0;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var p = new Point((byte)x, (byte)y);
                if (room.Terrain[pointToIndex[x, y]].CanMoveTo() && !IsSink(p))
                {
                    total++;
                    seed ??= p;
                }
            }
        }

        if (seed is not { } start)
        {
            return true;
        }

        var tileCount = room.TileCount;
        var visited = ArrayPool<bool>.Shared.Rent(tileCount);
        try
        {
            Array.Clear(visited, 0, tileCount);
            var queue = new Queue<short>();
            var startIdx = pointToIndex[start.X, start.Y];
            visited[startIdx] = true;
            queue.Enqueue(startIdx);
            var reached = 1;

            while (queue.Count > 0)
            {
                var p = indexToPoint[queue.Dequeue()];
                foreach (var neighbor in RoomLayout.Neighbors8(room, p.X, p.Y))
                {
                    var nIdx = pointToIndex[neighbor.X, neighbor.Y];
                    if (visited[nIdx] || !room.Terrain[nIdx].CanMoveTo() || IsSink(neighbor))
                    {
                        continue;
                    }

                    visited[nIdx] = true;
                    reached++;
                    queue.Enqueue(nIdx);
                }
            }

            // Every relevant walkable tile must have been reached from the single seed.
            return reached == total;
        }
        finally
        {
            ArrayPool<bool>.Shared.Return(visited, clearArray: false);
        }
    }

    private static int AlignmentScore(int x, int y, Point center, Vector dirVec)
        => (x - center.X) * dirVec.X + (y - center.Y) * dirVec.Y;

    private static void CarveArchway(RoomComponent room, Point door)
    {
        var index = room.PointToIndex![door.X, door.Y];
        if (!room.Terrain[index].CanMoveTo())
        {
            TerrainSystem.SetTerrain(MapFeature.StoneArchway, door, room);
        }
    }

    /// <summary>
    ///     Returns the cardinal direction the player faces when entering through the door.
    ///     Examines the four cardinal neighbors: if exactly one is walkable the player
    ///     approaches from that side, so the orientation is the opposite direction (facing
    ///     into the wall). When the surroundings are ambiguous (0 or 2+ walkable
    ///     neighbors) the direction from room centre to door is used as a tiebreaker.
    /// </summary>
    private static Direction CardinalDirectionFromDoor(RoomComponent room, Point door)
    {
        var walkableCount = 0;
        Direction singleWalkableDir = default;

        ReadOnlySpan<Direction> cardinals = [Direction.North, Direction.East, Direction.South, Direction.West];
        foreach (var dir in cardinals)
        {
            var vec = dir.AsVector();
            var nx = door.X + vec.X;
            var ny = door.Y + vec.Y;

            if (nx >= 0 && nx < room.Width && ny >= 0 && ny < room.Height
                && room.Terrain[room.PointToIndex![nx, ny]].CanMoveTo())
            {
                walkableCount++;
                singleWalkableDir = dir;
            }
        }

        if (walkableCount == 1)
        {
            return singleWalkableDir.Rotate(4);
        }

        var cx = room.Width / 2;
        var cy = room.Height / 2;
        var dx = door.X - cx;
        var dy = door.Y - cy;

        return Math.Abs(dx) >= Math.Abs(dy)
            ? dx >= 0 ? Direction.East : Direction.West
            : dy >= 0 ? Direction.South : Direction.North;
    }

    protected static void CreateIntraLevelConnection(
        RoomComponent sourceRoom, Point sourceDoor,
        RoomComponent targetRoom, Point targetDoor,
        Direction direction, bool hidden, GameManager manager)
    {
        using var connectionEntityReference = manager.CreateEntity();
        var connectionEntity = connectionEntityReference.Referenced;

        var position = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
        position.RoomEntity = sourceRoom.Entity;
        position.Cell = sourceDoor;
        connectionEntity.Position = position;

        var connection = manager.CreateComponent<ConnectionComponent>(EntityComponent.Connection);
        connection.TargetRoomId = targetRoom.EntityId;
        connection.TargetCell = targetDoor;
        connection.Directionality = Directionality.Source;
        connection.Orientation = direction;
        connection.Hidden = hidden;
        connectionEntity.Connection = connection;
    }

    /// <summary>
    ///     Creates a <see cref="RoomComponent" /> attached to <paramref name="gameLevel" />.
    ///     The component is constructed with empty (zero-length) terrain placeholders; the
    ///     caller must invoke <see cref="AllocateTerrain" /> before reading or writing
    ///     terrain.
    /// </summary>
    public static RoomComponent CreateEmptyRoom(GameLevel gameLevel, GameManager manager)
    {
        using var roomEntityReference = manager.CreateEntity();
        var room = manager.CreateComponent<RoomComponent>(EntityComponent.Room);

        roomEntityReference.Referenced.Room = room;
        room.Level = gameLevel;
        gameLevel.Rooms.Add(room);

        return room;
    }

    /// <summary>
    ///     Returns the room to use as <paramref name="level" />'s entrance. If a
    ///     placeholder room was created earlier by
    ///     <see cref="LevelConnection.CreateSourceConnection" /> from a neighboring level
    ///     that linked to this level before it existed, that placeholder is reused so its
    ///     <see cref="RoomComponent.IncomingConnections" /> (the neighbor's outbound
    ///     <see cref="ConnectionComponent" />s) end up pointing at the real entrance room
    ///     rather than at an orphaned zero-width room.
    /// </summary>
    public static RoomComponent GetEmptyEntranceRoom(GameLevel level, GameManager manager)
    {
        foreach (var existing in level.Rooms)
        {
            // The placeholder pre-created by a neighboring level: no terrain yet
            // (Width == 0) but at least one incoming connection from that level. Reuse it
            // so the neighbor's outbound connection points at the real entrance room, and
            // (re)assert the Entrance flag — preserved across generation retries by
            // RoomComponent.Reset — so this room is unambiguously, and exclusively, the
            // level's entrance.
            if (existing.Width == 0 && existing.IncomingConnections.Count > 0)
            {
                existing.Features |= RoomFeatures.Entrance;
                return existing;
            }
        }

        return CreateEmptyRoom(level, manager);
    }

    /// <summary>
    ///     Allocates pooled terrain arrays sized to <paramref name="width" />x<paramref name="height" />
    ///     and initialises them. <paramref name="defaultTerrain" /> fills <see cref="RoomComponent.Terrain" />;
    ///     <see cref="RoomComponent.KnownTerrain" /> is filled with <see cref="MapFeature.Unexplored" />.
    /// </summary>
    public static void AllocateTerrain(
        RoomComponent room, byte width, byte height, MapFeature defaultTerrain)
    {
        room.Width = width;
        room.Height = height;
        room.BoundPlaceableFeatures = 0;

        var length = room.TileCount;
        room.Terrain = ArrayPool<MapFeature>.Shared.Rent(length);
        room.Neighbors = ArrayPool<DirectionFlags>.Shared.Rent(length);
        room.KnownNeighbors = ArrayPool<DirectionFlags>.Shared.Rent(length);
        room.VisibleTerrain = ArrayPool<byte>.Shared.Rent(length);
        room.KnownTerrain = ArrayPool<MapFeature>.Shared.Rent(length);

        Array.Clear(room.Terrain, 0, length);
        Array.Clear(room.Neighbors, 0, length);
        Array.Clear(room.KnownNeighbors, 0, length);
        Array.Clear(room.VisibleTerrain, 0, length);

        room.TerrainChanges = null;
        room.KnownTerrainChanges = null;
        room.NeighborChanges = null;

        if (defaultTerrain != MapFeature.Default)
        {
            for (var i = 0; i < length; i++)
            {
                room.Terrain[i] = defaultTerrain;
            }
        }

        for (var i = 0; i < length; i++)
        {
            room.KnownTerrain[i] = MapFeature.Unexplored;
        }

        room.EnsureInitialized();
    }
}
