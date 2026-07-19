namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability OneHandedMeleeAttack = new Ability
    {
        Name = "one handed melee attack",
        Type = AbilityType.DefaultMeleeAttack,
        Effects = new List<Effect> { new Activate { WeaponSlot = EquipmentSlot.GraspSingleMelee } }
    };
}
