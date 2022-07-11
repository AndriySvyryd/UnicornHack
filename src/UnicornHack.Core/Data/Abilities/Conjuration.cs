namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Conjuration = new LeveledAbility
    {
        Name = "conjuration",
        Type = AbilityType.Skill,
        Cost = 4,
        Activation = ActivationType.Always,
        Cumulative = true,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>> { { 1, new List<Effect> { new AddAbility { Name = "ice shard" } } } }
    };
}
