using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GasSpore = new CreatureVariant
        {
            Name = "gas spore",
            Species = Species.FloatingSphere,
            MovementDelay = 400,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Explosion,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 140}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Explosion,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Deafen {Duration = 27}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "sleep resistance",
                "flight",
                "flight control",
                "non animal",
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
                    "physical deflection",
                    10
                },
                {
                    "weight",
                    10
                }
            },
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            CorpseName = ""
        };
    }
}