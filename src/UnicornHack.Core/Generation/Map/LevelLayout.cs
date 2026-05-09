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
///         Subclasses define the *topology* by overriding <see cref="GetNeighbourOffsets" />
///         (4-/6-/8-neighbour and other variants) and <see cref="PlaceRooms" /> (initial cell
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

    /// <summary>Per-edge probability that a connection is hidden (player-discoverable).</summary>
    public float HiddenConnectionProbability { get; init; } = 0.1f;

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
        var parameters = GetParameters(level);
        var graph = BuildGraph(parameters, level.GenerationRandom, level.GenerationStats);
        if (graph == null || graph.Rooms.Count < parameters.MinRooms)
        {
            return null;
        }

        return MaterializeGraph(level, graph, branch);
    }

    /// <summary>
    ///     Returns the per-level <see cref="LayoutParameters" /> snapshot used by
    ///     <see cref="BuildGraph" />. Default implementation returns the layout's instance
    ///     properties verbatim; subclasses override this to return depth-dependent (or
    ///     otherwise dynamic) values without mutating instance state.
    /// </summary>
    public virtual LayoutParameters GetParameters(GameLevel level)
        => new(GridWidth, GridHeight, Interconnectedness, ComputeDensity(),
            Sinuosity, RewardRoomProbability, CurseRoomProbability, HiddenConnectionProbability,
            MinRooms, MaxRooms);

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
    ///     candidate-graph neighbours for this layout's topology. Examples:
    ///     <list type="bullet">
    ///         <item>4-neighbour grid: <c>(±1, 0), (0, ±1)</c></item>
    ///         <item>8-neighbour grid: <c>(±1, 0), (0, ±1), (±1, ±1)</c></item>
    ///         <item>Offset hex: column-parity-dependent (no vertical-only neighbours)</item>
    ///     </list>
    ///     Default implementation throws — subclasses using <see cref="BuildGraph" /> must override.
    /// </summary>
    public virtual IEnumerable<(int dx, int dy)> GetNeighbourOffsets(byte cellX, byte cellY)
        => throw new NotImplementedException(
            $"{GetType().Name} does not implement {nameof(GetNeighbourOffsets)}; override it or do not call {nameof(BuildGraph)}.");

    /// <summary>
    ///     Counts how many neighbours emitted by <see cref="GetNeighbourOffsets" /> fall
    ///     inside the <paramref name="gridWidth" /> × <paramref name="gridHeight" />
    ///     bounding box. Subclasses use this in <see cref="PlaceRooms" /> to skip cells
    ///     that can never have enough connections to satisfy the
    ///     "every non-endpoint room has ≥ 2 undirected neighbours" invariant
    ///     <see cref="BuildGraph" /> requires; placing them just wastes the
    ///     <see cref="TrimRooms" /> budget and lowers the per-attempt success rate.
    /// </summary>
    public int CountInGridNeighbours(byte cellX, byte cellY, byte gridWidth, byte gridHeight)
    {
        var count = 0;
        foreach (var (dx, dy) in GetNeighbourOffsets(cellX, cellY))
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
    ///     two undirected neighbours and lies on a simple entrance-to-exit path — i.e.
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
    ///                 neighbours; otherwise the graph is rejected (the caller retries
    ///                 with a fresh seed).</item>
    ///             <item>Add forward-oriented <b>cross-edges</b> from the adjacency up to
    ///                 the <see cref="LayoutParameters.Interconnectedness" /> target so the
    ///                 graph isn't strictly tree-shaped.</item>
    ///             <item>Each non-mandatory edge is hidden with probability
    ///                 <see cref="LayoutParameters.HiddenConnectionProbability" />; every
    ///                 source keeps at least one visible outgoing edge so a room never
    ///                 looks completely isolated on the map.</item>
    ///             <item>Reward / Curse features applied to non-entrance/non-exit rooms.</item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         Returns <c>null</c> when the adjacency for this seed can't be ear-decomposed
    ///         (an uncovered node with no second covered anchor reachable through other
    ///         uncovered nodes — typically a degree-1 leaf the trim pass couldn't remove).
    ///         The caller retries with a fresh seed.
    ///     </para>
    /// </remarks>
    public RoomGraph? BuildGraph(LayoutParameters parameters, SimpleRandom random)
        => BuildGraph(parameters, random, stats: null);

    /// <summary>
    ///     <see cref="BuildGraph(LayoutParameters,SimpleRandom)" /> overload that records
    ///     the failure category on <paramref name="stats" /> when the graph can't be
    ///     built — each early-return path bumps the matching <c>RoomGraph*</c> counter
    ///     so diagnostic tests can attribute null returns to the specific stage that
    ///     rejected the seed. A <c>null</c> <paramref name="stats" /> disables recording.
    /// </summary>
    public RoomGraph? BuildGraph(
        LayoutParameters parameters,
        SimpleRandom random,
        GenerationStats? stats)
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
        // one neighbour, so any path through it must back-track. Removing leaves can
        // cascade (their sole neighbour may become a leaf in turn). If entrance or exit
        // end up isolated the seed is unusable and we return null so the caller retries.
        TrimLeaves(nodes, entrance, exit, adjacency);
        if (adjacency[entrance].Count == 0 || adjacency[exit].Count == 0 || nodes.Count < 2)
        {
            if (stats is not null) { stats.RoomGraphTrimIsolatedEndpoint++; }
            return null;
        }

        var spine = FindSpine(entrance, exit, adjacency, parameters.Sinuosity, random);
        if (spine is null || spine.Count < 2)
        {
            if (stats is not null) { stats.RoomGraphSpineNotFound++; }
            return null;
        }

        // Topological order. Spine nodes get integer positions; ear internals get
        // fractional values strictly between their two anchor orders. Two ears that share
        // identical anchor orders fall back to GridX/GridY (via CompareNodes) so every
        // undirected edge has a deterministic orientation.
        var order = new Dictionary<RoomGraphNode, double>(nodes.Count);
        for (var i = 0; i < spine.Count; i++)
        {
            order[spine[i]] = i;
        }

        var directedEdges = new HashSet<(RoomGraphNode Src, RoomGraphNode Tgt)>(nodes.Count * 2);
        for (var i = 0; i < spine.Count - 1; i++)
        {
            directedEdges.Add((spine[i], spine[i + 1]));
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
                    return null;
                }
                continue;
            }
            AttachEar(ear, order, directedEdges, uncovered);
        }

        if (!HasMinDegree(nodes, entrance, exit, adjacency))
        {
            if (stats is not null) { stats.RoomGraphMinDegreeViolated++; }
            return null;
        }

        AddCrossEdges(adjacency, directedEdges, order, parameters.Interconnectedness, random);

        // Defence-in-depth: confirm the DAG is well-formed (entrance reaches every node
        // via forward edges and every node reaches exit via forward edges). The ear
        // decomposition guarantees this by construction; failing here means a corner-case
        // bug — return null so the caller retries with a fresh seed rather than handing
        // back a broken graph.
        if (!ValidateDagConnectivity(nodes, entrance, exit, directedEdges))
        {
            if (stats is not null) { stats.RoomGraphDagConnectivityBroken++; }
            return null;
        }

        return BuildFinalGraph(nodes, directedEdges, parameters, random);
    }

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
            foreach (var neighbour in adjacency[current])
            {
                if (parent.ContainsKey(neighbour))
                {
                    continue;
                }
                parent[neighbour] = current;
                if (neighbour == target)
                {
                    found = true;
                    break;
                }
                queue.Enqueue(neighbour);
            }
        }

        if (!parent.ContainsKey(target))
        {
            return null;
        }

        var path = new List<RoomGraphNode>();
        RoomGraphNode? cursor = target;
        while (cursor != null)
        {
            path.Add(cursor);
            cursor = parent[cursor];
        }
        path.Reverse();
        return path;
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
            // Find anchor1: any covered neighbour of seed.
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
            foreach (var neighbour in adjacency[current])
            {
                if (parent.ContainsKey(neighbour) || neighbour == excludedAnchor)
                {
                    continue;
                }
                parent[neighbour] = current;
                if (order.ContainsKey(neighbour))
                {
                    anchor2 = neighbour;
                    break;
                }
                if (!uncovered.Contains(neighbour))
                {
                    // Covered but excluded — shouldn't happen since the only covered node
                    // we exclude is anchor1, but keep the guard for defensive clarity.
                    continue;
                }
                queue.Enqueue(neighbour);
            }
        }

        if (anchor2 is null)
        {
            return null;
        }

        var path = new List<RoomGraphNode>();
        RoomGraphNode? cursor = anchor2;
        while (cursor != null)
        {
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
    ///     neighbours in the topology adjacency. Ear decomposition can't produce a violator
    ///     on a well-formed adjacency (every covered internal of an ear is bracketed by
    ///     two neighbours), but a bug in <see cref="PlaceRooms" /> or
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
    ///     Iteratively drops non-endpoint nodes with fewer than two undirected neighbours
    ///     from <paramref name="adjacency" /> and <paramref name="nodes" />. A degree-1
    ///     node can't lie on any simple entrance→exit path through itself, so it would
    ///     break the ear-decomposition invariant; removing one may turn its sole
    ///     neighbour into a leaf in turn, so the trim cascades until the graph stabilises.
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

            foreach (var neighbour in adjacency[node])
            {
                adjacency[neighbour].Remove(node);
                if (neighbour != entrance && neighbour != exit
                    && adjacency[neighbour].Count < 2
                    && !toRemove.Contains(neighbour))
                {
                    queue.Enqueue(neighbour);
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
    ///     Mirrors <see cref="LevelGenerator.ValidateRoomGraph" /> but operates on the
    ///     pre-materialisation edge set so failures don't escape to the rendering layer.
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
        SimpleRandom random)
    {
        // Each undirected adjacency edge gives at most one directed cross-edge candidate:
        // the forward-orientation. Skip edges already on the DAG.
        var candidates = new List<(RoomGraphNode Src, RoomGraphNode Tgt)>();
        foreach (var (u, neighbours) in adjacency)
        {
            foreach (var v in neighbours)
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
        for (var i = 0; i < toAdd; i++)
        {
            directedEdges.Add(candidates[i]);
        }
    }

    /// <summary>
    ///     Materialises the directed edge set into a <see cref="RoomGraph" /> with hidden
    ///     flags and reward / curse features applied. Every source keeps at least one
    ///     visible outgoing edge so a room is never displayed as completely sealed off,
    ///     even when its other edges roll hidden.
    /// </summary>
    private static RoomGraph BuildFinalGraph(
        List<RoomGraphNode> nodes,
        HashSet<(RoomGraphNode Src, RoomGraphNode Tgt)> directedEdges,
        LayoutParameters parameters,
        SimpleRandom random)
    {
        var graph = new RoomGraph();
        graph.Rooms.AddRange(nodes);
        var degree = new Dictionary<RoomGraphNode, int>(nodes.Count);

        // Group edges by source so each source can reserve one mandatory-visible edge.
        var edgesBySource = new Dictionary<RoomGraphNode, List<RoomGraphNode>>(nodes.Count);
        foreach (var (src, tgt) in directedEdges)
        {
            if (!edgesBySource.TryGetValue(src, out var list))
            {
                list = [];
                edgesBySource[src] = list;
            }
            list.Add(tgt);
        }

        var hideCandidates = new List<(RoomGraphNode Src, RoomGraphNode Tgt)>();
        foreach (var (src, targets) in edgesBySource)
        {
            if (targets.Count == 0)
            {
                continue;
            }
            var keepIndex = random.Next(targets.Count);
            for (var i = 0; i < targets.Count; i++)
            {
                var tgt = targets[i];
                if (i == keepIndex)
                {
                    graph.Connections.Add(new RoomGraphEdge { Source = src, Target = tgt, Hidden = false });
                    degree[src] = degree.GetValueOrDefault(src) + 1;
                    degree[tgt] = degree.GetValueOrDefault(tgt) + 1;
                }
                else
                {
                    hideCandidates.Add((src, tgt));
                }
            }
        }

        var targetHidden = (int)Math.Round(hideCandidates.Count * (double)parameters.HiddenConnectionProbability);
        ShuffleInPlace(hideCandidates, random);
        for (var i = 0; i < hideCandidates.Count; i++)
        {
            var (src, tgt) = hideCandidates[i];
            graph.Connections.Add(new RoomGraphEdge
            {
                Source = src,
                Target = tgt,
                Hidden = i < targetHidden
            });
            degree[src] = degree.GetValueOrDefault(src) + 1;
            degree[tgt] = degree.GetValueOrDefault(tgt) + 1;
        }

        // Apply Reward / Curse feature tags. Rooms with low total degree (one in + one
        // out) are more likely to receive rewards as a "discovery prize".
        foreach (var node in nodes)
        {
            if ((node.Features & (RoomFeatures.Entrance | RoomFeatures.Exit)) != 0)
            {
                continue;
            }

            var d = degree.GetValueOrDefault(node);
            var isDeadEnd = d <= 2;
            var rewardChance = isDeadEnd
                ? Math.Min(1f, parameters.RewardRoomProbability * 1.5f)
                : parameters.RewardRoomProbability;
            if (random.NextSingle(0f, 1f) < rewardChance)
            {
                node.Features |= PickRewardFeature(random);
            }

            if (random.NextSingle(0f, 1f) < parameters.CurseRoomProbability)
            {
                node.Features |= RoomFeatures.Cursed;
            }
        }

        return graph;
    }

    private const int LongestPathAttempts = 4;

    /// <summary>
    ///     Walks the graph from <paramref name="start" /> toward <paramref name="target" />
    ///     using Warnsdorff's rule: at each step the next node is the unvisited neighbour
    ///     with the fewest unvisited neighbours (ties broken at random). The target is
    ///     held back until no other neighbour is available so the walk doesn't terminate
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

            foreach (var neighbour in adjacency[current])
            {
                if (visited.Contains(neighbour))
                {
                    continue;
                }

                if (neighbour == target && visited.Count < nodeCount - 1)
                {
                    deferredTarget = true;
                    continue;
                }

                var score = 0;
                foreach (var nn in adjacency[neighbour])
                {
                    if (!visited.Contains(nn))
                    {
                        score++;
                    }
                }

                if (score < bestScore)
                {
                    bestScore = score;
                    bestNext = neighbour;
                    tieCount = 1;
                }
                else if (score == bestScore)
                {
                    // Reservoir sampling for fair random tie-breaking.
                    tieCount++;
                    if (random.Next(tieCount) == 0)
                    {
                        bestNext = neighbour;
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
    ///     Shuffle helper exposed to subclasses that want to randomise candidate placement
    ///     order.
    /// </summary>
    protected static void Shuffle<T>(IList<T> list, SimpleRandom random) => ShuffleInPlace(list, random);

    /// <summary>
    ///     Builds a symmetrised undirected adjacency map over <paramref name="nodes" />
    ///     using <see cref="GetNeighbourOffsets" />. Each node maps to the set of nodes
    ///     in <paramref name="nodes" /> that are its topological neighbours; asymmetric
    ///     offsets (e.g. hex parity) are symmetrised so each edge appears in both
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
            foreach (var (dx, dy) in GetNeighbourOffsets(node.GridX, node.GridY))
            {
                var nx = node.GridX + dx;
                var ny = node.GridY + dy;
                if (nx < 0 || ny < 0 || nx > byte.MaxValue || ny > byte.MaxValue)
                {
                    continue;
                }

                if (byCell.TryGetValue(((byte)nx, (byte)ny), out var neighbour))
                {
                    adjacency[node].Add(neighbour);
                    adjacency[neighbour].Add(node);
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
    ///     useful their kept-graph neighbour set is for unblocking ear-finding:
    ///     <list type="number">
    ///         <item>Best: ≥ 2 currently covered neighbours — forms an instant ear
    ///             <c>[covered, restored, covered']</c> at the next <see cref="FindEar" /> call.</item>
    ///         <item>Good: ≥ 1 covered + ≥ 1 uncovered neighbour — creates a fresh
    ///             uncovered-region bridge to the covered graph.</item>
    ///         <item>Useful: ≥ 2 uncovered neighbours — joins two uncovered components
    ///             together (helps when there's currently no anchor1 at all).</item>
    ///     </list>
    ///     Candidates with fewer than 2 live neighbours overall are skipped (restoring
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
        var newNeighbours = new HashSet<RoomGraphNode>();
        foreach (var n in fullAdjacency[best])
        {
            if (adjacency.ContainsKey(n))
            {
                newNeighbours.Add(n);
                adjacency[n].Add(best);
            }
        }
        adjacency[best] = newNeighbours;
        return true;
    }

    /// <summary>
    ///     Builds the topology adjacency from the layout's
    ///     <see cref="GetNeighbourOffsets" />, samples a target room count from a normal
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
    ///             Every surviving topological neighbour keeps at least 2 neighbours of
    ///             its own — preserving the
    ///             "every non-endpoint room has ≥ 2 undirected neighbours" invariant
    ///             <see cref="BuildGraph" /> relies on.
    ///         </item>
    ///     </list>
    ///     Trim stops early when no removal satisfies both constraints, even if the
    ///     target count hasn't been reached; the level still satisfies
    ///     <see cref="LayoutParameters.MinRooms" /> downstream as long as the initial
    ///     placement does.
    /// </summary>
    /// <returns>
    ///     A symmetrised undirected adjacency map: each surviving <see cref="RoomGraphNode" />
    ///     maps to the set of its topological neighbours that also survived the trim.
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
                if (!IsRemovalSafeForNeighbourDegrees(node, adjacency))
                {
                    continue;
                }

                fallbackCandidates.Add(node);

                // Tentatively remove and check whether the remaining graph stays
                // biconnected (no cut vertices). If so, this is a "preserving"
                // candidate and gets priority. We mutate adjacency in place and restore
                // it afterwards to avoid copying the entire map per candidate.
                var savedNeighbours = adjacency[node];
                adjacency.Remove(node);
                foreach (var n in savedNeighbours)
                {
                    adjacency[n].Remove(node);
                }
                nodes.Remove(node);

                ComputeArticulationPoints(nodes, adjacency, postRemovalCutVertices);
                var preservesBiconnectivity = postRemovalCutVertices.Count == 0;

                nodes.Add(node);
                adjacency[node] = savedNeighbours;
                foreach (var n in savedNeighbours)
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
            foreach (var neighbour in adjacency[victim])
            {
                adjacency[neighbour].Remove(victim);
            }
            adjacency.Remove(victim);
            nodes.Remove(victim);
        }

        return adjacency;
    }

    /// <summary>
    ///     Samples an integer target room count from a normal distribution centred on
    ///     the midpoint of <paramref name="minRooms" /> .. <paramref name="maxRooms" />,
    ///     clamped to that range. <c>min == max</c> short-circuits to that value.
    /// </summary>
    private static int SampleTargetRoomCount(byte minRooms, byte maxRooms, SimpleRandom random)
    {
        if (minRooms >= maxRooms)
        {
            return maxRooms;
        }
        var mean = (minRooms + maxRooms) / 2.0;
        var stddev = (maxRooms - minRooms) / 4.0;
        // Box-Muller transform — one standard-normal sample from two uniform draws.
        var u1 = Math.Max(random.NextSingle(0f, 1f), 1e-9f);
        var u2 = random.NextSingle(0f, 1f);
        var z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        var sample = mean + stddev * z;
        return (int)Math.Round(Math.Clamp(sample, minRooms, maxRooms));
    }

    /// <summary>
    ///     Returns <c>true</c> when removing <paramref name="node" /> leaves every
    ///     surviving neighbour with at least two of its own neighbours. Pure local check
    ///     using current adjacency counts — pair this with an articulation-point check
    ///     to guarantee the global connectivity invariant.
    /// </summary>
    private static bool IsRemovalSafeForNeighbourDegrees(
        RoomGraphNode node,
        Dictionary<RoomGraphNode, HashSet<RoomGraphNode>> adjacency)
    {
        foreach (var neighbour in adjacency[node])
        {
            if (adjacency[neighbour].Count - 1 < 2)
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

    private static RoomFeatures PickRewardFeature(SimpleRandom random)
    {
        var roll = random.Next(100);
        return roll switch
        {
            < 25 => RoomFeatures.Fountain,
            < 40 => RoomFeatures.SkillPedestal,
            < 55 => RoomFeatures.TraitPedestal,
            < 70 => RoomFeatures.FeatPedestal,
            < 80 => RoomFeatures.WeaponStand,
            < 90 => RoomFeatures.ArmorStand,
            < 95 => RoomFeatures.MinorShrine,
            _ => RoomFeatures.ItemCase
        };
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
        var stats = gameLevel.GenerationStats;

        // Compute the directional doorway demand per node up-front. Each edge contributes
        // one doorway to its source room in the source→target direction and one doorway
        // to its target room in the reverse direction. Both endpoints must be supplied by
        // the room layout, so RoomLayout.Fill needs the full per-room picture before it
        // commits to a chunk arrangement.
        var doorwayDemand = ComputeDoorwayDemand(graph);

        foreach (var node in graph.Rooms)
        {
            var room = (node.Features & RoomFeatures.Entrance) != 0
                ? GetEmptyEntranceRoom(gameLevel, manager)
                : CreateEmptyRoom(gameLevel, manager);
            room.GridX = node.GridX;
            room.GridY = node.GridY;
            room.Features = node.Features;
            node.Room = room;

            // Per-room fragment selection. Throws if no fragment matches the room's
            // RoomFeatures — a data-set bug.
            var roomFragment = LevelGenerator.PickFragmentForRoom(
                gameLevel, node.Features, stats);
            var roomParameters = roomFragment.GetRoomParameters(branch);

            AllocateTerrain(room, roomParameters.RoomWidth, roomParameters.RoomHeight,
                roomParameters.DefaultTerrain);

            var demand = doorwayDemand.TryGetValue(node, out var d)
                ? d
                : [];
            var chunks = branch.RoomLayout.Fill(room, branch, roomFragment, demand);
            if (chunks is null)
            {
                // Abort and let the outer retry try a different seed.
                return null;
            }

            allChunks.AddRange(chunks);
            chunksByRoom[room.EntityId] = chunks;

            roomParameters.CreatureGenerator.Fill(room,
                chunks.Where(c => c.Fragment is not ConnectingMapFragment).ToList());
            roomParameters.ItemGenerator.Fill(room, chunks);
        }

        // Materialise edges. Bidirectional pairs are present as two separate edges (A→B and
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

        return allChunks;
    }

    /// <summary>
    ///     Returns the per-room doorway demand: how many distinct doorway tiles each room
    ///     needs to expose in each <see cref="Direction" />, based on the directed edges
    ///     in <paramref name="graph" />. Every directed edge <c>(source, target)</c>
    ///     demands one doorway on the <c>source</c> side facing the target and one on the
    ///     <c>target</c> side facing the source; bidirectional pairs are present as two
    ///     separate edges so the count naturally doubles where both endpoints need two
    ///     doorways on the same side.
    /// </summary>
    private static Dictionary<RoomGraphNode, Dictionary<Direction, int>> ComputeDoorwayDemand(RoomGraph graph)
    {
        var demand = new Dictionary<RoomGraphNode, Dictionary<Direction, int>>(graph.Rooms.Count);
        foreach (var edge in graph.Connections)
        {
            var dx = edge.Target.GridX - edge.Source.GridX;
            var dy = edge.Target.GridY - edge.Source.GridY;
            AddDemand(demand, edge.Source, GridDeltaToDirection(dx, dy));
            AddDemand(demand, edge.Target, GridDeltaToDirection(-dx, -dy));
        }
        return demand;
    }

    private static void AddDemand(
        Dictionary<RoomGraphNode, Dictionary<Direction, int>> demand,
        RoomGraphNode node,
        Direction direction)
    {
        if (!demand.TryGetValue(node, out var perDir))
        {
            perDir = [];
            demand[node] = perDir;
        }
        perDir[direction] = perDir.GetValueOrDefault(direction) + 1;
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

        // Pick the doorway tile most aligned with the direction across all chunks of this
        // room. The picker is permissive about side — it just returns the best-aligned
        // unused doorway, no matter which chunk it belongs to. This mirrors what
        // <see cref="RoomLayout.SatisfiesDemand" /> assumes: total unused doorway
        // count is what matters, not per-chunk distribution.
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
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDoorway = p;
                }
            }
        }

        if (bestDoorway is { } doorway)
        {
            return doorway;
        }

        throw new InvalidOperationException(
            $"No usable doorway tile found in room {room.EntityId} facing {direction}. "
            + "Ensure the room's fragments provide reachable DoorwayPoints.");
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
    ///     Examines the four cardinal neighbours: if exactly one is walkable the player
    ///     approaches from that side, so the orientation is the opposite direction (facing
    ///     into the wall). When the surroundings are ambiguous (0 or 2+ walkable
    ///     neighbours) the direction from room centre to door is used as a tiebreaker.
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
    ///     <see cref="LevelConnection.CreateSourceConnection" /> from a neighbouring level
    ///     that linked to this level before it existed, that placeholder is reused so its
    ///     <see cref="RoomComponent.IncomingConnections" /> (the neighbour's outbound
    ///     <see cref="ConnectionComponent" />s) end up pointing at the real entrance room
    ///     rather than at an orphaned zero-width room.
    /// </summary>
    public static RoomComponent GetEmptyEntranceRoom(GameLevel level, GameManager manager)
    {
        foreach (var existing in level.Rooms)
        {
            // Placeholder rooms have no terrain (Width == 0) but at least one incoming
            // connection from a neighbouring level; reuse the first one we find.
            if (existing.Width == 0 && existing.IncomingConnections.Count > 0)
            {
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

    /// <summary>
    ///     Returns rented terrain arrays back to <see cref="ArrayPool{T}.Shared" /> and replaces them
    ///     with <see cref="Array.Empty{T}" /> placeholders. Safe to call on a partially or never
    ///     initialised <see cref="RoomComponent" /> — freshly-constructed components already
    ///     hold empty placeholders, so the rent-return helpers see length 0 and skip the pool return.
    /// </summary>
    public static void ReleaseTerrain(RoomComponent room)
    {
        ReturnTerrainIfRented(room.Terrain);
        ReturnTerrainIfRented(room.Neighbors);
        ReturnTerrainIfRented(room.KnownNeighbors);
        ReturnTerrainIfRented(room.VisibleTerrain);
        ReturnTerrainIfRented(room.KnownTerrain);

        room.Terrain = Array.Empty<MapFeature>();
        room.Neighbors = Array.Empty<DirectionFlags>();
        room.KnownNeighbors = Array.Empty<DirectionFlags>();
        room.VisibleTerrain = Array.Empty<byte>();
        room.KnownTerrain = Array.Empty<MapFeature>();
        room.Width = 0;
        room.Height = 0;
    }

    private static void ReturnTerrainIfRented(byte[] array)
    {
        // Array.Empty<T>() instances have length 0 and must not be returned to the pool.
        if (array.Length > 0)
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    private static void ReturnTerrainIfRented(MapFeature[] array)
    {
        if (array.Length > 0)
        {
            ArrayPool<MapFeature>.Shared.Return(array);
        }
    }

    private static void ReturnTerrainIfRented(DirectionFlags[] array)
    {
        if (array.Length > 0)
        {
            ArrayPool<DirectionFlags>.Shared.Return(array);
        }
    }
}
