namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Confused = new LeveledAbility
    {
        Name = "confused",
        Type = AbilityType.Status,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect>
                {
                    new ChangeProperty<bool> { TargetActivator = true, Amount = "0", PropertyName = "CanPerceiveIntention" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-25", PropertyName = "MentalStatusResistance" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "frightened" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "taunted" }
                }
            },
            {
                2,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "50", PropertyName = "RandomMovementPercent" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "50", PropertyName = "RandomActionPercent" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "frightened" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "taunted" }
                }
            },
            {
                3,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "50", PropertyName = "RandomMovementPercent" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "50", PropertyName = "RandomActionPercent" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "frightened" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "taunted" }
                }
            }
        }
    };
}
