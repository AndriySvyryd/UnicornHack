using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GlassGolem = new CreatureVariant
        {
            Name = "glass golem",
            Species = Species.Golem,
            MovementDelay = 200,
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
                "reflection",
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
                    "acid resistance",
                    3
                },
                {
                    "cold resistance",
                    3
                },
                {
                    "fire resistance",
                    3
                },
                {
                    "electricity resistance",
                    3
                },
                {
                    "poison resistance",
                    3
                },
                {
                    "venom resistance",
                    3
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
                    1800
                }
            },
            InitialLevel = 16,
            CorpseName = ""
        };
    }
}