namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly LeveledAbility Corroded = new LeveledAbility
    {
        Name = "corroded",
        Type = AbilityType.Status,
        LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                1,
                new List<Effect> { new ChangeProperty<int> { TargetActivator = true, Amount = "-2", PropertyName = "Armor" } }
            },
            {
                2,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-2", PropertyName = "Armor" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-25", PropertyName = "Deflection" }
                }
            },
            {
                3,
                new List<Effect>
                {
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-2", PropertyName = "Armor" },
                    new ChangeProperty<int> { TargetActivator = true, Amount = "-25", PropertyName = "PhysicalResistance" }
                }
            }
        }
    };
}
