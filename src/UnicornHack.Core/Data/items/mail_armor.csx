new Item
{
    Name = "mail armor",
    Type = ItemType.Armor,
    Weight = 40,
    Material = Material.Steel,
    EquipableSlots = new HashSet<EquipmentSlot> { EquipmentSlot.Body },
    ValuedProperties = new Dictionary<string, Object> { { "ArmorClass", 10 } }
}
