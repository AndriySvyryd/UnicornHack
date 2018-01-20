using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GoldenNagaHatchling = new CreatureVariant
        {
            Name = "golden naga hatchling",
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
                "invisibility detection",
                "infravision",
                "serpentlike body",
                "limblessness",
                "singular inventory"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {"poison resistance", 75},
                {"venom resistance", 75},
                {"thick hide", 3},
                {"physical deflection", 14},
                {"weight", 500}
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            NextStageName = "golden naga",
            Noise = ActorNoiseType.Hiss
        };
    }
}