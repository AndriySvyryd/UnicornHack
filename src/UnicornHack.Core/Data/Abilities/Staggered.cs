namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Staggered = new LeveledAbility
    {
        Name = "staggered",
        Type = AbilityType.Status,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "25", PropertyName = "Hindrance" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "50", PropertyName = "RandomMoveAfterActionPercent" }
                }
            },
            {
                2,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "25", PropertyName = "Hindrance" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "50", PropertyName = "RandomMoveAfterActionPercent" }
                }
            }
        }
    };
}
