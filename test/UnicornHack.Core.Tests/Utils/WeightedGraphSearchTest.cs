namespace UnicornHack.Utils;

public class WeightedGraphSearchTest
{
    [Fact]
    public void Stops_when_known_targets_are_settled_or_exceed_their_budgets()
    {
        var graph = new LineGraph(nodeCount: 100);
        int[] targets = [2, 99];
        int[] maximumDistances = [100, 3];
        var distances = new int[targets.Length];

        WeightedGraphSearch.GetShortestDistances(
            graph, source: 0, targets, maximumDistances, distances);

        Assert.Equal([2, int.MaxValue], distances);
        Assert.Equal(3, graph.FarthestExpandedNode);

        graph.Reset();
        WeightedGraphSearch.GetShortestDistancesAStar(
            graph, source: 0, targets, maximumDistances, distances);

        Assert.Equal([2, int.MaxValue], distances);
        Assert.Equal(1, graph.FarthestExpandedNode);
    }

    [Fact]
    public void Single_target_search_returns_the_weighted_shortest_distance()
    {
        var graph = new LineGraph(nodeCount: 5, distance: 10);

        Assert.Equal(
            30,
            WeightedGraphSearch.GetShortestDistance(
                graph, source: 0, target: 3, maximumDistance: 30));
        Assert.Equal(
            int.MaxValue,
            WeightedGraphSearch.GetShortestDistance(
                graph, source: 0, target: 3, maximumDistance: 29));
        Assert.Equal(
            30,
            WeightedGraphSearch.GetShortestDistanceAStar(
                graph, source: 0, target: 3, maximumDistance: 30));
        Assert.Equal(
            int.MaxValue,
            WeightedGraphSearch.GetShortestDistanceAStar(
                graph, source: 0, target: 3, maximumDistance: 29));
    }

    [Fact]
    public void A_star_reconstructs_the_path_from_target_to_source()
    {
        var graph = new LineGraph(nodeCount: 5, distance: 10);
        var path = new List<int>();

        Assert.True(WeightedGraphSearch.TryGetShortestPathAStar(
            graph, source: 0, target: 3, maximumDistance: 30,
            path, out var distance));
        Assert.Equal(30, distance);
        Assert.Equal([3, 2, 1, 0], path);

        Assert.False(WeightedGraphSearch.TryGetShortestPathAStar(
            graph, source: 0, target: 3, maximumDistance: 29,
            path, out distance));
        Assert.Equal(int.MaxValue, distance);
        Assert.Empty(path);
    }

    private sealed class LineGraph(int nodeCount, int distance = 1) : IWeightedGraph
    {
        public int FarthestExpandedNode { get; private set; } = -1;

        public int NodeCount { get; } = nodeCount;
        public int NeighborCount => 2;

        public void Reset() => FarthestExpandedNode = -1;

        public bool TryGetNeighbor(
            int node, int neighborIndex, out int neighbor, out int neighborDistance)
        {
            FarthestExpandedNode = Math.Max(FarthestExpandedNode, node);
            neighbor = neighborIndex == 0 ? node - 1 : node + 1;
            neighborDistance = distance;
            return (uint)neighbor < NodeCount;
        }

        public int GetEstimatedDistance(int node, int target)
            => Math.Abs(target - node) * distance;
    }
}