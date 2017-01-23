new Item
{
    Name = "long sword",
    Type = ItemType.WeaponMeleeMedium,
    Weight = 10,
    Material = Material.Steel,
    EquipableSizes = Size.Medium | Size.Large,
    EquipableSlots = EquipmentSlot.GraspSingleExtremity | EquipmentSlot.GraspBothExtremities,
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
