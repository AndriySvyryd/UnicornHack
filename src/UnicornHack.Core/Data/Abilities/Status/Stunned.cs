namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Stunned = new LeveledAbility
    {
        Name = "stunned",
        Type = AbilityType.Debuff,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "25", PropertyName = "UnableToActPercent" } }
            },
            {
                2,
                new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "25", PropertyName = "UnableToActPercent" } }
            },
            {
                3,
                new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "50", PropertyName = "UnableToActPercent" } }
            }
        }
    };
}
