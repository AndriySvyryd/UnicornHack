namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item GoldClasp = new Item
    {
        Name = "gold clasp",
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "Set the Foot",
                Activation = ActivationType.Manual,
                Delay = "100",
                Effects = new List<Effect>
                {
                    new DecreaseAbilityLevel { TargetActivator = true, Duration = EffectDuration.UntilTimeout, DurationAmount = "200", AbilityName = "unstable" },
                    new AddAbility { TargetActivator = true, Duration = EffectDuration.UntilTimeout, DurationAmount = "200", Name = "stable" },
                    new ChangeProperty<int> { TargetActivator = true, Duration = EffectDuration.UntilTimeout, DurationAmount = "200", Amount = "10", PropertyName = "Hindrance" }
                }
            }
        },
        Type = ItemType.Trinket,
        GenerationWeight = "$branch == 'ruined stronghold' ? 0.5 : 0",
        Material = Material.Gold,
        Weight = 2,
        EquipableSizes = SizeCategory.Small | SizeCategory.Medium | SizeCategory.Large,
        EquipableSlots = EquipmentSlot.Feet
    };
}
