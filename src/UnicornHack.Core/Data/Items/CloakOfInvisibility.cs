namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item CloakOfInvisibility = new Item
    {
        Name = "cloak of invisibility",
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "cloak of invisibility ability #1",
                Activation = ActivationType.WhileEquipped,
                EnergyPointCost = 50,
                Effects = new List<Effect> { new ChangeProperty<int> { CombinationFunction = ValueCombinationFunction.Override, Amount = "0", PropertyName = "Visibility" } }
            }
        },
        Type = ItemType.AccessoryBack,
        Material = Material.Cloth,
        Weight = 5,
        EquipableSizes = SizeCategory.Small | SizeCategory.Medium | SizeCategory.Large,
        EquipableSlots = EquipmentSlot.Back
    };
}
