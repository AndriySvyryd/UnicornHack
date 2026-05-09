namespace UnicornHack.Generation.Map;

/// <summary>
///     Abstract level structure used during generation: a directed graph of rooms placed on a
///     grid, with cardinal-style edges connecting them. Materialised into
///     <see cref="Systems.Levels.RoomComponent" /> entities and
///     <see cref="Systems.Levels.ConnectionComponent" /> entities by the level layout.
/// </summary>
public sealed class RoomGraph
{
    public List<RoomGraphNode> Rooms { get; } = [];
    public List<RoomGraphEdge> Connections { get; } = [];
}

/// <summary>
///     A single node in a <see cref="RoomGraph" />. Carries grid coordinates and intended
///     <see cref="RoomFeatures" />. The materialised <see cref="Systems.Levels.RoomComponent" />
///     is attached after creation.
/// </summary>
public sealed class RoomGraphNode
{
    public required byte GridX { get; init; }

    public required byte GridY { get; init; }

    public RoomFeatures Features { get; set; } = RoomFeatures.Normal;

    /// <summary>
    ///     Materialised room. Assigned by the per-room generation pipeline once the graph is
    ///     finalised; null during graph construction.
    /// </summary>
    public Systems.Levels.RoomComponent? Room { get; set; }
}

/// <summary>
///     A directed edge in a <see cref="RoomGraph" />. Bidirectional connections are represented
///     as two paired edges (A→B and B→A) with mirrored <see cref="Hidden" />.
/// </summary>
public sealed class RoomGraphEdge
{
    public required RoomGraphNode Source { get; init; }

    public required RoomGraphNode Target { get; init; }

    /// <summary>
    ///     A hidden edge isn't shown on the map until revealed (e.g. by entering either
    ///     endpoint). For bidirectional pairs both halves carry the same <see cref="Hidden" />
    ///     value and are revealed simultaneously.
    /// </summary>
    public bool Hidden { get; set; }
}
