namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Blighted = new LeveledAbility
    {
        Name = "blighted",
        Type = AbilityType.Debuff,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, CombinationFunction = ValueCombinationFunction.Override, Amount = "0", PropertyName = "Regeneration" }
                }
            },
            {
                2,
                new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "50", PropertyName = "HealingResistance" } }
            },
            {
                3,
                new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "50", PropertyName = "HealingResistance" } }
            }
        }
    };
}
