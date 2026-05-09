namespace UnicornHack.Generation.Map;

/// <summary>
///     Per-room snapshot of the effective values produced by combining a
///     <see cref="DefiningMapFragment" /> with the level's <see cref="Branch" /> defaults.
///     Returned by <see cref="DefiningMapFragment.GetRoomParameters" /> so callers don't have
///     to invoke five separate <c>GetEffective*</c> methods.
/// </summary>
public readonly record struct RoomParameters(
    byte RoomWidth,
    byte RoomHeight,
    MapFeature DefaultTerrain,
    CreatureGenerator CreatureGenerator,
    ItemGenerator ItemGenerator);
