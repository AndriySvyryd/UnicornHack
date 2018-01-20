using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant QuiveringBlob = new CreatureVariant
        {
            Name = "quivering blob",
            Species = Species.Blob,
            MovementDelay = 1200,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Touch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 40}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "sleep resistance",
                "decay resistance",
                "breathlessness",
                "amorphism",
                "non animal",
                "eyelessness",
                "limblessness",
                "headlessness",
                "mindlessness",
                "asexuality"
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
                    12
                },
                {
                    "weight",
                    200
                }
            },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 6F},
            Behavior = MonsterBehavior.Wandering
        };
    }
}