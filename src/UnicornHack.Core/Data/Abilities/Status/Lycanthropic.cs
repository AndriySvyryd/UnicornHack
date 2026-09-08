namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Lycanthropic = new LeveledAbility
    {
        Name = "lycanthropic",
        Type = AbilityType.Debuff,
        Cumulative = false,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect>
                {
                    new ConferLycanthropy
                    {
                        TargetActivator = true,
                        Duration = EffectDuration.Infinite,
                        CombinationFunction = ValueCombinationFunction.BuildUpPerSource,
                        Amount = "21",
                        CreatureName = "<propagated>"
                    }
                }
            },
            {
                2,
                new List<Effect> { new Polymorph { TargetActivator = true, DurationAmount = "2000", CreatureName = "<propagated>" } }
            }
        }
    };
}
