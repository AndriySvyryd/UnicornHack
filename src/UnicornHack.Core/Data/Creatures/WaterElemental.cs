using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant WaterElemental = new CreatureVariant
        {
            Name = "water elemental",
            Species = Species.Elemental,
            SpeciesClass = SpeciesClass.Extraplanar,
            MovementDelay = 200,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 17}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Soak {Damage = 3}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnMeleeHit,
                        Effects = new HashSet<Effect> {new Soak {Damage = 3}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "swimming",
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
                    {"poison resistance", 3},
                    {"venom resistance", 3},
                    {"size", 16},
                    {"physical deflection", 18},
                    {"magic resistance", 30},
                    {"weight", 2500}
                },
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            CorpseName = ""
        };
    }
}