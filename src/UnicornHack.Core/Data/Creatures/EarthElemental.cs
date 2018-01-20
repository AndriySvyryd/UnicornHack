using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant EarthElemental = new CreatureVariant
        {
            Name = "earth elemental",
            Species = Species.Elemental,
            SpeciesClass = SpeciesClass.Extraplanar,
            MovementDelay = 200,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 140}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Stun {Duration = 2}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "sleep resistance",
                "phasing",
                "non animal",
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
            ValuedProperties = new Dictionary<string, object>
            {
                {
                    "fire resistance",
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
                    "thick hide",
                    3
                },
                {
                    "size",
                    16
                },
                {
                    "physical deflection",
                    18
                },
                {
                    "magic resistance",
                    30
                },
                {
                    "weight",
                    2500
                }
            },
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            CorpseName = ""
        };
    }
}