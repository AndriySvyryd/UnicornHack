namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item FieryAegis = new Item
    {
        Name = "fiery aegis",
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.WhileEquipped,
                Effects = new List<Effect> { new ChangeProperty<int> { PropertyName = "Deflection", Value = 10 }, new ChangeProperty<int> { PropertyName = "Armor", Value = 2 } }
            },
            new Ability
            {
                Name = "Activate",
                Activation = ActivationType.WhileToggled,
                ItemCondition = ActivationType.WhileEquipped,
                Cooldown = 200,
                Delay = "50",
                EnergyPointCost = 20,
                Effects = new List<Effect> { new ChangeProperty<int> { Duration = EffectDuration.Infinite, PropertyName = "FireResistance", Value = 30 } }
            }
        },
        Type = ItemType.Shield,
        Material = Material.Steel,
        Weight = 30,
        EquipableSizes = SizeCategory.Medium | SizeCategory.Large,
        EquipableSlots = EquipmentSlot.GraspMelee
    };
}
