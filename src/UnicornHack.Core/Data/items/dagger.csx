new Item
{
    Name = "dagger",
    Type = ItemType.WeaponMeleeShort,
    Weight = 5,
    Material = Material.Steel,
    EquipableSizes = Size.Tiny | Size.Small,
    EquipableSlots = EquipmentSlot.GraspSingleExtremity | EquipmentSlot.GraspBothExtremities,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Slash,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 10 } }
        }
    }
}
