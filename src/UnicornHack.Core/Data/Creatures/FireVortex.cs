using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant FireVortex = new CreatureVariant
        {
            Name = "fire vortex",
            Species = Species.Vortex,
            SpeciesClass = SpeciesClass.Extraplanar,
            MovementDelay = 54,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Engulf {Duration = 4}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnDigestion,
                        Action = AbilityAction.Digestion,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Burn {Damage = 5}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnMeleeHit,
                        Effects = new HashSet<Effect> {new Burn {Damage = 5}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnRangedHit,
                        Effects = new HashSet<Effect> {new Burn {Damage = 5}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "flight",
                    "flight control",
                    "infravisibility",
                    "non animal",
                    "non solid body",
                    "breathlessness",
                    "limblessness",
                    "eyelessness",
                    "headlessness",
                    "mindlessness",
                    "asexuality",
                    "stoning resistance",
                    "sliming resistance",
                    "sickness resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"fire resistance", 3},
                    {"acid resistance", 3},
                    {"poison resistance", 3},
                    {"venom resistance", 3},
                    {"size", 16},
                    {"physical deflection", 18},
                    {"magic resistance", 30},
                    {"weight", 0}
                },
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            CorpseName = ""
        };
    }
}