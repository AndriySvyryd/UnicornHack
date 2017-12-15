using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant BlackNagaHatchling = new CreatureVariant
        {
            Name = "black naga hatchling",
            Species = Species.Naga,
            SpeciesClass = SpeciesClass.Aberration,
            MovementDelay = 120,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 20}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "infravision",
                "serpentlike body",
                "limblessness",
                "singular inventory",
                "stoning resistance"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {
                    "acid resistance",
                    3
                },
                {
                    "poison resistance",
                    3
                },
                {
                    "venom resistance",
                    3
                },
                {
                    "thick hide",
                    3
                },
                {
                    "physical deflection",
                    14
                },
                {
                    "weight",
                    500
                }
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            NextStageName = "black naga",
            Noise = ActorNoiseType.Hiss
        };
    }
}