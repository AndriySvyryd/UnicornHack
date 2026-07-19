namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Blinded = new LeveledAbility
    {
        Name = "blinded",
        Type = AbilityType.Status,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect>
                {
                    new ChangeProperty<int>
                    {
                        TargetActivator = true,
                        CombinationFunction = ValueCombinationFunction.Percent,
                        Amount = "50",
                        PropertyName = "PrimaryVisionRange"
                    },
                    new ChangeProperty<int>
                    {
                        TargetActivator = true,
                        CombinationFunction = ValueCombinationFunction.Percent,
                        Amount = "50",
                        PropertyName = "SecondaryVisionRange"
                    }
                }
            },
            {
                2,
                new List<Effect>
                {
                    new ChangeProperty<int>
                    {
                        TargetActivator = true,
                        CombinationFunction = ValueCombinationFunction.Override,
                        Amount = "0",
                        PropertyName = "PrimaryVisionRange"
                    },
                    new ChangeProperty<int>
                    {
                        TargetActivator = true,
                        CombinationFunction = ValueCombinationFunction.Override,
                        Amount = "0",
                        PropertyName = "SecondaryVisionRange"
                    }
                }
            },
            {
                3,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-50", PropertyName = "Accuracy" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-25", PropertyName = "Evasion" }
                }
            }
        }
    };
}
