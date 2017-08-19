using System.Collections.Generic;
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
            Weight = 150,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new VenomDamage {Damage = 3}}
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
                    {"largeness", Size.Small},
                    {"physical deflection", 17}
                },
            InitialLevel = 4,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.LargeGroup,
            Noise = ActorNoiseType.Hiss
        };
    }
}