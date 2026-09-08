namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Muted = new LeveledAbility
    {
        Name = "muted",
        Type = AbilityType.Debuff,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect>()
            },
            {
                2,
                new List<Effect>
                {
                    new ChangeProperty<bool> { TargetActivator = true, Amount = "0", PropertyName = "CanCastSpells" },
                    new ChangeProperty<bool> { TargetActivator = true, Amount = "0", PropertyName = "CanUseComplexFollowerCommands" }
                }
            },
            {
                3,
                new List<Effect>
                {
                    new ChangeProperty<bool> { TargetActivator = true, Amount = "0", PropertyName = "CanUseVerbalAbilities" },
                    new ChangeProperty<bool> { TargetActivator = true, Amount = "0", PropertyName = "CanUseDivineAbilities" },
                    new ChangeProperty<bool> { TargetActivator = true, Amount = "0", PropertyName = "CanUseFollowerCommands" }
                }
            }
        }
    };
}
