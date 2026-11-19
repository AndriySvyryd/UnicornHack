using UnicornHack.Systems.Levels;

namespace UnicornHack.Data.Features;

public static partial class FeatureData
{
    public static readonly Feature FeatPedestal = new Feature
    {
        Name = "feat pedestal",
        RoomFeature = RoomFeatures.FeatPedestal,
        RewardType = RewardType.Ability,
        AbilityRewardType = AbilityType.Feat
    };
}
