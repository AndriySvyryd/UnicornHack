using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant OchreJelly = new CreatureVariant
        {
            Name = "ochre jelly",
            Species = Species.Jelly,
            MovementDelay = 400,
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
                    Effects = new HashSet<Effect> {new Corrode {Damage = 100}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnMeleeHit,
                    Effects = new HashSet<Effect> {new Corrode {Damage = 70}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new Corrode {Damage = 70}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "sleep resistance",
                "breathlessness",
                "amorphism",
                "non animal",
                "eyelessness",
                "limblessness",
                "headlessness",
                "mindlessness",
                "asexuality",
                "no inventory",
                "stoning resistance"
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
                    "acid resistance",
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
                    12
                },
                {
                    "magic resistance",
                    20
                },
                {
                    "weight",
                    100
                }
            },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 5F}
        };
    }
}