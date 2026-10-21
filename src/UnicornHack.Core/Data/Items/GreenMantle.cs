namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item GreenMantle = new Item
    {
        Name = "green mantle",
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "green mantle protection",
                Activation = ActivationType.WhileEquipped,
                Effects = new List<Effect> { new ChangeProperty<int> { Amount = "20", PropertyName = "ColdResistance" } }
            },
            new Ability
            {
                Name = "Wrap",
                Activation = ActivationType.Manual,
                XPCooldown = 200,
                Delay = "100",
                Effects = new List<Effect>
                {
                    new DecreaseAbilityLevel { TargetActivator = true, Duration = EffectDuration.UntilTimeout, DurationAmount = "300", Amount = "2", AbilityName = "burning" },
                    new ChangeProperty<int> { TargetActivator = true, Duration = EffectDuration.UntilTimeout, DurationAmount = "300", Amount = "10", PropertyName = "Hindrance" }
                }
            }
        },
        Type = ItemType.AccessoryBack,
        GenerationWeight = "$branch == 'ruined stronghold' ? 0.5 : 0",
        Material = Material.Cloth,
        Weight = 8,
        EquipableSizes = SizeCategory.Small | SizeCategory.Medium | SizeCategory.Large,
        EquipableSlots = EquipmentSlot.Back
    };
}
