using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant StrawGolem = new CreatureVariant
        {
            Name = "straw golem",
            Species = Species.Golem,
            MovementDelay = 100,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "sleep resistance",
                "non animal",
                "breathlessness",
                "mindlessness",
                "humanoidness",
                "asexuality",
                "stoning resistance",
                "sickness resistance"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {
                    "cold resistance",
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
                    "hit point maximum",
                    20
                },
                {
                    "size",
                    8
                },
                {
                    "physical deflection",
                    10
                },
                {
                    "weight",
                    400
                }
            },
            InitialLevel = 3,
            CorpseName = ""
        };
    }
}