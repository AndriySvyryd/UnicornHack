using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GrayOoze = new CreatureVariant
        {
            Name = "gray ooze",
            Species = Species.Ooze,
            MovementDelay = 1200,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 9}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Soak {Damage = 9}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnMeleeHit,
                        Effects = new HashSet<Effect> {new Soak {Damage = 4}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "decay resistance",
                    "breathlessness",
                    "amorphism",
                    "non animal",
                    "eyelessness",
                    "limblessness",
                    "headlessness",
                    "mindlessness",
                    "asexuality",
                    "stoning resistance"
                },
            ValuedProperties = new Dictionary<string, object>
            {
                {"fire resistance", 3},
                {"cold resistance", 3},
                {"poison resistance", 3},
                {"venom resistance", 3},
                {"acid resistance", 3},
                {"stealthiness", 3},
                {"physical deflection", 12},
                {"weight", 500}
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            CorpseName = ""
        };
    }
}