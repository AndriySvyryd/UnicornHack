using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Cobra = new CreatureVariant
        {
            Name = "cobra",
            Species = Species.Snake,
            SpeciesClass = SpeciesClass.Reptile,
            MovementDelay = 66,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Envenom {Damage = 5}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Spit,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Blind {Duration = 5}}
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
                    {"physical deflection", 18},
                    {"weight", 250}
                },
            InitialLevel = 7,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            Noise = ActorNoiseType.Hiss
        };
    }
}