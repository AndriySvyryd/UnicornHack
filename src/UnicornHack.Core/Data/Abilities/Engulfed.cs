namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Engulfed = new LeveledAbility
    {
        Name = "engulfed",
        Type = AbilityType.Status,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect>
                {
                    new ChangeProperty<bool> { TargetActivator = true, Amount = "0", PropertyName = "CanMove" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "25", PropertyName = "Hindrance" }
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
                new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "-25", PropertyName = "Evasion" } }
            }
        }
    };
}
