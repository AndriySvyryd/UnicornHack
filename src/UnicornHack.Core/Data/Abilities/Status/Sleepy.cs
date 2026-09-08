namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Sleepy = new LeveledAbility
    {
        Name = "sleepy",
        Type = AbilityType.Debuff,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "-3", PropertyName = "Perception" } }
            },
            {
                2,
                new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "100", PropertyName = "UnableToActPercent" } }
            }
        }
    };
}
