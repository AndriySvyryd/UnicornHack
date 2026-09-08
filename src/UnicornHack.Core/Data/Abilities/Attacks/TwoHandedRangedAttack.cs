namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability TwoHandedRangedAttack = new Ability
    {
        Name = "two-handed ranged attack",
        Type = AbilityType.DefaultRangedAttack,
        Effects = new List<Effect> { new Activate { WeaponSlot = EquipmentSlot.GraspBothRanged } }
    };
}
