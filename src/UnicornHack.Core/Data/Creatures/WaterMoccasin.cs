using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant WaterMoccasin = new CreatureVariant
        {
            Name = "water moccasin",
            Species = Species.Snake,
            SpeciesClass = SpeciesClass.Reptile,
            MovementDelay = 80,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Envenom {Damage = 30}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "swimming",
                "concealment",
                "infravision",
                "serpentlike body",
                "limblessness",
                "oviparity",
                "no inventory"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {"poison resistance", 75},
                {"venom resistance", 75},
                {"size", 2},
                {"physical deflection", 17},
                {"weight", 150}
            },
            InitialLevel = 4,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.LargeGroup,
            Noise = ActorNoiseType.Hiss
        };
    }
}