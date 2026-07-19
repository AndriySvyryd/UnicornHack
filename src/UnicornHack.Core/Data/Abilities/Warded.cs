namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Warded = new LeveledAbility
    {
        Name = "warded",
        Type = AbilityType.Status,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new Effect[] { new DecreaseAbilityLevel { TargetActivator = true, AbilityType = AbilityType.Status } }
            },
            {
                2,
                new Effect[] { new DecreaseAbilityLevel { TargetActivator = true, AbilityType = AbilityType.Status } }
            },
            {
                3,
                new Effect[] { new DecreaseAbilityLevel { TargetActivator = true, AbilityType = AbilityType.Status } }
            }
        }
    };
}
