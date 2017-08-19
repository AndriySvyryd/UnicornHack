using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant RopeGolem = new CreatureVariant
        {
            Name = "rope golem",
            Species = Species.Golem,
            MovementDelay = 100,
            Weight = 450,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Hug,
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
                {"cold resistance", 3},
                {"poison resistance", 3},
                {"venom resistance", 3},
                {"health point maximum", 30},
                {"largeness", Size.Large},
                {"physical deflection", 12}
            },
            InitialLevel = 4,
            CorpseName = ""
        };
    }
}