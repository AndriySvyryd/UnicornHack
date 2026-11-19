using UnicornHack.Systems.Levels;

namespace UnicornHack.Data.Features;

public static partial class FeatureData
{
    public static readonly Feature MajorShrine = new Feature
    {
        Name = "major shrine",
        RoomFeature = RoomFeatures.MajorShrine,
        RewardType = RewardType.Effect,
        AbilityRewardType = AbilityType.Buff
    };
}
