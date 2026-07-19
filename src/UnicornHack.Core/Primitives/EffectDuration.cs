namespace UnicornHack.Primitives;

public enum EffectDuration
{
    /// <summary>The effect lasts until its containing ability is deactivated or removed.</summary>
    Infinite = -1,

    /// <summary>The effect is applied once when its ability activates.</summary>
    Instant = 0,

    /// <summary>The effect expires after its resolved duration amount in game ticks.</summary>
    UntilTimeout = 1,

    /// <summary>The effect expires after the target gains its resolved amount of experience.</summary>
    UntilXPGained = 2,

    /// <summary>The effect lasts only while the containing ability application is being processed.</summary>
    DuringApplication = 3,

    /// <summary>The effect expires when the target enters a different room.</summary>
    UntilNextRoom = 4,

    /// <summary>The effect expires when the target enters a room on a different level.</summary>
    UntilNextLevel = 5
}
