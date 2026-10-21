namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Gestation = new LeveledAbility
    {
        Name = "gestation",
        Type = AbilityType.Innate,
        Cumulative = false,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "-3*$might", PropertyName = "HitPointMaximum" } }
            },
            {
                2,
                new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "-2*$might", PropertyName = "HitPointMaximum" } }
            },
            {
                3,
                new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "-$might", PropertyName = "HitPointMaximum" } }
            },
            {
                4,
                new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "0", PropertyName = "HitPointMaximum" } }
            }
        }
    };
}
