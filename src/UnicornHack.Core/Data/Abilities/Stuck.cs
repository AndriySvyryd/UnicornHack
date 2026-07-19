namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Stuck = new LeveledAbility
    {
        Name = "stuck",
        Type = AbilityType.Status,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect> { new ChangeProperty<bool> { TargetActivator = true, Amount = "0", PropertyName = "CanMoveItems" } }
            },
            {
                2,
                new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "-25", PropertyName = "Evasion" } }
            },
            {
                3,
                new List<Effect> { new ChangeProperty<bool> { TargetActivator = true, Amount = "0", PropertyName = "CanMove" } }
            }
        }
    };
}
