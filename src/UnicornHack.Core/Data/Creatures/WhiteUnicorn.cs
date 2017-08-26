using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant WhiteUnicorn = new CreatureVariant
        {
            Name = "white unicorn",
            Species = Species.Unicorn,
            SpeciesClass = SpeciesClass.Quadrupedal,
            MovementDelay = 50,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Headbutt,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 6}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Kick,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 3}}
                    }
                },
            SimpleProperties = new HashSet<string> {"animal body", "infravisibility", "handlessness"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"venom resistance", 3},
                    {"size", 8},
                    {"physical deflection", 18},
                    {"magic resistance", 70},
                    {"weight", 1300}
                },
            InitialLevel = 4,
            GenerationWeight = new DefaultWeight {Multiplier = 6F},
            Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.RangedPeaceful | MonsterBehavior.Wandering |
                       MonsterBehavior.GemCollector,
            Noise = ActorNoiseType.Neigh
        };
    }
}