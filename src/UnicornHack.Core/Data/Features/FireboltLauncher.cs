using UnicornHack.Systems.Features;

namespace UnicornHack.Data.Features;

public static partial class FeatureData
{
    public static readonly Feature FireboltLauncher = new Feature
    {
        Name = "firebolt launcher",
        EnglishDescription = "Fires a bolt in each cardinal direction after a short delay.",
        RoomFeature = RoomFeatures.Trapped,
        Behavior = FeatureBehavior.BlocksMovement | FeatureBehavior.TriggerOnInterval,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "launcher fire bolt",
                Type = AbilityType.RangedAttack,
                Windup = 200,
                Range = 8,
                Action = AbilityAction.Spell,
                Delay = "100",
                TargetingShape = TargetingShape.Plus,
                Effects = new List<Effect> { new FireDamage { Amount = "20" } }
            }
        }
    };
}
