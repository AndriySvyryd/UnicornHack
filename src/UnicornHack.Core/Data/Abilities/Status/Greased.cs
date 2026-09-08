namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Greased = new LeveledAbility
    {
        Name = "greased",
        Type = AbilityType.Buff,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "25", PropertyName = "Quietness" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "bound" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "engulfed" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "stuck" }
                }
            },
            {
                2,
                new List<Effect>
                {
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "bound" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "engulfed" },
                    new DecreaseAbilityLevel { TargetActivator = true, AbilityName = "stuck" }
                }
            }
        }
    };
}
