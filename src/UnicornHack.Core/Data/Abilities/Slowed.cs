namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Slowed = new LeveledAbility
    {
        Name = "slowed",
        Type = AbilityType.Status,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-2", PropertyName = "Speed" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "100", PropertyName = "CooldownFactor" }
                }
            },
            {
                2,
                new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "-2", PropertyName = "Speed" } }
            },
            {
                3,
                new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "-2", PropertyName = "Speed" } }
            }
        }
    };
}
