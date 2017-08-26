using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GoldGolem = new CreatureVariant
        {
            Name = "gold golem",
            Species = Species.Golem,
            MovementDelay = 133,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
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
                {"acid resistance", 3},
                {"cold resistance", 3},
                {"poison resistance", 3},
                {"venom resistance", 3},
                {"thick hide", 3},
                {"health point maximum", 40},
                {"physical deflection", 14},
                {"weight", 2000}
            },
            InitialLevel = 5,
            CorpseName = ""
        };
    }
}