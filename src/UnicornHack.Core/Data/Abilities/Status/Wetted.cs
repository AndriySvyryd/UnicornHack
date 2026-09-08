namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Wetted = new LeveledAbility
    {
        Name = "wetted",
        Type = AbilityType.Debuff,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "30", PropertyName = "FireResistance" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-30", PropertyName = "ElectricityResistance" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-30", PropertyName = "ColdResistance" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "burning" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "chilled" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "corroded" }
                }
            },
            {
                2,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "30", PropertyName = "FireResistance" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-30", PropertyName = "ElectricityResistance" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-30", PropertyName = "ColdResistance" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "20", PropertyName = "Hindrance" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "burning" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "chilled" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "corroded" }
                }
            }
        }
    };
}
