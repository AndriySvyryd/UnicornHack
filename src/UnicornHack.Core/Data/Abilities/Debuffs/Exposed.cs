namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Exposed = new LeveledAbility
    {
        Name = "exposed",
        Type = AbilityType.Debuff,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "-25", PropertyName = "PhysicalStatusResistance" } }
            },
            {
                2,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, CombinationFunction = ValueCombinationFunction.Percent, Amount = "50", PropertyName = "Evasion" }
                }
            },
            {
                3,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, CombinationFunction = ValueCombinationFunction.Override, Amount = "0", PropertyName = "Evasion" }
                }
            }
        }
    };
}
