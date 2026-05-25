namespace UnicornHack.Primitives;

[Flags]
public enum RoomFeatures
{
    Normal = 0,
    Entrance = 1 << 0,
    Exit = 1 << 1,
    Secret = 1 << 2,
    Cursed = 1 << 3,
    SkillPedestal = 1 << 4,
    TraitPedestal = 1 << 5,
    FeatPedestal = 1 << 6,
    RacePedestal = 1 << 7,
    DeityAltar = 1 << 8,
    WeaponStand = 1 << 9,
    ArmorStand = 1 << 10,
    ItemCase = 1 << 11,
    SmallFountain = 1 << 12,
    BigFountain = 1 << 13,
    MinorShrine = 1 << 14,
    MajorShrine = 1 << 15,
    RitualShrine = 1 << 16,
    Challenge = 1 << 17,
    Boss = 1 << 18,
    Trapped = 1 << 19
}
