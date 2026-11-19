using UnicornHack.Systems.Levels;

namespace UnicornHack.Data.Features;

public static partial class FeatureData
{
    public static readonly Feature MinorShrine = new Feature
    {
        Name = "minor shrine",
        RoomFeature = RoomFeatures.MinorShrine,
        RewardType = RewardType.Effect,
        AbilityRewardType = AbilityType.Buff
    };
}
