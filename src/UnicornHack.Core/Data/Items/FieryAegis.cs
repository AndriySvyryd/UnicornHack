namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item FieryAegis = new Item
    {
        Name = "fiery aegis",
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "fiery aegis ability #1",
                Activation = ActivationType.WhileEquipped,
                Effects = new List<Effect> { new ChangeProperty<int> { Amount = "10", PropertyName = "Deflection" }, new ChangeProperty<int> { Amount = "2", PropertyName = "Armor" } }
            },
            new Ability
            {
                Name = "Activate",
                Activation = ActivationType.WhileToggled,
                Cooldown = 200,
                Delay = "50",
                EnergyPointCost = 20,
                Effects = new List<Effect> { new ChangeProperty<int> { Amount = "30", PropertyName = "FireResistance" } }
            }
        },
        Type = ItemType.Shield,
        Material = Material.Steel,
        Weight = 30,
        EquipableSizes = SizeCategory.Medium | SizeCategory.Large,
        EquipableSlots = EquipmentSlot.GraspMelee
    };
}
