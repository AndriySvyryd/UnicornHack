namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Canceled = new LeveledAbility
    {
        Name = "canceled",
        Type = AbilityType.Debuff,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new Effect[] { new DecreaseAbilityLevel { TargetActivator = true, AbilityType = AbilityType.Buff } }
            },
            {
                2,
                new Effect[] { new DecreaseAbilityLevel { TargetActivator = true, AbilityType = AbilityType.Buff } }
            },
            {
                3,
                new Effect[] { new DecreaseAbilityLevel { TargetActivator = true, AbilityType = AbilityType.Buff } }
            }
        }
    };
}
