using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Snake = new CreatureVariant
        {
            Name = "snake",
            Species = Species.Snake,
            SpeciesClass = SpeciesClass.Reptile,
            MovementDelay = 80,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Envenom {Damage = 3}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "swimming",
                    "concealment",
                    "infravision",
                    "serpentlike body",
                    "limblessness",
                    "oviparity",
                    "no inventory"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"venom resistance", 3},
                    {"size", 2},
                    {"physical deflection", 17},
                    {"weight", 100}
                },
            InitialLevel = 4,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            Noise = ActorNoiseType.Hiss
        };
    }
}