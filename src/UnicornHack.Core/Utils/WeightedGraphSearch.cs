using System.Buffers;

namespace UnicornHack.Utils;

public interface IWeightedGraph
{
    int NodeCount { get; }
    int NeighborCount { get; }

    bool TryGetNeighbor(int node, int neighborIndex, out int neighbor, out int distance);

    /// <summary>Returns a nonnegative admissible estimate of the distance from one node to another.</summary>
    int GetEstimatedDistance(int node, int target);
}

public static class WeightedGraphSearch
{
    private const int PendingDistance = -1;
    private static long _nextSearchId;

    public static int GetShortestDistance<TGraph>(
        TGraph graph, int source, int target, int maximumDistance)
        where TGraph : IWeightedGraph
    {
        Span<int> targets = [target];
        Span<int> maximumDistances = [maximumDistance];
        Span<int> shortestDistances = stackalloc int[1];
        GetShortestDistances(
            graph, source, targets, maximumDistances, shortestDistances);
        return shortestDistances[0];
    }

    public static int GetShortestDistanceAStar<TGraph>(
        TGraph graph, int source, int target, int maximumDistance)
        where TGraph : IWeightedGraph
    {
        Span<int> targets = [target];
        Span<int> maximumDistances = [maximumDistance];
        Span<int> shortestDistances = stackalloc int[1];
        GetShortestDistancesAStar(
            graph, source, targets, maximumDistances, shortestDistances);
        return shortestDistances[0];
    }

    /// <summary>
    ///     Finds one shortest path and writes it from target back to source, including both endpoints.
    /// </summary>
    public static bool TryGetShortestPathAStar<TGraph>(
        TGraph graph,
        int source,
        int target,
        int maximumDistance,
        List<int> path,
        out int distance)
        where TGraph : IWeightedGraph
    {
        ValidateArguments(
            graph, source, [target], [maximumDistance], shortestDistances: stackalloc int[1]);
        path.Clear();
        var nodeStates = ArrayPool<PathNodeState>.Shared.Rent(graph.NodeCount);
        try
        {
            var open = new PriorityQueue<SearchNode, long>();
            return TryGetShortestPathAStar(
                graph, source, target, maximumDistance,
                nodeStates, open, path, out distance);
        }
        finally
        {
            ArrayPool<PathNodeState>.Shared.Return(nodeStates);
        }
    }

    internal static bool TryGetShortestPathAStar<TGraph>(
        TGraph graph,
        int source,
        int target,
        int maximumDistance,
        PathWorkspace workspace,
        List<int> path,
        out int distance)
        where TGraph : IWeightedGraph
    {
        ValidateArguments(
            graph, source, [target], [maximumDistance], shortestDistances: stackalloc int[1]);
        if (workspace.NodeStates.Length < graph.NodeCount)
        {
            throw new ArgumentException("The workspace is smaller than the graph.", nameof(workspace));
        }

        path.Clear();
        workspace.Open.Clear();
        return TryGetShortestPathAStar(
            graph, source, target, maximumDistance,
            workspace.NodeStates, workspace.Open, path, out distance);
    }

    private static bool TryGetShortestPathAStar<TGraph>(
        TGraph graph,
        int source,
        int target,
        int maximumDistance,
        PathNodeState[] nodeStates,
        PriorityQueue<SearchNode, long> open,
        List<int> path,
        out int distance)
        where TGraph : IWeightedGraph
    {
        var searchId = Interlocked.Increment(ref _nextSearchId);
        var sourceEstimate = graph.GetEstimatedDistance(source, target);
        Debug.Assert(sourceEstimate >= 0);
        if (sourceEstimate > maximumDistance)
        {
            distance = int.MaxValue;
            return false;
        }

        var sourcePriority = GetAStarPriority(distance: 0, sourceEstimate);
        nodeStates[source] = new PathNodeState(
            searchId, distance: 0, sourcePriority, parent: -1);
        open.Enqueue(new SearchNode(source, distance: 0), sourcePriority);

        while (open.TryDequeue(out var currentNode, out var currentPriority))
        {
            ref var currentState = ref nodeStates[currentNode.Node];
            if (currentState.SearchId != searchId
                || currentState.Distance != currentNode.Distance
                || currentState.Priority != currentPriority)
            {
                continue;
            }

            if (currentNode.Node == target)
            {
                distance = currentNode.Distance;
                ReconstructPath(source, target, graph.NodeCount, nodeStates, path);
                return true;
            }

            for (var neighborIndex = 0; neighborIndex < graph.NeighborCount; neighborIndex++)
            {
                if (!graph.TryGetNeighbor(
                        currentNode.Node, neighborIndex, out var neighbor, out var edgeDistance))
                {
                    continue;
                }

                Debug.Assert(edgeDistance >= 0);
                var nextDistance = (long)currentNode.Distance + edgeDistance;
                if (nextDistance > maximumDistance)
                {
                    continue;
                }

                var estimate = graph.GetEstimatedDistance(neighbor, target);
                Debug.Assert(estimate >= 0);
                if (nextDistance + estimate > maximumDistance)
                {
                    continue;
                }

                ref var neighborState = ref nodeStates[neighbor];
                if (neighborState.SearchId == searchId
                    && neighborState.Distance <= nextDistance)
                {
                    continue;
                }

                var nextPriority = GetAStarPriority((int)nextDistance, estimate);
                neighborState = new PathNodeState(
                    searchId, (int)nextDistance, nextPriority, currentNode.Node);
                open.Enqueue(new SearchNode(neighbor, (int)nextDistance), nextPriority);
            }
        }

        distance = int.MaxValue;
        return false;
    }

