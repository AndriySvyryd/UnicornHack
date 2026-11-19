using UnicornHack.Systems.Levels;

namespace UnicornHack.Data.Features;

public static partial class FeatureData
{
    public static readonly Feature GildedWeaponStand = new Feature
    {
        Name = "gilded weapon stand",
        RoomFeature = RoomFeatures.GildedWeaponStand,
        RewardType = RewardType.Item,
        ItemRewardType = ItemType.Weapon
    };
}
