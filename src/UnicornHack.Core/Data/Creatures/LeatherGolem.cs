using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant LeatherGolem = new CreatureVariant
        {
            Name = "leather golem",
            Species = Species.Golem,
            MovementDelay = 200,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
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
                    "venom resistance",
                    75
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
                    14
                },
                {
                    "weight",
                    800
                }
            },
            InitialLevel = 6,
            CorpseName = ""
        };
    }
}