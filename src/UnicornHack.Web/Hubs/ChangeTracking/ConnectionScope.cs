namespace UnicornHack.Hubs.ChangeTracking;

/// <summary>
///     Classification of where a <see cref="UnicornHack.Systems.Levels.ConnectionComponent" />
///     leads, relative to the source room: same level, a different level on the same branch,
///     or a different branch entirely. The client uses this to decide how to react when a
///     <see cref="TurnEventType.SteppedOnConnection" /> event arrives.
/// </summary>
public enum ConnectionScope
{
    Room = 0,
    Level = 1,
    Branch = 2
}
