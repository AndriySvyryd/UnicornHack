namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility BloodSourcery = new LeveledAbility
    {
        Name = "blood sourcery",
        Type = AbilityType.Skill,
        Cost = 4,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            { 1, new List<Effect>() }, { 2, new List<Effect>() }, { 3, new List<Effect>() }
        }
    };
}
