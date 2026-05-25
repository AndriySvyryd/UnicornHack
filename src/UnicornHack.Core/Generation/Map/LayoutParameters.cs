namespace UnicornHack.Generation.Map;

/// <summary>
///     Per-level snapshot of layout parameters consumed by <see cref="LevelLayout.BuildGraph" />.
///     Returned by <see cref="LevelLayout.GetParameters" />; subclasses override that method to
///     supply depth-dependent (or otherwise dynamic) values without mutating instance state.
/// </summary>
/// <param name="GridWidth">Logical grid extent in columns.</param>
/// <param name="GridHeight">Logical grid extent in rows.</param>
/// <param name="Interconnectedness">Edge-density target after pruning, in [0, 1].</param>
/// <param name="Density">Probability that an optional candidate cell is occupied during placement.</param>
/// <param name="Sinuosity">
///     How winding the entrance-to-exit path should be, in <c>[0, 1]</c>.
///     <c>0</c> keeps the shortest possible path; <c>1</c> produces the longest simple
///     (non-revisiting) path on the undirected adjacency graph.
/// </param>
/// <param name="RewardRoomProbability">Probability that a non-entrance/non-exit room is tagged with a reward feature.</param>
/// <param name="CurseRoomProbability">Probability that a non-entrance/non-exit room is tagged Cursed.</param>
/// <param name="HiddenConnectionProbability">Probability that an Optional (non-required) edge starts hidden.</param>
/// <param name="MinRooms">Minimum total room count; below this the level is regenerated.</param>
/// <param name="MaxRooms">Maximum total room count; the placement algorithm caps at this value.</param>
/// <param name="Difficulty">
///     Effective level difficulty (<see cref="GameLevel.Difficulty" />).
///     Drives difficulty-scaled content choices such as the deep-tier reward gating in
///     <see cref="LevelLayout.GetRewardFeatureWeight" />.
/// </param>
public readonly record struct LayoutParameters(
    byte GridWidth,
    byte GridHeight,
    float Interconnectedness,
    float Density,
    float Sinuosity,
    float RewardRoomProbability,
    float CurseRoomProbability,
    float HiddenConnectionProbability,
    byte MinRooms,
    byte MaxRooms,
    int Difficulty);
