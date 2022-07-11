namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly WieldingAbility TwoHandedRangedAttack = new WieldingAbility
    {
        Name = "two-handed ranged attack",
        Type = AbilityType.DefaultAttack,
        Effects = new List<Effect> { new Activate() },
        WieldingStyle = WieldingStyle.TwoHanded,
        ItemType = ItemType.WeaponRanged
    };
}
