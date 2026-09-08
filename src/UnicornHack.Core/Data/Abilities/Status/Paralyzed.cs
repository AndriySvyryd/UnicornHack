namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Paralyzed = new LeveledAbility
    {
        Name = "paralyzed",
        Type = AbilityType.Debuff,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-25", PropertyName = "PhysicalStatusResistance" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "slowed" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "dazed" }
                }
            },
            {
                2,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "100", PropertyName = "UnableToActPercent" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "slowed" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "dazed" }
                }
            }
        }
    };
}
