using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant WoodGolem = new CreatureVariant
        {
            Name = "wood golem",
            Species = Species.Golem,
            MovementDelay = 400,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 70}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "sleep resistance",
                "non animal",
                "breathlessness",
                "mindlessness",
                "humanoidness",
                "asexuality"
            },
            ValuedProperties = new Dictionary<string, object>
            {
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
                    50
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
                    "weight",
                    1000
                }
            },
            InitialLevel = 7,
            CorpseName = ""
        };
    }
}