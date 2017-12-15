using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant WaterTroll = new CreatureVariant
        {
            Name = "water troll",
            Species = Species.Troll,
            MovementDelay = 85,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 90}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Claw,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 90}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 70}}
                }
            },
            SimpleProperties =
                new HashSet<string> {"swimming", "infravision", "infravisibility", "humanoidness", "reanimation"},
            ValuedProperties = new Dictionary<string, object>
            {
                {"regeneration", 3},
                {"size", 8},
                {"physical deflection", 16},
                {"magic resistance", 40},
                {"weight", 1200}
            },
            InitialLevel = 11,
            GenerationWeight = new BranchWeight {NotMatched = new DefaultWeight(), Name = "hell"},
            Behavior = MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Grunt
        };
    }
}