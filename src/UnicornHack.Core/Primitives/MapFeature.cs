namespace UnicornHack.Primitives;

public enum MapFeature : byte
{
    Default = 0,
    RockFloor = 1,
    StoneFloor = 2,
    RockWall = 3,
    StoneWall = 4,
    StoneArchway = 5,
    Pool = 6,
    DeityAltar = 7,
    InactiveDeityAltar = 8,
    MajorShrine = 9,
    InactiveMajorShrine = 10,
    MinorShrine = 11,
    InactiveMinorShrine = 12,
    RitualShrine = 13,
    InactiveRitualShrine = 14,
    SmallFountain = 15,
    InactiveSmallFountain = 16,
    BigFountain = 17,
    InactiveBigFountain = 18,
    SkillPedestal = 19,
    InactiveSkillPedestal = 20,
    RacePedestal = 21,
    InactiveRacePedestal = 22,
    TraitPedestal = 23,
    InactiveTraitPedestal = 24,
    FeatPedestal = 25,
    InactiveFeatPedestal = 26,
    ArmorStand = 27,
    InactiveArmorStand = 28,
    ItemCase = 29,
    InactiveItemCase = 30,
    WeaponStand = 31,
    InactiveWeaponStand = 32,
    Unexplored = 254
}

public static class MapFeatureExtensions
{
    // TODO: use locomotion type mask (walking, flying, swimming, heavy, phasing)
    public static bool CanMoveTo(this MapFeature feature)
    {
        switch (feature)
        {
            case MapFeature.StoneFloor:
            case MapFeature.RockFloor:
            case MapFeature.StoneArchway:
            case MapFeature.Pool:
                return true;
            default:
                return false;
        }
    }
}