    private static void ReconstructPath(
        int source,
        int target,
        int nodeCount,
        PathNodeState[] nodeStates,
        List<int> path)
    {
        var current = target;
        var safetyBudget = nodeCount + 1;
        while (true)
        {
            if (--safetyBudget < 0)
            {
                throw new InvalidOperationException("The shortest-path parent chain contains a cycle.");
            }

            path.Add(current);
            if (current == source)
            {
                return;
            }

            current = nodeStates[current].Parent;
            if (current < 0)
            {
                throw new InvalidOperationException("The shortest-path parent chain does not reach the source.");
            }
        }
    }

    /// <summary>
    ///     Finds shortest distances from one source to a small known set of targets. A target is discarded
    ///     once Dijkstra's settled distance exceeds its maximum, and the search stops when every target is
    ///     either settled or discarded. Unreachable targets are returned as <see cref="int.MaxValue" />.
    /// </summary>
    public static void GetShortestDistances<TGraph>(
        TGraph graph,
        int source,
        ReadOnlySpan<int> targets,
        ReadOnlySpan<int> maximumDistances,
        Span<int> shortestDistances)
        where TGraph : IWeightedGraph
        => GetShortestDistancesDijkstra(
            graph, source, targets, maximumDistances, shortestDistances);

    public static void GetShortestDistancesAStar<TGraph>(
        TGraph graph,
        int source,
        ReadOnlySpan<int> targets,
        ReadOnlySpan<int> maximumDistances,
        Span<int> shortestDistances)
        where TGraph : IWeightedGraph
    {
        ValidateArguments(
            graph, source, targets, maximumDistances, shortestDistances);
        shortestDistances = shortestDistances[..targets.Length];
        if (targets.Length == 0)
        {
            return;
        }

        var nodeStates = ArrayPool<NodeState>.Shared.Rent(graph.NodeCount);
        var open = new PriorityQueue<SearchNode, long>();
        try
        {
            for (var i = 0; i < targets.Length; i++)
            {
                shortestDistances[i] = GetShortestDistanceAStar(
                    graph, source, targets[i], maximumDistances[i], nodeStates, open);
            }
        }
        finally
        {
            ArrayPool<NodeState>.Shared.Return(nodeStates);
        }
    }

    private static int GetShortestDistanceAStar<TGraph>(
        TGraph graph,
        int source,
        int target,
        int maximumDistance,
        NodeState[] nodeStates,
        PriorityQueue<SearchNode, long> open)
        where TGraph : IWeightedGraph
    {
        var sourceEstimate = graph.GetEstimatedDistance(source, target);
        Debug.Assert(sourceEstimate >= 0);
        if (sourceEstimate > maximumDistance)
        {
            return int.MaxValue;
        }

        var searchId = Interlocked.Increment(ref _nextSearchId);
        open.Clear();
        var sourcePriority = GetAStarPriority(distance: 0, sourceEstimate);
        nodeStates[source] = new NodeState(searchId, distance: 0, sourcePriority);
        open.Enqueue(new SearchNode(source, distance: 0), sourcePriority);

        while (open.TryDequeue(out var currentNode, out var currentPriority))
        {
            ref var currentState = ref nodeStates[currentNode.Node];
            if (currentState.SearchId != searchId
                || currentState.Distance != currentNode.Distance
                || currentState.Priority != currentPriority)
            {
                continue;
            }

            if (currentNode.Node == target)
            {
                return currentNode.Distance;
            }

            for (var neighborIndex = 0; neighborIndex < graph.NeighborCount; neighborIndex++)
            {
                if (!graph.TryGetNeighbor(
                        currentNode.Node, neighborIndex, out var neighbor, out var edgeDistance))
                {
                    continue;
                }

                Debug.Assert(edgeDistance >= 0);
                var nextDistance = (long)currentNode.Distance + edgeDistance;
                if (nextDistance > maximumDistance)
                {
                    continue;
                }

                var estimate = graph.GetEstimatedDistance(neighbor, target);
                Debug.Assert(estimate >= 0);
                if (nextDistance + estimate > maximumDistance)
                {
                    continue;
                }

                ref var neighborState = ref nodeStates[neighbor];
                if (neighborState.SearchId == searchId
                    && neighborState.Distance <= nextDistance)
                {
                    continue;
                }

                var nextPriority = GetAStarPriority((int)nextDistance, estimate);
                neighborState = new NodeState(searchId, (int)nextDistance, nextPriority);
                open.Enqueue(new SearchNode(neighbor, (int)nextDistance), nextPriority);
            }
        }

        return int.MaxValue;
    }

