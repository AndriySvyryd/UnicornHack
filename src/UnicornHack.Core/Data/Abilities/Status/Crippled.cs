namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Crippled = new LeveledAbility
    {
        Name = "crippled",
        Type = AbilityType.Debuff,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "25", PropertyName = "Hindrance" } }
            },
            {
                2,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "25", PropertyName = "Hindrance" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-25", PropertyName = "Evasion" }
                }
            }
        }
    };
}
