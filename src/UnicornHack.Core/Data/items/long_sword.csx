new Item
{
    Name = "long sword",
    Type = ItemType.Weapon,
    Weight = 10,
    Material = Material.Steel,
    EquipableSlots = new HashSet<EquipmentSlot> { EquipmentSlot.GraspMainExtremity, EquipmentSlot.GraspSecondaryExtremity, EquipmentSlot.GraspBothExtremities },
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Slash,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 5 } }
        }
    }
}
