using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant ClayGolem = new CreatureVariant
        {
            Name = "clay golem",
            Species = Species.Golem,
            MovementDelay = 171,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 160}}
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
                    "poison resistance",
                    75
                },
                {
                    "venom resistance",
                    75
                },
                {
                    "thick hide",
                    3
                },
                {
                    "hit point maximum",
                    50
                },
                {
                    "size",
                    8
                },
                {
                    "physical deflection",
                    13
                },
                {
                    "magic resistance",
                    40
                },
                {
                    "weight",
                    1500
                }
            },
            InitialLevel = 11,
            CorpseName = ""
        };
    }
}