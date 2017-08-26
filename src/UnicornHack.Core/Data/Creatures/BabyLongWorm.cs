using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant BabyLongWorm = new CreatureVariant
        {
            Name = "baby long worm",
            Species = Species.Worm,
            SpeciesClass = SpeciesClass.Vermin,
            MovementDelay = 400,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    }
                },
            SimpleProperties = new HashSet<string> {"serpentlike body", "eyelessness", "limblessness", "no inventory"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"stealthiness", 3},
                    {"physical deflection", 15},
                    {"weight", 600}
                },
            InitialLevel = 2,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            NextStageName = "long worm"
        };
    }
}