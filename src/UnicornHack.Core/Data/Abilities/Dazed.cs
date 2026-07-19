namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Dazed = new LeveledAbility
    {
        Name = "dazed",
        Type = AbilityType.Status,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-3", PropertyName = "Perception" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-3", PropertyName = "Speed" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-25", PropertyName = "Evasion" }
                }
            },
            {
                2,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-2", PropertyName = "Perception" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-2", PropertyName = "Speed" },
                    new ChangeProperty<bool> { TargetActivator = true, Amount = "0", PropertyName = "CanUseComplexAbilities" }
                }
            },
            {
                3,
                new List<Effect> { new ChangeProperty<bool> { TargetActivator = true, Amount = "0", PropertyName = "CanUseNonDefaultAbilities" } }
            }
        }
    };
}
