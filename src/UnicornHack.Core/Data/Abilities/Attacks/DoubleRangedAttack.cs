namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability DoubleRangedAttack = new Ability
    {
        Name = "double ranged attack",
        Type = AbilityType.DefaultRangedAttack,
        Effects = new List<Effect> { new Activate { WeaponSlot = EquipmentSlot.GraspPrimaryRanged }, new Activate { WeaponSlot = EquipmentSlot.GraspSecondaryRanged } }
    };
}
