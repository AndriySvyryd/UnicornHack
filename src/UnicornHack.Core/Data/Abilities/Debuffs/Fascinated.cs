namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Fascinated = new LeveledAbility
    {
        Name = "fascinated",
        Type = AbilityType.Debuff,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "-25", PropertyName = "MentalStatusResistance" } }
            },
            {
                2,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "100", PropertyName = "UnableToActPercent" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-25", PropertyName = "MentalStatusResistance" }
                }
            }
        }
    };
}
