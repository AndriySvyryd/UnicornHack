using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant BlackUnicorn = new CreatureVariant
        {
            Name = "black unicorn",
            Species = Species.Unicorn,
            SpeciesClass = SpeciesClass.Quadrupedal,
            MovementDelay = 50,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Headbutt,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 60}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Kick,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                }
            },
            SimpleProperties = new HashSet<string> {"animal body", "infravisibility", "handlessness"},
            ValuedProperties = new Dictionary<string, object>
            {
                {
                    "poison resistance",
                    75
                },
                {
                    "venom resistance",
                    75
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
                    70
                },
                {
                    "weight",
                    1300
                }
            },
            InitialLevel = 4,
            GenerationWeight = new DefaultWeight {Multiplier = 6F},
            Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.RangedPeaceful | MonsterBehavior.Wandering |
                       MonsterBehavior.GemCollector,
            Noise = ActorNoiseType.Neigh
        };
    }
}