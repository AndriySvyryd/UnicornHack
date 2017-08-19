using System.Collections.Generic;
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
            Weight = 2000,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 22}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Breath,
                        Timeout = 5,
                        Effects = new HashSet<Effect> {new PoisonDamage {Damage = 14}}
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
                {"water weakness", 3},
                {"cold resistance", 3},
                {"fire resistance", 3},
                {"poison resistance", 3},
                {"venom resistance", 3},
                {"thick hide", 3},
                {"health point maximum", 80},
                {"largeness", Size.Large},
                {"physical deflection", 17},
                {"magic resistance", 60}
            },
            InitialLevel = 18,
            CorpseName = ""
        };
    }
}