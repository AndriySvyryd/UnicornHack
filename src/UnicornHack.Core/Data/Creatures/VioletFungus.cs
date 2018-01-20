using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant VioletFungus = new CreatureVariant
        {
            Name = "violet fungus",
            Species = Species.Fungus,
            MovementDelay = 1200,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Touch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Poison {Damage = 30}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Touch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Stick()}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new Poison {Damage = 30}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "sleep resistance",
                "breathlessness",
                "non animal",
                "eyelessness",
                "limblessness",
                "headlessness",
                "mindlessness",
                "asexuality",
                "no inventory"
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
                    13
                },
                {
                    "weight",
                    100
                }
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 2F}
        };
    }
}