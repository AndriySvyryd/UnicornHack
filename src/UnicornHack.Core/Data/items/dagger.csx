new Item
{
    Name = "dagger",
    Type = ItemType.Weapon,
    Weight = 5,
    Material = Material.Steel,
    Size = Size.Small,
    EquipableSizes = Size.Tiny | Size.Small,
    EquipableSlots = EquipmentSlot.GraspSingleExtremity | EquipmentSlot.GraspBothExtremities,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Slash,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 3 } }
        }
    }
}
