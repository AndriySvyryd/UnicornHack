using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Lichen = new CreatureVariant
        {
            Name = "lichen",
            Species = Species.Fungus,
            MovementDelay = 1200,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Touch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Stick()}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnMeleeHit,
                    Effects = new HashSet<Effect> {new Stick()}
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
                    11
                },
                {
                    "weight",
                    20
                }
            },
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 5F}
        };
    }
}