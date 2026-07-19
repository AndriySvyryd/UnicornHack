namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Frightened = new LeveledAbility
    {
        Name = "frightened",
        Type = AbilityType.Status,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>> { { 1, new List<Effect>() }, { 2, new List<Effect>() }, { 3, new List<Effect>() } }
    };
}
