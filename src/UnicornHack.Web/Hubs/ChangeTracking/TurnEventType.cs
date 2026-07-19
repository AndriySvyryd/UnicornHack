namespace UnicornHack.Hubs.ChangeTracking;

/// <summary>
///     Discriminator tag for entries in <see cref="TurnChangeSet.Events" />. Server-side
///     each event is a strongly-typed <see cref="TurnEvent" />
///     whose <c>[Key(0)]</c> is one of these values.
/// </summary>
public enum TurnEventType
{
    SteppedOnConnection = 0
}
