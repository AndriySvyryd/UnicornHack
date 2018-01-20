using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant LongWorm = new CreatureVariant
        {
            Name = "long worm",
            Species = Species.Worm,
            SpeciesClass = SpeciesClass.Vermin,
            MovementDelay = 400,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 50}}
                }
            },
            SimpleProperties =
                new HashSet<string> {"serpentlike body", "eyelessness", "limblessness", "oviparity", "no inventory"},
            ValuedProperties = new Dictionary<string, object>
            {
                {"poison resistance", 75},
                {"size", 32},
                {"physical deflection", 15},
                {"magic resistance", 10},
                {"weight", 1500}
            },
            InitialLevel = 9,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            PreviousStageName = "baby long worm"
        };
    }
}