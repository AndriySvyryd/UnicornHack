using UnicornHack.Systems.Features;

namespace UnicornHack.Data.Features;

public static partial class FeatureData
{
    public static readonly Feature SpikeTrap = new Feature
    {
        Name = "spike trap",
        EnglishDescription = "Steel spikes strike creatures entering this tile.",
        RoomFeature = RoomFeatures.Trapped,
        Behavior = FeatureBehavior.TriggerOnEntry,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "spike trap attack",
                Type = AbilityType.MeleeAttack,
                TargetingShape = TargetingShape.Square,
                Action = AbilityAction.Hit,
                Cooldown = 100,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "15", ArmorPenetration = "2" } }
            }
        }
    };
}
