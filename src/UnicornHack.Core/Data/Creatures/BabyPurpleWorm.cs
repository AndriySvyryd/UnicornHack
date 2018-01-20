using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant BabyPurpleWorm = new CreatureVariant
        {
            Name = "baby purple worm",
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
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                }
            },
            SimpleProperties = new HashSet<string> {"serpentlike body", "eyelessness", "limblessness", "no inventory"},
            ValuedProperties = new Dictionary<string, object>
            {
                {"poison resistance", 75},
                {"stealthiness", 3},
                {"physical deflection", 15},
                {"weight", 600}
            },
            InitialLevel = 4,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            NextStageName = "purple worm"
        };
    }
}