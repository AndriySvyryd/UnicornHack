namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability OneHandedRangedAttack = new Ability
    {
        Name = "one handed ranged attack",
        Type = AbilityType.DefaultRangedAttack,
        Effects = new List<Effect> { new Activate { WeaponSlot = EquipmentSlot.GraspSingleRanged } }
    };
}
