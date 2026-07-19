namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Deafened = new LeveledAbility
    {
        Name = "deafened",
        Type = AbilityType.Status,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, CombinationFunction = ValueCombinationFunction.Percent, Amount = "50", PropertyName = "HearingLevel" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "50", PropertyName = "FollowerCommandFailurePercent" }
                }
            },
            {
                2,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, CombinationFunction = ValueCombinationFunction.Override, Amount = "0", PropertyName = "HearingLevel" }
                }
            }
        }
    };
}
