using UnicornHack.Systems.Levels;

namespace UnicornHack.Data.Features;

public static partial class FeatureData
{
    public static readonly Feature BigFountain = new Feature
    {
        Name = "big fountain",
        RoomFeature = RoomFeatures.BigFountain,
        RewardType = RewardType.Effect,
        AbilityRewardType = AbilityType.Buff
    };
}
