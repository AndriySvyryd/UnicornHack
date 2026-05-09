using UnicornHack.Utils;

namespace UnicornHack.Generation;

/// <summary>
///     Test-only helpers for exercising the graph algorithm directly without spinning up a
///     <see cref="GameLevel" />.
/// </summary>
public static class LevelLayoutTestExtensions
{
    /// <summary>
    ///     Snapshots <paramref name="layout" />'s instance properties into a
    ///     <see cref="LayoutParameters" /> and invokes <see cref="LevelLayout.BuildGraph" />.
    ///     Production code goes through <see cref="LevelLayout.GetParameters" /> instead.
    /// </summary>
    public static RoomGraph? GenerateRoomGraph(this GridLevelLayout layout, SimpleRandom random)
    {
        var parameters = new LayoutParameters(
            layout.GridWidth, layout.GridHeight, layout.Interconnectedness, layout.ComputeDensity(),
            layout.Sinuosity, layout.RewardRoomProbability, layout.CurseRoomProbability,
            layout.HiddenConnectionProbability, layout.MinRooms, layout.MaxRooms);
        return layout.BuildGraph(parameters, random);
    }
}
