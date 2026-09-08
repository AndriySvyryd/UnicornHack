namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Taunted = new LeveledAbility
    {
        Name = "taunted",
        Type = AbilityType.Debuff,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect>()
            },
            {
                2,
                new List<Effect>()
            },
            {
                3,
                new List<Effect> { new ChangeProperty<bool> { TargetActivator = true, Amount = "0", PropertyName = "CanUseNonAttackAbilities" } }
            }
        }
    };
}
