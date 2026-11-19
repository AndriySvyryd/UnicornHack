using UnicornHack.Systems.Levels;

namespace UnicornHack.Data.Features;

public static partial class FeatureData
{
    public static readonly Feature ArmorStand = new Feature { Name = "armor stand", RoomFeature = RoomFeatures.ArmorStand, RewardType = RewardType.Item, ItemRewardType = ItemType.Armor };
}
