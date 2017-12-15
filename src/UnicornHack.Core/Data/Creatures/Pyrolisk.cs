using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Pyrolisk = new CreatureVariant
        {
            Name = "pyrolisk",
            Species = Species.Cockatrice,
            SpeciesClass = SpeciesClass.MagicalBeast,
            MovementDelay = 200,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Gaze,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Burn {Damage = 70}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "animal body",
                "infravisibility",
                "handlessness",
                "oviparity",
                "singular inventory"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {
                    "poison resistance",
                    3
                },
                {
                    "fire resistance",
                    3
                },
                {
                    "size",
                    2
                },
                {
                    "physical deflection",
                    14
                },
                {
                    "magic resistance",
                    30
                },
                {
                    "weight",
                    30
                }
            },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Noise = ActorNoiseType.Hiss
        };
    }
}