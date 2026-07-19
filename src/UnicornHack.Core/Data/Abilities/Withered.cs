namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Withered = new LeveledAbility
    {
        Name = "withered",
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
                        CombinationFunction = ValueCombinationFunction.Override,
                        Amount = "0",
                        PropertyName = "EnergyRegeneration"
                    },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "overcharged" }
                }
            },
            {
                2,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "50", PropertyName = "EnergyRestorationResistance" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "overcharged" }
                }
            },
            {
                3,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "50", PropertyName = "EnergyRestorationResistance" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "overcharged" }
                }
            }
        }
    };
}
