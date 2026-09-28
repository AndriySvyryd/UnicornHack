namespace UnicornHack.Primitives;

/// <summary>
///     Behavioral capabilities and authored AI intent. Except for <see cref="Roaming" />,
///     these values are currently data markers and are not yet interpreted by
///     <see cref="Systems.Actors.AISystem" />.
/// </summary>
[Flags]
public enum AIBehavior
{
    /// <summary>No additional authored behavior.</summary>
    None = 0,

    /// <summary>Intended to remain non-hostile unless provoked.</summary>
    Peaceful = 1 << 0,

    /// <summary>Intended to remain non-hostile while actors keep distance from it.</summary>
    RangedPeaceful = 1 << 1,

    /// <summary>Prevents the creature from being recruited as a follower.</summary>
    NonRecruitable = 1 << 2,

    /// <summary>Intended to allow another actor to use this creature as a mount.</summary>
    Mountable = 1 << 3,

    /// <summary>Intended to move between idle destinations instead of remaining in place.</summary>
    Wandering = 1 << 4,

    /// <summary>Intended to pursue a target while using concealment or favorable distance.</summary>
    Stalking = 1 << 5,

    /// <summary>Intended to move through occupied cells by displacing their occupants.</summary>
    Displacing = 1 << 6,

    /// <summary>Intended to select and use magical abilities.</summary>
    MagicUser = 1 << 7,

    /// <summary>
    ///     Relocates the creature to a connected unvisited room after player room travel,
    ///     unless the player entered its current room.
    /// </summary>
    Roaming = 1 << 8
}
