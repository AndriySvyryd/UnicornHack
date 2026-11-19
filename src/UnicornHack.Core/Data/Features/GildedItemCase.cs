using UnicornHack.Systems.Levels;

namespace UnicornHack.Data.Features;

public static partial class FeatureData
{
    public static readonly Feature GildedItemCase = new Feature
    {
        Name = "gilded item case",
        RoomFeature = RoomFeatures.GildedItemCase,
        RewardType = RewardType.Item,
        ItemRewardType = ItemType.Potion | ItemType.Orb | ItemType.Figurine | ItemType.Trinket
    };
}
