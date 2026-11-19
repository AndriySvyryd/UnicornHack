using UnicornHack.Systems.Levels;

namespace UnicornHack.Data.Features;

public static partial class FeatureData
{
    public static readonly Feature ItemCase = new Feature
    {
        Name = "item case",
        RoomFeature = RoomFeatures.ItemCase,
        RewardType = RewardType.Item,
        ItemRewardType = ItemType.Potion | ItemType.Orb | ItemType.Figurine | ItemType.Trinket
    };
}
