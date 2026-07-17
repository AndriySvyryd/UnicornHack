namespace UnicornHack.Generation.Map;

/// <summary>
///     A single perimeter anchoring directive: the <see cref="Side" /> (a cardinal or ordinal
///     <see cref="Direction" />) a connection chunk should be steered onto, and whether that side
///     carries a real doorway demand (<see cref="IsDoorwayDemand" /> = <c>true</c>: an inter-room
///     edge that must yield a doorway tile) or is only a positioning anchor
///     (<see cref="IsDoorwayDemand" /> = <c>false</c>: e.g. the side opposite the entrance's outgoing
///     connection). Ordinal sides are collapsed to their constituent cardinals only when a lot's
///     edge contact is tested.
/// </summary>
/// <remarks>
///     A room's demand is an ordered list of these anchors that the room layout steers its first
///     chunks onto so connection-hosting chunks land on the room perimeter rather than the central
///     defining chunk. Anchors are consumed in order: incoming connections first (on their facing
///     side), then outgoing connections (on their own facing side). A room with no incoming edge
///     (the entrance) leads with positioning-only anchors opposite its outgoing connections so the
///     outgoing chunks can take the outgoing side.
/// </remarks>
public readonly record struct RoomDoorwayAnchor(Direction Side, bool IsDoorwayDemand);
