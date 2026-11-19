using UnicornHack.Systems.Levels;

namespace UnicornHack.Data.Features;

public static partial class FeatureData
{
    public static readonly Feature RitualShrine = new Feature { Name = "ritual shrine", RoomFeature = RoomFeatures.RitualShrine, RewardType = RewardType.Ritual };
}
