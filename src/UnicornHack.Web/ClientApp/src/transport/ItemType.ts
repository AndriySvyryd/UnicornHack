export enum ItemType {
    None = 0,
    Coin = 1 << 0,
    Container = 1 << 1,
    WeaponMeleeHand = 1 << 2,
    WeaponMeleeShort = 1 << 3,
    WeaponMeleeMedium = 1 << 4,
    WeaponMeleeLong = 1 << 5,
    WeaponRangedClose = 1 << 6,
    WeaponRangedShort = 1 << 7,
    WeaponRangedMedium = 1 << 8,
    WeaponRangedLong = 1 << 9,
    WeaponAmmoContainer = 1 << 10,
    WeaponProjectile = 1 << 11,
    Shield = 1 << 14,
    ArmorTorso = 1 << 15,
    ArmorHead = 1 << 16,
    ArmorFeet = 1 << 17,
    ArmorHands = 1 << 18,
    LightArmor = 1 << 19,
    HeavyArmor = 1 << 20,
    AccessoryBack = 1 << 21,
    AccessoryNeck = 1 << 22,
    SkillBook = 1 << 23,
    Potion = 1 << 24,
    Rune = 1 << 25,
    Orb = 1 << 26,
    Figurine = 1 << 27,
    Trinket = 1 << 28,

    WeaponMelee = WeaponMeleeHand | WeaponMeleeShort | WeaponMeleeMedium | WeaponMeleeLong,

    WeaponRanged = WeaponRangedClose | WeaponRangedShort | WeaponRangedMedium | WeaponRangedLong | WeaponProjectile,

    Weapon = WeaponMelee | WeaponRanged,

    Armor = Shield | ArmorTorso | ArmorHead | ArmorFeet | ArmorHands
}