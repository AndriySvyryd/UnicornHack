using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

/// <summary>
///     Transport event emitted when the player actor steps onto a
///     <see cref="UnicornHack.Systems.Levels.ConnectionComponent" />. Carried on
///     <see cref="TurnChangeSet.Events" /> so the client can react (e.g. open a room-map
///     dialog for intra-level connections; level / branch transitions are handled by the
///     existing inter-level path).
/// </summary>
/// <remarks>
///     <see cref="Type" /> is fixed to <see cref="TurnEventType.SteppedOnConnection" /> and
///     occupies <c>[Key(0)]</c> so client-side dispatch can read the discriminator at a
///     stable position regardless of which concrete event DTO arrives.
/// </remarks>
[MessagePackObject]
public class SteppedOnConnectionEvent
{
    [Key(0)]
    public TurnEventType Type { get; set; } = TurnEventType.SteppedOnConnection;

    [Key(1)]
    public ConnectionScope Scope { get; set; }

    [Key(2)]
    public int ConnectionId { get; set; }
}
