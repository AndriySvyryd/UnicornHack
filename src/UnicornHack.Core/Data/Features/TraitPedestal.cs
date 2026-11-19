using UnicornHack.Systems.Levels;

namespace UnicornHack.Data.Features;

public static partial class FeatureData
{
    public static readonly Feature TraitPedestal = new Feature
    {
        Name = "trait pedestal",
        RoomFeature = RoomFeatures.TraitPedestal,
        RewardType = RewardType.Ability,
        AbilityRewardType = AbilityType.Trait
    };
}
