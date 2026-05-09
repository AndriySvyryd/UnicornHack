using UnicornHack.Utils;

namespace UnicornHack;

/// <summary>
///     Game-wide settings that affect gameplay and are persisted with the game state.
/// </summary>
public class GameOptions : NotificationEntity
{
    private uint _initialSeed;
    private bool _revealLevels;
    private bool _revealRooms;
    private bool _disableSessionCache;

    public uint InitialSeed
    {
        get => _initialSeed;
        set => SetWithNotify(value, ref _initialSeed);
    }

    /// <summary>
    ///     When <c>true</c>, all terrain tiles on the player's current level are fully
    ///     visible — including tiles in unvisited rooms — and hidden connections are
    ///     revealed.
    /// </summary>
    public bool RevealLevels
    {
        get => _revealLevels;
        set => SetWithNotify(value, ref _revealLevels);
    }

    /// <summary>
    ///     When <c>true</c>, all terrain tiles in the player's current room are fully
    ///     visible regardless of line-of-sight.
    /// </summary>
    public bool RevealRooms
    {
        get => _revealRooms;
        set => SetWithNotify(value, ref _revealRooms);
    }

    /// <summary>
    ///     Diagnostic switch that bypasses <c>GameSession</c> caching so every hub call
    ///     reloads from the database. Honored only when the host environment is
    ///     <c>Development</c>. Wire via configuration key <c>DisableSessionCache</c>
    ///     (CLI: <c>--DisableSessionCache=true</c>). Not persisted with the game state.
    /// </summary>
    public bool DisableSessionCache
    {
        get => _disableSessionCache;
        set => SetWithNotify(value, ref _disableSessionCache);
    }
}
