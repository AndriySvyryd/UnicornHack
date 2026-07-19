namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item MailArmor = new Item
    {
        Name = "mail armor",
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "mail armor ability #1",
                Activation = ActivationType.WhileEquipped,
                Effects = new List<Effect> { new ChangeProperty<int> { Amount = "5", PropertyName = "Armor" } }
            }
        },
        Type = ItemType.ArmorTorso,
        Material = Material.Steel,
        Weight = 40,
        EquipableSizes = SizeCategory.Small | SizeCategory.Medium | SizeCategory.Large,
        EquipableSlots = EquipmentSlot.Torso
    };
}
