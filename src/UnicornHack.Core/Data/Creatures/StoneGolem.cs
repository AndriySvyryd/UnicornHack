using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant StoneGolem = new CreatureVariant
        {
            Name = "stone golem",
            Species = Species.Golem,
            MovementDelay = 200,
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
                "stoning resistance",
                "sliming resistance",
                "sickness resistance"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {
                    "cold resistance",
                    75
                },
                {
                    "fire resistance",
                    75
                },
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
                    60
                },
                {
                    "size",
                    8
                },
                {
                    "physical deflection",
                    16
                },
                {
                    "magic resistance",
                    50
                },
                {
                    "weight",
                    2000
                }
            },
            InitialLevel = 14,
            CorpseName = ""
        };
    }
}