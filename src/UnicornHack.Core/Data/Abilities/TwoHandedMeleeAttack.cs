namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability TwoHandedMeleeAttack = new Ability
    {
        Name = "two-handed melee attack",
        Type = AbilityType.DefaultMeleeAttack,
        Effects = new List<Effect> { new Activate { WeaponSlot = EquipmentSlot.GraspBothMelee } }
    };
}
