namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Bound = new LeveledAbility
    {
        Name = "bound",
        Type = AbilityType.Debuff,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect> { new ChangeProperty<bool> { TargetActivator = true, Amount = "0", PropertyName = "CanMove" } }
            },
            {
                2,
                new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "-25", PropertyName = "Evasion" } }
            },
            {
                3,
                new List<Effect> { new ChangeProperty<bool> { TargetActivator = true, Amount = "0", PropertyName = "CanUseDefaultAttacks" } }
            }
        }
    };
}
