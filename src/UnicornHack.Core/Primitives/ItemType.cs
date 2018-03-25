using System;

namespace UnicornHack.Primitives
{
    [Flags]
    public enum ItemType
    {
        None = 0,
        Coin = 1 << 0,
        Container = 1 << 1,
        WeaponMeleeFist = 1 << 2,
        WeaponMeleeShort = 1 << 3,
        WeaponMeleeMedium = 1 << 4,
        WeaponMeleeLong = 1 << 5,
        WeaponMagicFocus = 1 << 6,
        WeaponMagicStaff = 1 << 7,
        WeaponRangedThrown = 1 << 8,
        WeaponRangedSlingshot = 1 << 9,
        WeaponRangedBow = 1 << 10,
        WeaponRangedCrossbow = 1 << 11,
        WeaponAmmoContainer = 1 << 12,
        WeaponProjectile = 1 << 13,
        Shield = 1 << 14,
        ArmorTorso = 1 << 15,
        ArmorHead = 1 << 16,
        ArmorFeet = 1 << 17,
        ArmorHands = 1 << 18,
        AccessoryBack = 1 << 19,
        AccessoryNeck = 1 << 20,
        SkillBook = 1 << 21,
        Potion = 1 << 22,
        Rune = 1 << 23,
        Wand = 1 << 24,
        Figurine = 1 << 25,
        Trinket = 1 << 26,

        WeaponRanged = WeaponRangedThrown | WeaponRangedSlingshot | WeaponRangedBow | WeaponRangedCrossbow
                       | WeaponMagicStaff | WeaponProjectile,

        WeaponMelee = WeaponMeleeFist | WeaponMeleeShort | WeaponMeleeMedium | WeaponMeleeLong
                      | WeaponMagicFocus,

        Weapon = WeaponMelee | WeaponRanged
    }
}
