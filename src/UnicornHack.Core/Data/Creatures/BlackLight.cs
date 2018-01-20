using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant BlackLight = new CreatureVariant
        {
            Name = "black light",
            Species = Species.FloatingSphere,
            SpeciesClass = SpeciesClass.Extraplanar,
            MovementDelay = 80,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Explosion,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Confuse {Duration = 27}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "sleep resistance",
                "flight",
                "flight control",
                "infravisibility",
                "invisibility detection",
                "non animal",
                "non solid body",
                "breathlessness",
                "limblessness",
                "eyelessness",
                "headlessness",
                "mindlessness",
                "asexuality",
                "no inventory",
                "sliming resistance",
                "sickness resistance"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {
                    "fire resistance",
                    75
                },
                {
                    "cold resistance",
                    75
                },
                {
                    "electricity resistance",
                    75
                },
                {
                    "acid resistance",
                    75
                },
                {
                    "disintegration resistance",
                    75
                },
                {
                    "poison resistance",
                    75
                },
                {
                    "venom resistance",
                    75
                },
                {
                    "stealthiness",
                    3
                },
                {
                    "size",
                    2
                },
                {
                    "weight",
                    0
                }
            },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            CorpseName = ""
        };
    }
}