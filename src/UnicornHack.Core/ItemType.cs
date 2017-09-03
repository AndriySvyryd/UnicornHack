using System;

namespace UnicornHack
{
    [Flags]
    public enum ItemType
    {
        None = 0,
        Coin = 1 << 0,
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
        Shield = 1 << 13,
        ArmorBody = 1 << 14,
        ArmorHead = 1 << 15,
        ArmorFeet = 1 << 16,
        ArmorHands = 1 << 17,
        ArmorBack = 1 << 18,
        Accessory = 1 << 19,
        Container = 1 << 20,
        Potion = 1 << 21,
        Wand = 1 << 22,
        Figurine = 1 << 23,
        Trinket = 1 << 25,
        SkillBook = 1 << 26,
        Intricate = 1 << 30,
        Exotic = 1 << 31,

        WeaponRanged = WeaponRangedThrown | WeaponRangedSlingshot | WeaponRangedBow | WeaponRangedCrossbow
                       | WeaponMagicStaff,

        WeaponMelee = WeaponMeleeFist | WeaponMeleeShort | WeaponMeleeMedium | WeaponMeleeLong
                      | WeaponMagicFocus,

        Wepon = WeaponMelee | WeaponRanged
    }
}