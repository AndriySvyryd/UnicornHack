using System.Collections.Generic;
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
            Weight = 2000,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 16}}
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
                {"cold resistance", 3},
                {"fire resistance", 3},
                {"electricity resistance", 3},
                {"poison resistance", 3},
                {"venom resistance", 3},
                {"thick hide", 3},
                {"health point maximum", 60},
                {"largeness", Size.Large},
                {"physical deflection", 16},
                {"magic resistance", 50}
            },
            InitialLevel = 14,
            CorpseName = ""
        };
    }
}