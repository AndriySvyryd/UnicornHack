using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant BlackNaga = new CreatureVariant
        {
            Name = "black naga",
            Species = Species.Naga,
            SpeciesClass = SpeciesClass.Aberration,
            MovementDelay = 85,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 70}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Spit,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Corrode {Damage = 70}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "infravision",
                "serpentlike body",
                "limblessness",
                "oviparity",
                "singular inventory",
                "stoning resistance"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {
                    "acid resistance",
                    75
                },
                {
                    "poison resistance",
                    75
                },
                {
                    "venom resistance",
                    75
                },
                {
                    "thick hide",
                    3
                },
                {
                    "size",
                    16
                },
                {
                    "physical deflection",
                    18
                },
                {
                    "magic resistance",
                    10
                },
                {
                    "weight",
                    1500
                }
            },
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            PreviousStageName = "black naga hatchling",
            Noise = ActorNoiseType.Hiss
        };
    }
}