    private static long GetAStarPriority(int distance, int estimate)
        => ((long)(distance + estimate) << 32) - distance;

    private static void GetShortestDistancesDijkstra<TGraph>(
        TGraph graph,
        int source,
        ReadOnlySpan<int> targets,
        ReadOnlySpan<int> maximumDistances,
        Span<int> shortestDistances)
        where TGraph : IWeightedGraph
    {
        ValidateArguments(
            graph, source, targets, maximumDistances, shortestDistances);

        shortestDistances = shortestDistances[..targets.Length];
        shortestDistances.Fill(PendingDistance);
        if (targets.Length == 0)
        {
            return;
        }

        var searchId = Interlocked.Increment(ref _nextSearchId);
        var nodeStates = ArrayPool<NodeState>.Shared.Rent(graph.NodeCount);
        try
        {
            var open = new PriorityQueue<SearchNode, long>();
            nodeStates[source] = new NodeState(searchId, distance: 0, priority: 0);
            open.Enqueue(new SearchNode(source, distance: 0), priority: 0);

            var remainingTargets = targets.Length;

            while (open.TryDequeue(out var currentNode, out var currentPriority))
            {
                var current = currentNode.Node;
                var currentDistance = currentNode.Distance;
                ref var currentState = ref nodeStates[current];
                if (currentState.SearchId != searchId
                    || currentState.Distance != currentDistance
                    || currentState.Priority != currentPriority)
                {
                    continue;
                }

                var maximumPendingDistance = -1;
                for (var i = 0; i < targets.Length; i++)
                {
                    if (shortestDistances[i] != PendingDistance)
                    {
                        continue;
                    }

                    if (currentDistance > maximumDistances[i])
                    {
                        shortestDistances[i] = int.MaxValue;
                        remainingTargets--;
                    }
                    else if (current == targets[i])
                    {
                        shortestDistances[i] = currentDistance;
                        remainingTargets--;
                    }
                    else
                    {
                        maximumPendingDistance = Math.Max(maximumPendingDistance, maximumDistances[i]);
                    }
                }

                if (remainingTargets == 0)
                {
                    break;
                }

                for (var neighborIndex = 0; neighborIndex < graph.NeighborCount; neighborIndex++)
                {
                    if (!graph.TryGetNeighbor(
                            current, neighborIndex, out var neighbor, out var distance))
                    {
                        continue;
                    }

                    Debug.Assert(distance >= 0);
                    var nextDistance = (long)currentDistance + distance;
                    if (nextDistance > int.MaxValue
                        || nextDistance > maximumPendingDistance)
                    {
                        continue;
                    }

                    var nextPriority = nextDistance;

                    ref var neighborState = ref nodeStates[neighbor];
                    if (neighborState.SearchId == searchId
                        && neighborState.Distance <= nextDistance)
                    {
                        continue;
                    }

                    neighborState = new NodeState(searchId, (int)nextDistance, nextPriority);
                    open.Enqueue(new SearchNode(neighbor, (int)nextDistance), nextPriority);
                }
            }

            for (var i = 0; i < shortestDistances.Length; i++)
            {
                if (shortestDistances[i] == PendingDistance)
                {
                    shortestDistances[i] = int.MaxValue;
                }
            }
        }
        finally
        {
            ArrayPool<NodeState>.Shared.Return(nodeStates);
        }
    }

    private static void ValidateArguments<TGraph>(
        TGraph graph,
        int source,
        ReadOnlySpan<int> targets,
        ReadOnlySpan<int> maximumDistances,
        Span<int> shortestDistances)
        where TGraph : IWeightedGraph
    {
        if ((uint)source >= graph.NodeCount)
        {
            throw new ArgumentOutOfRangeException(nameof(source));
        }

        if (maximumDistances.Length != targets.Length)
        {
            throw new ArgumentException("A maximum distance is required for every target.", nameof(maximumDistances));
        }

        if (shortestDistances.Length < targets.Length)
        {
            throw new ArgumentException("The result span is shorter than the target span.", nameof(shortestDistances));
        }

        for (var i = 0; i < targets.Length; i++)
        {
            if ((uint)targets[i] >= graph.NodeCount)
            {
                throw new ArgumentOutOfRangeException(nameof(targets));
            }
        }
    }

    internal readonly struct SearchNode(int node, int distance)
    {
        public int Node { get; } = node;
        public int Distance { get; } = distance;
    }

    private readonly struct NodeState(long searchId, int distance, long priority)
    {
        public long SearchId { get; } = searchId;
        public int Distance { get; } = distance;
        public long Priority { get; } = priority;
    }

    internal readonly struct PathNodeState(long searchId, int distance, long priority, int parent)
    {
        public long SearchId { get; } = searchId;
        public int Distance { get; } = distance;
        public long Priority { get; } = priority;
        public int Parent { get; } = parent;
    }

    internal sealed class PathWorkspace(int nodeCount)
    {
        internal PathNodeState[] NodeStates { get; } = new PathNodeState[nodeCount];
        internal PriorityQueue<SearchNode, long> Open { get; } = new();
        internal List<int> Path { get; } = new();
    }
}