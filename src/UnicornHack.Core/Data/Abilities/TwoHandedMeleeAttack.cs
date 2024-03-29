namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly WieldingAbility TwoHandedMeleeAttack = new WieldingAbility
    {
        Name = "two-handed melee attack",
        Type = AbilityType.DefaultAttack,
        Effects = new List<Effect> { new Activate() },
        WieldingStyle = WieldingStyle.TwoHanded,
        ItemType = ItemType.WeaponMelee
    };
}
