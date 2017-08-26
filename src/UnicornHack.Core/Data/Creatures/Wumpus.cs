using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Wumpus = new CreatureVariant
        {
            Name = "wumpus",
            Species = Species.Quadruped,
            SpeciesClass = SpeciesClass.Aberration,
            MovementDelay = 400,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "clinginess",
                    "animal body",
                    "infravisibility",
                    "handlessness",
                    "singular inventory"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"size", 8},
                    {"physical deflection", 18},
                    {"magic resistance", 10},
                    {"weight", 2500}
                },
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 6F},
            Noise = ActorNoiseType.Burble
        };
    }
}