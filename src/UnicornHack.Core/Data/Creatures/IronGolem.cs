using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant IronGolem = new CreatureVariant
        {
            Name = "iron golem",
            Species = Species.Golem,
            MovementDelay = 200,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 220}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Breath,
                    Timeout = 5,
                    Effects = new HashSet<Effect> {new Poison {Damage = 140}}
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
                    "water resistance",
                    50
                },
                {
                    "cold resistance",
                    75
                },
                {
                    "fire resistance",
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
                    80
                },
                {
                    "size",
                    8
                },
                {
                    "physical deflection",
                    17
                },
                {
                    "magic resistance",
                    60
                },
                {
                    "weight",
                    2000
                }
            },
            InitialLevel = 18,
            CorpseName = ""
        };
    }
}