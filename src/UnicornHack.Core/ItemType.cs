using System;

namespace UnicornHack
{
    [Flags]
    public enum ItemType
    {
        None = 0,
        Weapon = 1 << 0,
        Armor = 1 << 1,
        Accessory = 1 << 2,
        Container = 1 << 3,
        Food = 1 << 4,
        Potion = 1 << 5,
        Scroll = 1 << 6,
        SpellBook = 1 << 7,
        Coin = 1 << 8,
        Trinket = 1 << 9
    }
}