namespace UnicornHack.Generation;

/// <summary>
///     Test-only helpers over <see cref="RoomGraph" />. Lives in the test assembly because
///     it's purely a validation convenience with no production callers (relocated from
///     <c>LevelGenerator</c>).
/// </summary>
internal static class RoomGraphTestExtensions
{
    /// <summary>
    ///     Verifies that every room is reachable from the entrance and that an exit is reachable
    ///     from every room, following directed edges.
    /// </summary>
    public static bool HasValidConnectivity(this RoomGraph graph)
    {
        var entrances = graph.Rooms.Where(r => (r.Features & RoomFeatures.Entrance) != 0).ToList();
        var exits = graph.Rooms.Where(r => (r.Features & RoomFeatures.Exit) != 0).ToList();
        if (entrances.Count == 0 || exits.Count == 0)
        {
            return false;
        }

        var forward = graph.Connections
            .GroupBy(e => e.Source)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Target).ToList());

        if (BreadthFirstReach(entrances, forward).Count != graph.Rooms.Count)
        {
            return false;
        }

        // To check "an exit is reachable from every room", reverse the edges and BFS from every exit.
        var backward = graph.Connections
            .GroupBy(e => e.Target)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Source).ToList());
        return BreadthFirstReach(exits, backward).Count == graph.Rooms.Count;
    }

    private static HashSet<RoomGraphNode> BreadthFirstReach(
        IEnumerable<RoomGraphNode> sources,
        Dictionary<RoomGraphNode, List<RoomGraphNode>> adjacency)
    {
        var visited = new HashSet<RoomGraphNode>();
        var queue = new Queue<RoomGraphNode>();
        foreach (var source in sources)
        {
            if (visited.Add(source))
            {
                queue.Enqueue(source);
            }
        }

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            if (!adjacency.TryGetValue(node, out var neighbors))
            {
                continue;
            }

            foreach (var neighbor in neighbors)
            {
                if (visited.Add(neighbor))
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        return visited;
    }
}
