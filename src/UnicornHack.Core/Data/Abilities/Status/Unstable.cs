namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Unstable = new LeveledAbility
    {
        Name = "unstable",
        Type = AbilityType.Debuff,
        Cumulative = false,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, CombinationFunction = ValueCombinationFunction.Percent, Amount = "75", PropertyName = "Accuracy" },
                    new ChangeProperty<int> { TargetActivator = true, CombinationFunction = ValueCombinationFunction.Percent, Amount = "75", PropertyName = "Evasion" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "bound" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "stuck" }
                }
            },
            {
                2,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, CombinationFunction = ValueCombinationFunction.Percent, Amount = "150", PropertyName = "MovementDelay" },
                    new ChangeProperty<int> { TargetActivator = true, CombinationFunction = ValueCombinationFunction.Percent, Amount = "150", PropertyName = "TurningDelay" },
                    new ChangeProperty<int> { TargetActivator = true, CombinationFunction = ValueCombinationFunction.Percent, Amount = "50", PropertyName = "Accuracy" },
                    new ChangeProperty<int> { TargetActivator = true, CombinationFunction = ValueCombinationFunction.Percent, Amount = "50", PropertyName = "Evasion" },
                    new DecreaseAbilityLevel { TargetActivator = true, Amount = "2", AbilityName = "bound" },
                    new DecreaseAbilityLevel { TargetActivator = true, Amount = "2", AbilityName = "stuck" }
                }
            }
        }
    };
}
