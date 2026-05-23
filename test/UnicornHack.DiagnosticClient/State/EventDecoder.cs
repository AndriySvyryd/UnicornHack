using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.DiagnosticClient.State;

/// <summary>
///     Translates the typeless wire representation of <see cref="TurnChangeSet.Events" />
///     entries — each event is serialized by the server as a fixed-length MessagePack
///     array whose <c>[0]</c> slot is the <see cref="TurnEventType" /> discriminator —
///     back into the strongly-typed DTOs declared under <c>UnicornHack.Hubs.ChangeTracking</c>.
///     <para>
///         The shared DTOs are visible to the diagnostic client via the <c>ProjectReference</c>
///         to <c>UnicornHack.Web</c>; the conversion happens here rather than via a
///         typed MessagePack formatter so the wire format stays identical to what the
///         Unity client (which still does typeless dispatch in <c>ApplyEvents</c>)
///         expects.
///     </para>
/// </summary>
internal static class EventDecoder
{
    public static object? TryDecode(object? raw)
    {
        if (raw is not System.Collections.IList list || list.Count < 1)
        {
            return null;
        }

        var type = (TurnEventType)Convert.ToInt32(list[0]);
        return type switch
        {
            TurnEventType.SteppedOnConnection when list.Count >= 3 => new SteppedOnConnectionEvent
            {
                Type = TurnEventType.SteppedOnConnection,
                Scope = (ConnectionScope)Convert.ToInt32(list[1]),
                ConnectionId = Convert.ToInt32(list[2])
            },
            _ => null
        };
    }
}
