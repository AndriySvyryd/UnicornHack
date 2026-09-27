namespace UnicornHack.Primitives;

[Flags]
public enum RoomFeatures
{
    Normal = 0,
    Entrance = 1 << 0,
    Exit = 1 << 1,
    Secret = 1 << 2,
    Shortcut = 1 << 3,
    Cursed = 1 << 4,
    Challenge = 1 << 5,
    Boss = 1 << 6,
    Trapped = 1 << 7,
    DeityAltar = 1 << 8,
    MajorShrine = 1 << 9,
    MinorShrine = 1 << 10,
    RitualShrine = 1 << 11,
    SmallFountain = 1 << 12,
    BigFountain = 1 << 13,
    SkillPedestal = 1 << 14,
    RacePedestal = 1 << 15,
    TraitPedestal = 1 << 16,
    FeatPedestal = 1 << 17,
    ArmorStand = 1 << 18,
    GildedArmorStand = 1 << 19,
    ItemCase = 1 << 20,
    GildedItemCase = 1 << 21,
    WeaponStand = 1 << 22,
    GildedWeaponStand = 1 << 23
}
