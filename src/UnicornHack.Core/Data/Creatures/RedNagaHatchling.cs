using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant RedNagaHatchling = new CreatureVariant
        {
            Name = "red naga hatchling",
            Species = Species.Naga,
            SpeciesClass = SpeciesClass.Aberration,
            MovementDelay = 120,
            Weight = 500,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "infravision",
                    "serpentlike body",
                    "limblessness",
                    "singular inventory",
                    "sliming resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"fire resistance", 3},
                    {"poison resistance", 3},
                    {"venom resistance", 3},
                    {"thick hide", 3},
                    {"physical deflection", 14}
                },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            NextStageName = "red naga",
            Noise = ActorNoiseType.Hiss
        };
    }
}