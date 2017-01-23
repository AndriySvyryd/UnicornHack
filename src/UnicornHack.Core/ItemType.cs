using System;

namespace UnicornHack
{
    [Flags]
    public enum ItemType
    {
        None = 0,
        WeaponMeleeShort = 1 << 10,
        WeaponMeleeMedium = 1 << 11,
        WeaponMeleeLong = 1 << 12,
        WeaponRangedThrown = 1 << 13,
        WeaponRangedSlingshots = 1 << 14,
        WeaponRangedBows = 1 << 15,
        WeaponRangedCrossbows = 1 << 16,
        Armor = 1 << 1,
        Accessory = 1 << 2,
        Container = 1 << 3,
        Food = 1 << 4,
        Potion = 1 << 5,
        Scroll = 1 << 6,
        SpellBook = 1 << 7,
        Coin = 1 << 8,
        Trinket = 1 << 9,
        Rare = 1 << 17,
        Exotic = 1 << 18
    }
}