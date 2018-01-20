using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant EnergyVortex = new CreatureVariant
        {
            Name = "energy vortex",
            Species = Species.Vortex,
            SpeciesClass = SpeciesClass.Extraplanar,
            MovementDelay = 60,
            Abilities = new HashSet<AbilityDefinition>
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
                    Effects = new HashSet<Effect> {new Shock {Damage = 30}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnDigestion,
                    Action = AbilityAction.Digestion,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new DrainEnergy {Amount = 3}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnMeleeHit,
                    Effects = new HashSet<Effect> {new Shock {Damage = 30}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnRangedHit,
                    Effects = new HashSet<Effect> {new Shock {Damage = 30}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "sleep resistance",
                "flight",
                "flight control",
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
            ValuedProperties = new Dictionary<string, object>
            {
                {
                    "electricity resistance",
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
                    0
                }
            },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            CorpseName = ""
        };
    }
}