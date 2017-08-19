using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GarterSnake = new CreatureVariant
        {
            Name = "garter snake",
            Species = Species.Snake,
            SpeciesClass = SpeciesClass.Reptile,
            MovementDelay = 150,
            Weight = 50,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
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
            ValuedProperties = new Dictionary<string, object> {{"largeness", Size.Tiny}, {"physical deflection", 12}},
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            GenerationFlags = GenerationFlags.LargeGroup,
            Noise = ActorNoiseType.Hiss
        };
    }
}