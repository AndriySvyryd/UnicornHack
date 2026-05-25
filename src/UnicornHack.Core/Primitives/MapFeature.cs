namespace UnicornHack.Primitives;

public enum MapFeature : byte
{
    Default = 0,
    RockFloor = 1,
    StoneFloor = 2,
    RockWall = 3,
    StoneWall = 4,
    StoneArchway = 5,
    DeityAltar = 6,
    MajorShrine = 7,
    MinorShrine = 8,
    RitualShrine = 9,
    SmallFountain = 10,
    BigFountain = 11,
    SkillPedestal = 12,
    RacePedestal = 13,
    Pool = 14,
    TraitPedestal = 15,
    FeatPedestal = 16,
    ArmorStand = 17,
    ItemCase = 18,
    WeaponStand = 19,
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
