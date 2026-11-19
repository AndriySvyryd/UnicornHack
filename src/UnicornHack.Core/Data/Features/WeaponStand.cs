using UnicornHack.Systems.Levels;

namespace UnicornHack.Data.Features;

public static partial class FeatureData
{
    public static readonly Feature WeaponStand = new Feature
    {
        Name = "weapon stand",
        RoomFeature = RoomFeatures.WeaponStand,
        RewardType = RewardType.Item,
        ItemRewardType = ItemType.WeaponMelee | ItemType.WeaponRangedClose | ItemType.WeaponRangedShort | ItemType.WeaponRangedMedium | ItemType.WeaponRangedLong
    };
}
