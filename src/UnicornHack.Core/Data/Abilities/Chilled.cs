namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Chilled = new LeveledAbility
    {
        Name = "chilled",
        Type = AbilityType.Status,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-2", PropertyName = "Speed" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-25", PropertyName = "ColdResistance" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "25", PropertyName = "FireResistance" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "burning" }
                }
            },
            {
                2,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-2", PropertyName = "Speed" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-25", PropertyName = "ColdResistance" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "25", PropertyName = "FireResistance" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "burning" }
                }
            },
            {
                3,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-2", PropertyName = "Speed" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-25", PropertyName = "ColdResistance" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "25", PropertyName = "FireResistance" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "30", PropertyName = "Hindrance" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "burning" }
                }
            }
        }
    };
}
