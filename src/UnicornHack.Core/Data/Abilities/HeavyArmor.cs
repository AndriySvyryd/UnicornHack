namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility HeavyArmor = new LeveledAbility
    {
        Name = "heavy armor",
        Type = AbilityType.Skill,
        Cost = 4,
        Activation = ActivationType.Always,
        Cumulative = true,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>> { { 1, new List<Effect>() } }
    };
}
