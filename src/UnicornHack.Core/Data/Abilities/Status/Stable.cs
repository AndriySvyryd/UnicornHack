namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Stable = new LeveledAbility
    {
        Name = "stable",
        Type = AbilityType.Buff,
        Cumulative = false,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect> { new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "unstable" } }
            },
            {
                2,
                new List<Effect> { new DecreaseAbilityLevel { TargetActivator = true, Amount = "2", AbilityName = "unstable" } }
            }
        }
    };
}
