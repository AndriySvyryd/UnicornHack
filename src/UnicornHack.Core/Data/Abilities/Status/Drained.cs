namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Drained = new LeveledAbility
    {
        Name = "drained",
        Type = AbilityType.Debuff,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, CombinationFunction = ValueCombinationFunction.Override, Amount = "0", PropertyName = "Regeneration" },
                    new ChangeProperty<int>
                    {
                        TargetActivator = true,
                        CombinationFunction = ValueCombinationFunction.Override,
                        Amount = "0",
                        PropertyName = "EnergyRegeneration"
                    },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "blighted" }
                }
            },
            {
                2,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "100", PropertyName = "CooldownFactor" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "blighted" }
                }
            }
        }
    };
}
