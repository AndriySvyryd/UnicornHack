using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant PaperGolem = new CreatureVariant
        {
            Name = "paper golem",
            Species = Species.Golem,
            MovementDelay = 100,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
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
                    "sickness resistance"
                },
            ValuedProperties = new Dictionary<string, object>
            {
                {"water weakness", 3},
                {"cold resistance", 3},
                {"poison resistance", 3},
                {"venom resistance", 3},
                {"health point maximum", 20},
                {"size", 8},
                {"physical deflection", 10},
                {"weight", 400}
            },
            InitialLevel = 2,
            CorpseName = ""
        };
    }
}