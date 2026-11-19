using UnicornHack.Systems.Levels;

namespace UnicornHack.Data.Features;

public static partial class FeatureData
{
    public static readonly Feature DeityAltar = new Feature
    {
        Name = "deity altar",
        RoomFeature = RoomFeatures.DeityAltar,
        RewardType = RewardType.Effect,
        AbilityRewardType = AbilityType.Faith
    };
}
