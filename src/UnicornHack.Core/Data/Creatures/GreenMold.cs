using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GreenMold = new CreatureVariant
        {
            Name = "green mold",
            Species = Species.Fungus,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnMeleeHit,
                    Effects = new HashSet<Effect> {new Corrode {Damage = 30}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new Corrode {Damage = 20}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "sleep resistance",
                "decay resistance",
                "breathlessness",
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
                    "acid resistance",
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
                    "physical deflection",
                    11
                },
                {
                    "weight",
                    50
                }
            },
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 2F}
        };
    }
}