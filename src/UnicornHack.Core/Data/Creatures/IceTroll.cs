using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant IceTroll = new CreatureVariant
        {
            Name = "ice troll",
            Species = Species.Troll,
            MovementDelay = 120,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 70}}
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
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 70}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 70}}
                }
            },
            SimpleProperties = new HashSet<string> {"infravision", "infravisibility", "humanoidness", "reanimation"},
            ValuedProperties = new Dictionary<string, object>
            {
                {
                    "cold resistance",
                    75
                },
                {
                    "regeneration",
                    3
                },
                {
                    "size",
                    8
                },
                {
                    "physical deflection",
                    18
                },
                {
                    "magic resistance",
                    20
                },
                {
                    "weight",
                    1000
                }
            },
            InitialLevel = 9,
            GenerationWeight = new BranchWeight {NotMatched = new DefaultWeight {Multiplier = 3F}, Name = "hell"},
            Behavior = MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Grunt
        };
    }
}