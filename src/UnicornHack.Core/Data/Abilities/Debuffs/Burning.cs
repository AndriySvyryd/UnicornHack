namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Burning = new LeveledAbility
    {
        Name = "burning",
        Type = AbilityType.Debuff,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "50", PropertyName = "Visibility" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "wetted" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "chilled" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "greased" }
                }
            },
            {
                2,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "50", PropertyName = "Visibility" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-25", PropertyName = "FireResistance" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "wetted" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "chilled" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "greased" }
                }
            },
            {
                3,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "50", PropertyName = "Visibility" },
                    new ChangeProperty<bool> { TargetActivator = true, Amount = "1", PropertyName = "EmitsSmoke" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "wetted" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "chilled" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "greased" }
                }
            }
        }
    };
}
