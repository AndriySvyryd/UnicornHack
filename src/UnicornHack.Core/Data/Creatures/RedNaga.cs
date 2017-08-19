using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant RedNaga = new CreatureVariant
        {
            Name = "red naga",
            Species = Species.Naga,
            SpeciesClass = SpeciesClass.Aberration,
            MovementDelay = 100,
            Weight = 1500,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Spit,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new FireDamage {Damage = 7}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "infravision",
                    "serpentlike body",
                    "limblessness",
                    "oviparity",
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
                    {"largeness", Size.Huge},
                    {"physical deflection", 16}
                },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            PreviousStageName = "red naga hatchling",
            Noise = ActorNoiseType.Hiss
        };
    }
}