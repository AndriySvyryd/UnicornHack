using UnicornHack.Systems.Levels;

namespace UnicornHack.Data.Features;

public static partial class FeatureData
{
    public static readonly Feature SmallFountain = new Feature
    {
        Name = "small fountain",
        RoomFeature = RoomFeatures.SmallFountain,
        RewardType = RewardType.Effect,
        AbilityRewardType = AbilityType.Buff
    };
}
