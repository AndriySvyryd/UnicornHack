namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability DoubleMeleeAttack = new Ability
    {
        Name = "double melee attack",
        Type = AbilityType.DefaultMeleeAttack,
        Effects = new List<Effect> { new Activate { WeaponSlot = EquipmentSlot.GraspPrimaryMelee }, new Activate { WeaponSlot = EquipmentSlot.GraspSecondaryMelee } }
    };
}
