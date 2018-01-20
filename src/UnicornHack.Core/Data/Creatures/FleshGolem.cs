using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant FleshGolem = new CreatureVariant
        {
            Name = "flesh golem",
            Species = Species.Golem,
            MovementDelay = 150,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 90}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 90}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "sleep resistance",
                "breathlessness",
                "mindlessness",
                "humanoidness",
                "asexuality"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {
                    "poison resistance",
                    75
                },
                {
                    "regeneration",
                    3
                },
                {
                    "hit point maximum",
                    40
                },
                {
                    "size",
                    8
                },
                {
                    "physical deflection",
                    11
                },
                {
                    "weight",
                    1400
                }
            },
            InitialLevel = 9
        };
    }
}