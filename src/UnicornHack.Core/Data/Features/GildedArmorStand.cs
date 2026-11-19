using UnicornHack.Systems.Levels;

namespace UnicornHack.Data.Features;

public static partial class FeatureData
{
    public static readonly Feature GildedArmorStand = new Feature
    {
        Name = "gilded armor stand",
        RoomFeature = RoomFeatures.GildedArmorStand,
        RewardType = RewardType.Item,
        ItemRewardType = ItemType.Armor
    };
}