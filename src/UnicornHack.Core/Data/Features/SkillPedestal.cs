using UnicornHack.Systems.Levels;

namespace UnicornHack.Data.Features;

public static partial class FeatureData
{
    public static readonly Feature SkillPedestal = new Feature
    {
        Name = "skill pedestal",
        RoomFeature = RoomFeatures.SkillPedestal,
        RewardType = RewardType.Ability,
        AbilityRewardType = AbilityType.Skill
    };
}
